using System;
using System.Collections.Generic;

public static class UserDataNormalizer
{
    public static bool Normalize(UserDataRoot data, string userId)
    {
        if (data == null)
            return false;

        bool changed = false;

        if (data.Profile == null)
        {
            data.Profile = UserDataFactory.CreateDefaultProfile(userId);
            changed = true;
        }
        else if (data.Profile.UserId != userId)
        {
            data.Profile.UserId = userId;
            changed = true;
        }

        if (data.Resource == null)
        {
            data.Resource = UserDataFactory.CreateDefaultResources();
            changed = true;
        }

        if (data.Roster == null)
        {
            data.Roster = UserDataFactory.CreateDefaultRoster();
            changed = true;
        }

        if (data.Roster.OwnedUnits == null)
        {
            data.Roster.OwnedUnits = new List<UserUnitData>();
            changed = true;
        }

        if (data.Roster.SelectedUnitIds == null)
        {
            data.Roster.SelectedUnitIds = new List<string>();
            changed = true;
        }

        changed |= NormalizeProfile(data.Profile, userId);
        changed |= NormalizeResources(data.Resource);
        changed |= NormalizeRoster(data.Roster);

        string resolvedIconId = ProfileIconResolver.ResolveIconId(data.Profile.IconId, data.Roster);
        if (data.Profile.IconId != resolvedIconId)
        {
            data.Profile.IconId = resolvedIconId;
            changed = true;
        }

        if (data.Progress == null)
        {
            data.Progress = UserDataFactory.CreateDefaultProgress();
            changed = true;
        }

        if (data.Inventory == null)
        {
            data.Inventory = UserDataFactory.CreateDefaultInventory();
            changed = true;
        }

        if (data.Inventory.Materials == null)
        {
            data.Inventory.Materials = new List<InventoryStackItem>();
            changed = true;
        }

        if (data.Inventory.Consumables == null)
        {
            data.Inventory.Consumables = new List<InventoryStackItem>();
            changed = true;
        }

        if (data.Inventory.Equipments == null)
        {
            data.Inventory.Equipments = new List<EquipmentItemData>();
            changed = true;
        }

        changed |= NormalizeInventory(data.Inventory);

        if (data.Gacha == null)
        {
            data.Gacha = UserDataFactory.CreateDefaultGacha();
            changed = true;
        }

        if (data.Ad == null)
        {
            data.Ad = UserDataFactory.CreateDefaultAd();
            changed = true;
        }

        changed |= NormalizeProgress(data.Progress);
        changed |= NormalizeGacha(data.Gacha);
        changed |= NormalizeAd(data.Ad);

        return changed;
    }

    private static bool NormalizeProfile(UserProfileData profile, string userId)
    {
        bool changed = false;

        if (profile.UserId != userId)
        {
            profile.UserId = userId;
            changed = true;
        }

        if (string.IsNullOrWhiteSpace(profile.Nickname))
        {
            profile.Nickname = $"User_{userId}";
            changed = true;
        }

        if (profile.Level < 1)
        {
            profile.Level = 1;
            changed = true;
        }

        if (profile.Exp < 0)
        {
            profile.Exp = 0;
            changed = true;
        }

        return changed;
    }

    private static bool NormalizeResources(UserResourceData resources)
    {
        bool changed = false;
        changed |= SetIfDifferent(resources.Gold, Math.Max(resources.Gold, 0), value => resources.Gold = value);
        changed |= SetIfDifferent(resources.Gem, Math.Max(resources.Gem, 0), value => resources.Gem = value);
        changed |= SetIfDifferent(resources.Fuel, Math.Max(resources.Fuel, 0), value => resources.Fuel = value);

        if (resources.MaxFuel <= 0)
        {
            resources.MaxFuel = Math.Max(1, GameConfig.NewUserConfig.MaxFuel);
            changed = true;
        }

        return changed;
    }

    private static bool NormalizeRoster(UserRosterData roster)
    {
        bool changed = false;
        HashSet<string> ownedIds = new();

        for (int i = roster.OwnedUnits.Count - 1; i >= 0; i--)
        {
            UserUnitData unit = roster.OwnedUnits[i];
            if (unit == null || string.IsNullOrWhiteSpace(unit.UnitId) || !ownedIds.Add(unit.UnitId))
            {
                roster.OwnedUnits.RemoveAt(i);
                changed = true;
                continue;
            }

            changed |= NormalizeUnit(unit);
        }

        // The reverse removal loop keeps the last duplicate, so rebuild membership from the final list.
        ownedIds.Clear();
        foreach (UserUnitData unit in roster.OwnedUnits)
            ownedIds.Add(unit.UnitId);

        List<string> selectedIds = new();
        HashSet<string> selectedSet = new();
        foreach (string unitId in roster.SelectedUnitIds)
        {
            if (!string.IsNullOrWhiteSpace(unitId) && ownedIds.Contains(unitId) && selectedSet.Add(unitId))
                selectedIds.Add(unitId);
            else
                changed = true;
        }

        if (selectedIds.Count != roster.SelectedUnitIds.Count)
            roster.SelectedUnitIds = selectedIds;

        if (roster.Power < 0)
        {
            roster.Power = 0;
            changed = true;
        }

        return changed;
    }

    private static bool NormalizeUnit(UserUnitData unit)
    {
        bool changed = false;
        UnitDataSO definition = GameConfig.Units.Get(unit.UnitId);
        int maxLevel = Math.Max(1, definition?.maxLevel ?? int.MaxValue);
        int maxPromotion = Math.Max(0, definition?.promotionCost?.Length ?? int.MaxValue);

        int normalizedLevel = ClampInt(unit.Level, 1, maxLevel);
        int normalizedPromotion = ClampInt(unit.Promotion, 0, maxPromotion);
        int normalizedLimitBreak = ClampInt(unit.LimitBreak, 0, UnitLimitBreakUseCase.MaxLimitBreak);
        int normalizedDuplicateCount = ClampInt(
            unit.DuplicateCount,
            0,
            UnitLimitBreakUseCase.MaxLimitBreak - normalizedLimitBreak);

        changed |= SetIfDifferent(unit.Level, normalizedLevel, value => unit.Level = value);
        changed |= SetIfDifferent(unit.Promotion, normalizedPromotion, value => unit.Promotion = value);
        changed |= SetIfDifferent(unit.LimitBreak, normalizedLimitBreak, value => unit.LimitBreak = value);
        changed |= SetIfDifferent(unit.DuplicateCount, normalizedDuplicateCount, value => unit.DuplicateCount = value);
        changed |= SetIfDifferent(unit.Exp, Math.Max(unit.Exp, 0), value => unit.Exp = value);

        if (unit.Level >= maxLevel && unit.Exp != 0)
        {
            unit.Exp = 0;
            changed = true;
        }

        return changed;
    }

    private static bool NormalizeInventory(UserInventoryData inventory)
    {
        bool changed = false;
        changed |= NormalizeStackItems(inventory.Materials);
        changed |= NormalizeStackItems(inventory.Consumables);

        HashSet<string> equipmentIds = new();
        for (int i = inventory.Equipments.Count - 1; i >= 0; i--)
        {
            EquipmentItemData equipment = inventory.Equipments[i];
            if (equipment == null || string.IsNullOrWhiteSpace(equipment.UniqueId) ||
                string.IsNullOrWhiteSpace(equipment.ItemId) || !equipmentIds.Add(equipment.UniqueId))
            {
                inventory.Equipments.RemoveAt(i);
                changed = true;
                continue;
            }

            if (equipment.Level < 1)
            {
                equipment.Level = 1;
                changed = true;
            }
        }

        return changed;
    }

    private static bool NormalizeStackItems(List<InventoryStackItem> items)
    {
        bool changed = false;
        Dictionary<string, InventoryStackItem> firstByItemId = new();

        for (int i = items.Count - 1; i >= 0; i--)
        {
            InventoryStackItem item = items[i];
            if (item == null || string.IsNullOrWhiteSpace(item.ItemId) || item.Count <= 0)
            {
                items.RemoveAt(i);
                changed = true;
                continue;
            }

            if (!firstByItemId.TryAdd(item.ItemId, item))
            {
                InventoryStackItem existing = firstByItemId[item.ItemId];
                existing.Count = (int)Math.Min((long)existing.Count + item.Count, int.MaxValue);
                items.RemoveAt(i);
                changed = true;
            }
        }

        return changed;
    }

    private static bool NormalizeProgress(UserProgressData progress)
    {
        bool changed = false;
        changed |= SetIfDifferent(progress.CurrentSector, Math.Max(progress.CurrentSector, 1), value => progress.CurrentSector = value);
        changed |= SetIfDifferent(progress.CurrentStage, Math.Max(progress.CurrentStage, 1), value => progress.CurrentStage = value);
        changed |= SetIfDifferent(progress.BestWaveCleared, Math.Max(progress.BestWaveCleared, 0), value => progress.BestWaveCleared = value);
        return changed;
    }

    private static bool NormalizeGacha(UserGachaData gacha)
    {
        bool changed = false;
        changed |= SetIfDifferent(gacha.NormalPity, Math.Max(gacha.NormalPity, 0), value => gacha.NormalPity = value);
        changed |= SetIfDifferent(gacha.SpecialPity, Math.Max(gacha.SpecialPity, 0), value => gacha.SpecialPity = value);
        return changed;
    }

    private static bool NormalizeAd(UserAdData ad)
    {
        bool changed = false;
        int fuelCount = ClampInt(ad.FuelAdWatchCount, 0, AdDailyLimitPolicy.DailyAdLimit);
        int gemCount = ClampInt(ad.GemAdWatchCount, 0, AdDailyLimitPolicy.DailyAdLimit);
        changed |= SetIfDifferent(ad.FuelAdWatchCount, fuelCount, value => ad.FuelAdWatchCount = value);
        changed |= SetIfDifferent(ad.GemAdWatchCount, gemCount, value => ad.GemAdWatchCount = value);
        return changed;
    }

    private static bool SetIfDifferent(int currentValue, int nextValue, Action<int> assign)
    {
        if (currentValue == nextValue)
            return false;

        assign(nextValue);
        return true;
    }

    private static int ClampInt(int value, int minimum, int maximum)
    {
        return Math.Min(Math.Max(value, minimum), maximum);
    }
}
