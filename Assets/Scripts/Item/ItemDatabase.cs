using System.Collections.Generic;
using UnityEngine;

public static class ItemDatabase
{
    private static Dictionary<string, ItemDataSO> itemDict;

    public static void Initialize()
    {
        itemDict = new();

        ItemDataSO[] items = Resources.LoadAll<ItemDataSO>("Items");

        foreach (var item in items)
        {
            itemDict[item.ItemId] = item;
        }

        Debug.Log($"ItemDatabase Loaded : {itemDict.Count}");
    }

    public static ItemDataSO GetItem(string itemId)
    {
        itemDict.TryGetValue(itemId, out var item);

        return item;
    }
}