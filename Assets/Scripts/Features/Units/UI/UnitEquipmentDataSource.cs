using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Provides the unit-equipment UI with read-only inventory and loadout data.
/// Mutation commands remain in the panel/coordinator boundary.
/// </summary>
public sealed class UnitEquipmentDataSource
{
    private readonly Func<UserDataRoot> getUserData;
    private readonly IItemCatalog itemCatalog;

    public UnitEquipmentDataSource(Func<UserDataRoot> userDataProvider, IItemCatalog catalog)
    {
        getUserData = userDataProvider ?? throw new ArgumentNullException(nameof(userDataProvider));
        itemCatalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
    }

    public UserUnitData GetUnit(string unitId)
        => getUserData()?.Roster?.OwnedUnits?
            .FirstOrDefault(unit => unit?.UnitId == unitId);

    public EquipmentItemData FindEquipment(string uniqueId)
        => getUserData()?.Inventory?.Equipments?
            .Find(item => item?.UniqueId == uniqueId);

    public EquipmentItemData GetEquipped(string unitId, EquipmentType type)
    {
        string uniqueId = GetUnit(unitId)?.Equipment?.Get(type);
        return string.IsNullOrEmpty(uniqueId) ? null : FindEquipment(uniqueId);
    }

    public IEnumerable<EquipmentItemData> GetEquippedItems(
        string unitId,
        IEnumerable<EquipmentType> equipmentTypes)
        => equipmentTypes
            .Select(type => GetEquipped(unitId, type))
            .Where(item => item != null);

    public IEnumerable<EquipmentItemData> GetUnequipped(EquipmentType type)
    {
        UserDataRoot data = getUserData();
        if (data?.Inventory == null || data.Roster == null)
            return Enumerable.Empty<EquipmentItemData>();

        EquipmentInventoryIndex index = new(data.Inventory, data.Roster);
        return index.GetUnequipped()
            .Where(item => GetDefinition(item)?.EquipmentType == type)
            .OrderByDescending(item => (int)GetDefinition(item).Rarity)
            .ThenByDescending(item => item.Level);
    }

    public IEnumerable<InventoryStackItem> GetEnhancementMaterials()
        => getUserData()?.Inventory?.Materials?
            .Where(item => item != null
                && item.Count > 0
                && itemCatalog.Get(item.ItemId) is MaterialDataSO material
                && material.MaterialType == MaterialType.Equipment)
            .OrderBy(item => (itemCatalog.Get(item.ItemId) as MaterialDataSO)?.Value ?? 0)
            ?? Enumerable.Empty<InventoryStackItem>();

    public EquipmentDataSO GetDefinition(EquipmentItemData item)
        => item == null ? null : itemCatalog.Get(item.ItemId) as EquipmentDataSO;
}
