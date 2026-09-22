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
        if (Inventory?.Materials == null)
            return Array.Empty<InventoryStackItem>();

        if(type == MaterialType.None)
            return Inventory.Materials;

        return Inventory.Materials.Where(x =>
        {
            if (x == null)
                return false;

            MaterialDataSO data = GameConfig.Items.Get(x.ItemId) as MaterialDataSO;
            return data != null && data.MaterialType == type;
        }).ToList();
    }


    public IReadOnlyList<InventoryStackItem> GetConsumables()
    {
        if (Inventory?.Consumables == null)
            return Array.Empty<InventoryStackItem>();

        return Inventory.Consumables;
    }

    public IReadOnlyList<EquipmentItemData> GetEquipments()
    {
        if (Inventory?.Equipments == null)
            return Array.Empty<EquipmentItemData>();

        return Inventory.Equipments;
    }

    public int GetItemCount(string itemId)
    {
        if (string.IsNullOrWhiteSpace(itemId) || Inventory == null)
            return 0;

        List<InventoryStackItem> materials = Inventory.Materials ?? new List<InventoryStackItem>();
        List<InventoryStackItem> consumables = Inventory.Consumables ?? new List<InventoryStackItem>();

        InventoryStackItem item = materials.FirstOrDefault(x => x != null && x.ItemId == itemId);

        if (item != null)
            return item.Count;

        item = consumables.FirstOrDefault(x => x != null && x.ItemId == itemId);

        return item?.Count ?? 0;
    }
}
