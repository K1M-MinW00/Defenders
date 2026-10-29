using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine.Localization.Settings;

public sealed class EquipmentSystemTests
{
    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        GameConfig.Initialize();
        LocalizationSettings.InitializationOperation.WaitForCompletion();
    }

    [SetUp]
    public void SetUp()
    {
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.GetLocale("ko");
    }

    [Test]
    public void Equip_SwapsSameSlotAndKeepsSingleAuthoritativeInstance()
    {
        UserInventoryData inventory = InventoryWith("equipment_helmet_rare", "first", "second");
        UserRosterData roster = RosterWithUnit();
        EquipmentLoadoutService service = new(GameConfig.Items);

        Assert.That(service.Equip(inventory, roster, "unit", "first").Succeeded, Is.True);
        EquipmentLoadoutResult swap = service.Equip(inventory, roster, "unit", "second");

        Assert.That(swap.Succeeded, Is.True);
        Assert.That(swap.UnequippedEquipmentId, Is.EqualTo("first"));
        Assert.That(roster.OwnedUnits[0].Equipment.HelmetId, Is.EqualTo("second"));
        Assert.That(inventory.Equipments, Has.Count.EqualTo(2));
        Assert.That(new EquipmentInventoryIndex(inventory, roster).UnequippedCount, Is.EqualTo(1));
    }

    [Test]
    public void Unequip_IsRejectedWhenUnequippedInventoryIsFull()
    {
        UserInventoryData inventory = InventoryWith("equipment_helmet_rare", "equipped");
        for (int i = 0; i < EquipmentInventoryIndex.UnequippedCapacity; i++)
            inventory.Equipments.Add(new EquipmentItemData { UniqueId = $"spare-{i}", ItemId = "equipment_helmet_rare" });
        UserRosterData roster = RosterWithUnit();
        roster.OwnedUnits[0].Equipment.HelmetId = "equipped";

        EquipmentLoadoutResult result = new EquipmentLoadoutService(GameConfig.Items)
            .Unequip(inventory, roster, "unit", EquipmentType.Helmet);

        Assert.That(result.Failure, Is.EqualTo(EquipmentLoadoutFailure.InventoryFull));
        Assert.That(roster.OwnedUnits[0].Equipment.HelmetId, Is.EqualTo("equipped"));
    }

    [Test]
    public void Enhance_AdvancesOneLevelAndGrantsLevelTenBonus()
    {
        UserInventoryData inventory = InventoryWith("equipment_main_weapon_rare", "weapon");
        inventory.Equipments[0].Level = 9;
        inventory.Equipments[0].EnhancementExp = 140;
        inventory.Materials.Add(new InventoryStackItem { ItemId = "equipment_stone_rare", Count = 1 });
        EquipmentEnhancementService service = new(GameConfig.Items, GameConfig.EquipmentBalance, new FixedRandom());

        EquipmentEnhancementResult result = service.Enhance(inventory, "weapon",
            new Dictionary<string, int> { ["equipment_stone_rare"] = 1 });

        Assert.That(result.Succeeded, Is.True);
        Assert.That(result.Level, Is.EqualTo(10));
        Assert.That(result.BonusGranted, Is.True);
        Assert.That(inventory.Equipments[0].BonusOption.StatType, Is.EqualTo(EquipmentStatType.Attack));
        Assert.That(inventory.Equipments[0].BonusOption.Value, Is.EqualTo(5f));
    }

    [Test]
    public void Enhance_RefundsRepresentableOverflowAndRetainsRemainder()
    {
        UserInventoryData inventory = InventoryWith("equipment_main_weapon_rare", "weapon");
        inventory.Materials.Add(new InventoryStackItem { ItemId = "equipment_stone_epic", Count = 1 });
        EquipmentEnhancementResult result = new EquipmentEnhancementService(
            GameConfig.Items, GameConfig.EquipmentBalance, new FixedRandom()).Enhance(
                inventory, "weapon", new Dictionary<string, int> { ["equipment_stone_epic"] = 1 });

        Assert.That(result.Succeeded, Is.True);
        Assert.That(result.Level, Is.EqualTo(1));
        Assert.That(result.Exp, Is.EqualTo(0));
        Assert.That(inventory.Materials.Find(x => x.ItemId == "equipment_stone_rare").Count, Is.EqualTo(4));
    }

    [Test]
    public void Dismantle_RefundsBaseAndNinetyPercentOfInvestedExp()
    {
        UserInventoryData inventory = InventoryWith("equipment_helmet_rare", "helmet");
        inventory.Equipments[0].TotalInvestedExp = 100;

        EquipmentDismantleResult result = new EquipmentDismantleService(GameConfig.Items, GameConfig.EquipmentBalance)
            .Dismantle(inventory, RosterWithUnit(), new[] { "helmet" });

        Assert.That(result.Succeeded, Is.True);
        Assert.That(result.RefundedExp, Is.EqualTo(110));
        Assert.That(result.Rewards["equipment_stone_epic"], Is.EqualTo(2));
        Assert.That(result.Rewards["equipment_stone_rare"], Is.EqualTo(1));
        Assert.That(inventory.Equipments, Is.Empty);
    }

    [Test]
    public void EquipmentReward_DiscardsOnlyCapacityOverflow()
    {
        UserDataRoot user = new();
        for (int i = 0; i < 99; i++)
            user.Inventory.Equipments.Add(new EquipmentItemData { UniqueId = $"item-{i}", ItemId = "equipment_helmet_rare" });

        RewardGrantResult result = RewardGrantCalculator.Calculate(user, new[]
        {
            new RewardData { Type = RewardType.Equipment, Id = "equipment_helmet_rare", Amount = 3 },
        });

        Assert.That(result.Succeeded, Is.True);
        Assert.That(result.Inventory.Equipments, Has.Count.EqualTo(100));
        Assert.That(result.DiscardedEquipmentCount, Is.EqualTo(2));
    }

    [Test]
    public void BonusSummary_MergesSameStatAndModifierAcrossEquipmentParts()
    {
        EquipmentItemData helmet = LevelTenBonus(EquipmentStatType.DamageReduction, EquipmentModifierType.Percent, 8f);
        EquipmentItemData armor = LevelTenBonus(EquipmentStatType.DamageReduction, EquipmentModifierType.Percent, 5f);

        IReadOnlyList<string> summary = EquipmentBonusSummaryFormatter.Build(new[] { helmet, armor });

        Assert.That(summary.Count, Is.EqualTo(1));
        Assert.That(summary[0], Is.EqualTo("받는 피해 감소  +13%"));
    }

    [Test]
    public void BonusSummary_KeepsFlatAndPercentValuesSeparate()
    {
        EquipmentItemData flat = LevelTenBonus(EquipmentStatType.Attack, EquipmentModifierType.Flat, 30f);
        EquipmentItemData percent = LevelTenBonus(EquipmentStatType.Attack, EquipmentModifierType.Percent, 7f);

        IReadOnlyList<string> summary = EquipmentBonusSummaryFormatter.Build(new[] { flat, percent });

        Assert.That(summary, Is.EquivalentTo(new[] { "공격력  +30", "공격력  +7%" }));
    }

    [Test]
    public void BaseStatSummary_MergesSameStatsAcrossEquipmentParts()
    {
        EquipmentItemData mainWeapon = new()
        {
            ItemId = "equipment_main_weapon_rare",
            Level = 0,
        };
        EquipmentItemData subWeapon = new()
        {
            ItemId = "equipment_sub_weapon_rare",
            Level = 0,
        };

        IReadOnlyList<string> summary = EquipmentBaseStatSummaryFormatter.Build(
            new[] { mainWeapon, subWeapon },
            GameConfig.Items,
            GameConfig.EquipmentBalance);

        Assert.That(summary.Count, Is.EqualTo(1));
        Assert.That(summary[0], Is.EqualTo("공격력  +150"));
    }

    [Test]
    public void EnhancementSelection_StopsAddingAfterTargetExperience()
    {
        EquipmentEnhancementSelection selection = new(GameConfig.Items);

        Assert.That(selection.TryAdd("equipment_stone_epic", 2, 40), Is.True);
        Assert.That(selection.GetTotalExperience(), Is.EqualTo(50));
        Assert.That(selection.TryAdd("equipment_stone_rare", 10, 40), Is.False);
        Assert.That(selection.GetQuantity("equipment_stone_rare"), Is.Zero);
    }

    [Test]
    public void EnhancementSelection_AutoSelectDoesNothingWhenMaterialsAreInsufficient()
    {
        EquipmentEnhancementSelection selection = new(GameConfig.Items);
        selection.TryAdd("equipment_stone_rare", 2, 100);
        InventoryStackItem[] materials =
        {
            new() { ItemId = "equipment_stone_rare", Count = 2 },
        };

        bool changed = selection.TryAutoSelect(materials, 100);

        Assert.That(changed, Is.False);
        Assert.That(selection.GetQuantity("equipment_stone_rare"), Is.EqualTo(1));
    }

    [Test]
    public void EnhancementSelection_AutoSelectCreatesSerializableSnapshot()
    {
        EquipmentEnhancementSelection selection = new(GameConfig.Items);
        InventoryStackItem[] materials =
        {
            new() { ItemId = "equipment_stone_rare", Count = 3 },
            new() { ItemId = "equipment_stone_epic", Count = 1 },
        };

        Assert.That(selection.TryAutoSelect(materials, 60), Is.True);
        Dictionary<string, int> snapshot = selection.CreateSnapshot();

        Assert.That(selection.GetTotalExperience(), Is.GreaterThanOrEqualTo(60));
        Assert.That(snapshot, Is.EquivalentTo(selection.CreateSnapshot()));
        snapshot.Clear();
        Assert.That(selection.Count, Is.GreaterThan(0));
    }

    [Test]
    public void EnhancementPresenter_AddsOneMaterialPerRequest()
    {
        UserDataRoot user = new();
        EquipmentItemData equipment = new()
        {
            UniqueId = "weapon",
            ItemId = "equipment_main_weapon_rare",
        };
        user.Inventory.Equipments.Add(equipment);
        user.Inventory.Materials.Add(new InventoryStackItem
        {
            ItemId = "equipment_stone_rare",
            Count = 10,
        });
        UnitEquipmentDataSource source = new(() => user, GameConfig.Items);
        EquipmentEnhancementPresenter presenter = new(
            source,
            GameConfig.Items,
            GameConfig.EquipmentBalance);
        presenter.Bind(equipment);

        Assert.That(presenter.TryAddMaterial("equipment_stone_rare", 10), Is.True);
        Assert.That(presenter.Selection.GetQuantity("equipment_stone_rare"), Is.EqualTo(1));
    }

    [Test]
    public void EnhancementPresenter_BuildsPreviewFromCurrentAndSelectedExperience()
    {
        UserDataRoot user = new();
        EquipmentItemData equipment = new()
        {
            UniqueId = "weapon",
            ItemId = "equipment_main_weapon_rare",
            EnhancementExp = 5,
        };
        user.Inventory.Equipments.Add(equipment);
        user.Inventory.Materials.Add(new InventoryStackItem
        {
            ItemId = "equipment_stone_rare",
            Count = 10,
        });
        EquipmentEnhancementPresenter presenter = new(
            new UnitEquipmentDataSource(() => user, GameConfig.Items),
            GameConfig.Items,
            GameConfig.EquipmentBalance);
        presenter.Bind(equipment);
        presenter.TryAddMaterial("equipment_stone_rare", 10);

        Assert.That(presenter.TryBuildState(out EquipmentEnhancementPresenter.ViewState state), Is.True);
        Assert.That(state.PreviewExperience, Is.EqualTo(
            equipment.EnhancementExp + presenter.Selection.GetTotalExperience()));
        Assert.That(state.Materials, Has.Count.EqualTo(1));
    }

    [Test]
    public void ChangePresenter_FiltersCandidatesByPartAndTracksSelection()
    {
        UserDataRoot user = new();
        user.Roster.OwnedUnits.Add(new UserUnitData { UnitId = "unit" });
        EquipmentItemData weapon = new()
        {
            UniqueId = "weapon",
            ItemId = "equipment_main_weapon_rare",
        };
        user.Inventory.Equipments.Add(weapon);
        user.Inventory.Equipments.Add(new EquipmentItemData
        {
            UniqueId = "helmet",
            ItemId = "equipment_helmet_rare",
        });
        EquipmentChangePresenter presenter = new(
            new UnitEquipmentDataSource(() => user, GameConfig.Items),
            GameConfig.EquipmentBalance);
        presenter.Bind("unit", EquipmentType.MainWeapon);

        EquipmentChangePresenter.ViewState before = presenter.BuildState(false);
        Assert.That(before.Candidates, Has.Count.EqualTo(1));
        Assert.That(before.CanEquip, Is.False);

        Assert.That(presenter.Select(weapon), Is.True);
        EquipmentChangePresenter.ViewState after = presenter.BuildState(false);
        Assert.That(after.Selected, Is.SameAs(weapon));
        Assert.That(after.CanEquip, Is.True);
    }

    [Test]
    public void ChangePresenter_RejectsSelectionFromDifferentPart()
    {
        UserDataRoot user = new();
        user.Roster.OwnedUnits.Add(new UserUnitData { UnitId = "unit" });
        EquipmentItemData helmet = new()
        {
            UniqueId = "helmet",
            ItemId = "equipment_helmet_rare",
        };
        user.Inventory.Equipments.Add(helmet);
        EquipmentChangePresenter presenter = new(
            new UnitEquipmentDataSource(() => user, GameConfig.Items),
            GameConfig.EquipmentBalance);
        presenter.Bind("unit", EquipmentType.MainWeapon);

        Assert.That(presenter.Select(helmet), Is.False);
        Assert.That(presenter.Selected, Is.Null);
    }

    private static EquipmentItemData LevelTenBonus(
        EquipmentStatType statType,
        EquipmentModifierType modifierType,
        float value) => new()
        {
            Level = EquipmentBalanceConfigSO.MaxLevel,
            BonusOption = new EquipmentBonusOptionData
            {
                StatType = statType,
                ModifierType = modifierType,
                Value = value,
            },
        };

    private static UserInventoryData InventoryWith(string itemId, params string[] ids)
    {
        UserInventoryData inventory = new();
        foreach (string id in ids) inventory.Equipments.Add(new EquipmentItemData { UniqueId = id, ItemId = itemId });
        return inventory;
    }

    private static UserRosterData RosterWithUnit() => new()
    {
        OwnedUnits = new List<UserUnitData> { new() { UnitId = "unit", Equipment = new EquipmentLoadout() } },
    };

    private sealed class FixedRandom : IEquipmentRandom
    {
        public int Range(int minInclusive, int maxExclusive) => minInclusive;
        public float Range(float minInclusive, float maxInclusive) => minInclusive;
    }
}
