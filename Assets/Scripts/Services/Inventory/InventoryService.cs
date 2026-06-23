using System;
using System.Collections.Generic;
using System.Linq;

public class InventoryService
{
    private UserInventoryData Inventory => UserDataManager.Instance.UserData.Inventory;


    public IReadOnlyList<InventoryStackItem> GetMaterials()
    {
        return Inventory.Materials;
    }


    public IReadOnlyList<InventoryStackItem> GetConsumables()
    {
        return Inventory.Consumables;
    }

    public IReadOnlyList<EquipmentItemData> GetEquipments()
    {
        return Inventory.Equipments;
    }

    public void AddStackItem(ItemCategory category, string itemId, int count)
    {
        List<InventoryStackItem> target = GetTargetList(category);

        if (target == null)
            return;

        var item = target.FirstOrDefault(x => x.ItemId == itemId);

        if (item == null)
        {
            item = new InventoryStackItem{ItemId = itemId,Count = 0};
            target.Add(item);
        }

        item.Count += count;
        UserDataManager.Instance.MarkDirty();
    }


    private List<InventoryStackItem> GetTargetList(ItemCategory category)
    {
        return category switch
        {
            ItemCategory.Material => Inventory.Materials,
            ItemCategory.Consumable => Inventory.Consumables,
            _ => null
        };
    }

    public void AddEquipment(string equipmentId)
    {
        ItemDataSO itemData = ItemDatabase.Get(equipmentId);

        if (itemData == null)
            return;

        EquipmentItemData equipment = new() { UniqueId = Guid.NewGuid().ToString(), ItemId = equipmentId, Level = 1 };

        Inventory.Equipments.Add(equipment);

        UserDataManager.Instance.MarkDirty();
    }

    public bool RemoveStackItem(ItemCategory category, string itemId, int count)
    {
        List<InventoryStackItem> target = GetTargetList(category);

        if (target == null)
            return false;

        InventoryStackItem item = target.FirstOrDefault(x => x.ItemId == itemId);
        
        if(item == null) 
            return false;

        if (item.Count < count)
            return false;

        item.Count -= count;

        if(item.Count <= 0)
            target.Remove(item);

        UserDataManager.Instance.MarkDirty();
        
        return true;
    }
    public int GetItemCount(string itemId)
    {
        InventoryStackItem item = Inventory.Materials.FirstOrDefault(x => x.ItemId == itemId);

        if (item != null)
            return item.Count;

        item = Inventory.Consumables.FirstOrDefault(x => x.ItemId == itemId);

        return item?.Count ?? 0;
    }
}