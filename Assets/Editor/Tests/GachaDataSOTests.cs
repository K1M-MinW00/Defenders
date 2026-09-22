using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public sealed class GachaDataSOTests
{
    public void TryValidate_AcceptsCompleteBanner()
    {
        GachaDataSO banner = CreateBanner();

        try
        {
            Assert(banner.TryValidate(out string error), $"A complete banner should be valid: {error}");
        }
        finally
        {
            DestroyBanner(banner);
        }
    }

    public void TryValidate_RejectsUnitInWrongRarityPool()
    {
        GachaDataSO banner = CreateBanner();
        UnitDataSO wrongUnit = CreateUnit("wrong", Rarity.Normal);
        SetField(banner, "legendPool", new List<UnitDataSO> { wrongUnit });

        try
        {
            Assert(!banner.TryValidate(out _), "A unit in the wrong rarity pool should invalidate the banner.");
        }
        finally
        {
            DestroyBanner(banner);
        }
    }

    public void ProjectBanners_AreValid()
    {
        string[] guids = AssetDatabase.FindAssets("t:GachaDataSO");
        Assert(guids.Length > 0, "At least one gacha banner asset should exist.");

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GachaDataSO banner = AssetDatabase.LoadAssetAtPath<GachaDataSO>(path);
            Assert(banner != null, $"Gacha banner could not be loaded at {path}.");
            Assert(banner.TryValidate(out string error), $"Invalid gacha banner at {path}: {error}");
        }
    }

    public void TryResolvePickup_UsesConfiguredSpecialUnit()
    {
        GachaDataSO banner = CreateBanner();
        SetField(banner, "recruitType", RecruitType.Special);
        UnitDataSO pickup = banner.RarePool[0];
        SetField(banner, "pickupUnit", pickup);

        try
        {
            Assert(banner.TryResolvePickup(null, out UnitDataSO resolved), "Configured special pickup should resolve.");
            Assert(resolved == pickup, "The configured pickup unit should be returned from its canonical pool.");
        }
        finally
        {
            DestroyBanner(banner);
        }
    }

    public void GachaRoller_AppliesFiftyPercentPickupWithinRolledRarity()
    {
        GachaDataSO banner = CreateBanner();
        SetField(banner, "recruitType", RecruitType.Special);
        UnitDataSO pickup = banner.RarePool[0];
        UnitDataSO otherRare = CreateUnit("rare_other", Rarity.Rare);
        SetField(banner, "rarePool", new List<UnitDataSO> { pickup, otherRare });
        SetField(banner, "pickupUnit", pickup);

        try
        {
            GachaRoller pickupRoller = new(new SequenceGachaRandom(new[] { 49f }, Array.Empty<int>()));
            GachaRoller nonPickupRoller = new(new SequenceGachaRandom(new[] { 50f }, new[] { 0 }));

            Assert(pickupRoller.RollUnit(banner, Rarity.Rare, pickup) == pickup,
                "A pickup roll below 50% should return the pickup unit.");
            Assert(nonPickupRoller.RollUnit(banner, Rarity.Rare, pickup) == otherRare,
                "A failed pickup roll should select uniformly from units excluding the pickup.");
        }
        finally
        {
            DestroyBanner(banner);
        }
    }

    public void EconomyConfig_LoadsConfiguredDuplicateRewards()
    {
        GachaEconomyConfigSO config = Resources.Load<GachaEconomyConfigSO>("Configs/GachaEconomyConfig");

        Assert(config != null, "Gacha economy config should be available from Resources.");
        Assert(config.GetDuplicateGemReward(Rarity.Normal) == 30, "Normal duplicate reward should come from config.");
        Assert(config.GetDuplicateGemReward(Rarity.Rare) == 100, "Rare duplicate reward should come from config.");
        Assert(config.GetDuplicateGemReward(Rarity.Legend) == 300, "Legend duplicate reward should come from config.");
    }

    private static GachaDataSO CreateBanner()
    {
        GachaDataSO banner = ScriptableObject.CreateInstance<GachaDataSO>();
        SetField(banner, "ticketItemId", "test_ticket");
        SetField(banner, "normalPool", new List<UnitDataSO> { CreateUnit("normal", Rarity.Normal) });
        SetField(banner, "rarePool", new List<UnitDataSO> { CreateUnit("rare", Rarity.Rare) });
        SetField(banner, "legendPool", new List<UnitDataSO> { CreateUnit("legend", Rarity.Legend) });
        return banner;
    }

    private static UnitDataSO CreateUnit(string id, Rarity rarity)
    {
        UnitDataSO unit = ScriptableObject.CreateInstance<UnitDataSO>();
        unit.unitId = id;
        unit.rarity = rarity;
        return unit;
    }

    private static void DestroyBanner(GachaDataSO banner)
    {
        DestroyUnits(banner.NormalPool);
        DestroyUnits(banner.RarePool);
        DestroyUnits(banner.LegendPool);
        UnityEngine.Object.DestroyImmediate(banner);
    }

    private static void DestroyUnits(IReadOnlyList<UnitDataSO> units)
    {
        if (units == null)
            return;

        for (int i = 0; i < units.Count; i++)
        {
            if (units[i] != null)
                UnityEngine.Object.DestroyImmediate(units[i]);
        }
    }

    private static void SetField<T>(GachaDataSO banner, string fieldName, T value)
    {
        FieldInfo field = typeof(GachaDataSO).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        field?.SetValue(banner, value);
    }

    private static void Assert(bool condition, string message)
    {
        if (!condition)
            throw new InvalidOperationException(message);
    }

    private sealed class SequenceGachaRandom : IGachaRandom
    {
        private readonly Queue<float> floatValues;
        private readonly Queue<int> intValues;

        public SequenceGachaRandom(IEnumerable<float> floatValues, IEnumerable<int> intValues)
        {
            this.floatValues = new Queue<float>(floatValues);
            this.intValues = new Queue<int>(intValues);
        }

        public float Range(float minInclusive, float maxExclusive) => floatValues.Dequeue();
        public int Range(int minInclusive, int maxExclusive) => intValues.Dequeue();
    }
}
