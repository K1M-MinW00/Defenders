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
}
