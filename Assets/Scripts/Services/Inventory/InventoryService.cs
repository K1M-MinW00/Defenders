using System;
using System.Collections.Generic;
using System.Linq;

public sealed class InventoryService
{
    private readonly UserDataRoot userData;
    private UserInventoryData Inventory => userData.Inventory;

    public InventoryService(UserDataRoot userData)
    {
        this.userData = userData;
    }


    public IReadOnlyList<InventoryStackItem> GetMaterials(MaterialType type = MaterialType.None)
    {
        if(type == MaterialType.None)
            return Inventory.Materials;

        return Inventory.Materials.Where(x =>
        {
            MaterialDataSO data = ItemDatabase.Get(x.ItemId) as MaterialDataSO;
            return data != null && data.MaterialType == type;
        }).ToList();
    }


    public IReadOnlyList<InventoryStackItem> GetConsumables()
    {
        return Inventory.Consumables;
    }

    public IReadOnlyList<EquipmentItemData> GetEquipments()
    {
        return Inventory.Equipments;
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
