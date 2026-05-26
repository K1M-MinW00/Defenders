using System.Collections.Generic;
using System.Linq;

public class InventoryService
{
    private UserInventoryData Inventory => UserDataManager.Instance.UserData.Inventory;


    public List<InventoryStackItem> GetMaterials()
    {
        return Inventory.Materials;
    }

    public List<InventoryStackItem> GetConsumables()
    {
        return Inventory.Consumables;
    }

    public List<EquipmentItemData> GetEquipments()
    {
        return Inventory.Equipments;
    }

    public void AddStackItem(ItemCategory category, string itemId, int count)
    {
        List<InventoryStackItem> target = null;

        switch (category)
        {
            case ItemCategory.Material:
                target = Inventory.Materials;
                break;

            case ItemCategory.Consumable:
                target = Inventory.Consumables;
                break;
        }

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

    public void AddEquipment(string equipmentId)
    {
        EquipmentItemData equipment = new()
        {
            UniqueId = System.Guid.NewGuid().ToString(),
            ItemId = equipmentId,
            Level = 1
        };

        Inventory.Equipments.Add(equipment);

        UserDataManager.Instance.MarkDirty();
    }
}