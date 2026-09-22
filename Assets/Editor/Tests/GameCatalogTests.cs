using System;
using UnityEngine;

public sealed class GameCatalogTests
{
    public void ProjectCatalogs_LoadAndResolveEveryAsset()
    {
        GameConfig.Initialize();

        Assert(GameConfig.Units != null && GameConfig.Units.GetAll().Count > 0, "Unit catalog should not be empty.");
        Assert(GameConfig.Items != null && GameConfig.Items.GetAll().Count > 0, "Item catalog should not be empty.");

        foreach (UnitDataSO unit in GameConfig.Units.GetAll())
            Assert(GameConfig.Units.Get(unit.unitId) == unit, $"Unit should resolve by ID: {unit.unitId}");

        foreach (ItemDataSO item in GameConfig.Items.GetAll())
            Assert(GameConfig.Items.Get(item.ItemId) == item, $"Item should resolve by ID: {item.ItemId}");
    }

    public void UnitCatalog_RejectsDuplicateIds()
    {
        UnitDataSO first = CreateUnit("duplicate");
        UnitDataSO second = CreateUnit("duplicate");

        try
        {
            AssertThrows(() => new UnitCatalog(new[] { first, second }), "Duplicate unit IDs should be rejected.");
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(first);
            UnityEngine.Object.DestroyImmediate(second);
        }
    }

    public void ItemCatalog_RejectsDuplicateIds()
    {
        ConsumableDataSO first = CreateItem("duplicate");
        ConsumableDataSO second = CreateItem("duplicate");

        try
        {
            AssertThrows(() => new ItemCatalog(new ItemDataSO[] { first, second }), "Duplicate item IDs should be rejected.");
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(first);
            UnityEngine.Object.DestroyImmediate(second);
        }
    }

    private static UnitDataSO CreateUnit(string id)
    {
        UnitDataSO unit = ScriptableObject.CreateInstance<UnitDataSO>();
        unit.unitId = id;
        return unit;
    }

    private static ConsumableDataSO CreateItem(string id)
    {
        ConsumableDataSO item = ScriptableObject.CreateInstance<ConsumableDataSO>();
        item.ItemId = id;
        return item;
    }

    private static void AssertThrows(Action action, string message)
    {
        try
        {
            action();
        }
        catch (ArgumentException)
        {
            return;
        }

        throw new InvalidOperationException(message);
    }

    private static void Assert(bool condition, string message)
    {
        if (!condition)
            throw new InvalidOperationException(message);
    }
}
