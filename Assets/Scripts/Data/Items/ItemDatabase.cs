using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class ItemDatabase
{
    private static Dictionary<string, ItemDataSO> itemDict;
    private const string ItemDataPath = "Items";

    public static void Initialize()
    {
        if (itemDict != null)
            return;

        itemDict = new();

        ItemDataSO[] items = Resources.LoadAll<ItemDataSO>(ItemDataPath);

        if (itemDict == null || items.Length == 0)
            return;

        foreach (ItemDataSO item in items)
        {
            if(item == null) 
                continue;

            if (string.IsNullOrEmpty(item.ItemId))
                continue;

            if (itemDict.ContainsKey(item.ItemId))
                continue;
            
            itemDict.Add(item.ItemId, item);
        }

        Debug.Log($"ItemDatabase Loaded : {itemDict.Count}");
    }

    public static ItemDataSO Get(string itemId)
    {
        if (string.IsNullOrEmpty(itemId))
            return null;

        itemDict.TryGetValue(itemId, out var item);

        return item;
    }
}