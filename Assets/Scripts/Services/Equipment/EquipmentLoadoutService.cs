using System;
using System.Linq;

public enum EquipmentLoadoutFailure
{
    None,
    InvalidData,
    UnitNotFound,
    EquipmentNotFound,
    InvalidEquipmentType,
    EquippedByAnotherUnit,
    InventoryFull,
}

public readonly struct EquipmentLoadoutResult
{
    public bool Succeeded => Failure == EquipmentLoadoutFailure.None;
    public EquipmentLoadoutFailure Failure { get; }
    public string UnequippedEquipmentId { get; }

    private EquipmentLoadoutResult(EquipmentLoadoutFailure failure, string unequippedEquipmentId = null)
    {
        Failure = failure;
        UnequippedEquipmentId = unequippedEquipmentId;
    }

    public static EquipmentLoadoutResult Success(string unequippedEquipmentId = null) => new(EquipmentLoadoutFailure.None, unequippedEquipmentId);
    public static EquipmentLoadoutResult Fail(EquipmentLoadoutFailure failure) => new(failure);
}

public sealed class EquipmentLoadoutService
{
    private readonly IItemCatalog itemCatalog;

    public EquipmentLoadoutService(IItemCatalog itemCatalog)
    {
        this.itemCatalog = itemCatalog ?? throw new ArgumentNullException(nameof(itemCatalog));
    }

    public EquipmentLoadoutResult Equip(UserInventoryData inventory, UserRosterData roster, string unitId, string equipmentUniqueId)
    {
        if (inventory?.Equipments == null || roster?.OwnedUnits == null || string.IsNullOrWhiteSpace(unitId))
            return EquipmentLoadoutResult.Fail(EquipmentLoadoutFailure.InvalidData);

        UserUnitData unit = roster.OwnedUnits.FirstOrDefault(candidate => candidate?.UnitId == unitId);
        if (unit == null)
            return EquipmentLoadoutResult.Fail(EquipmentLoadoutFailure.UnitNotFound);

        EquipmentInventoryIndex index = new(inventory, roster);
        if (!index.TryGet(equipmentUniqueId, out EquipmentItemData equipment))
            return EquipmentLoadoutResult.Fail(EquipmentLoadoutFailure.EquipmentNotFound);

        EquipmentDataSO definition = itemCatalog.Get(equipment.ItemId) as EquipmentDataSO;
        if (definition == null || definition.EquipmentType == EquipmentType.None)
            return EquipmentLoadoutResult.Fail(EquipmentLoadoutFailure.InvalidEquipmentType);

        unit.Equipment ??= new EquipmentLoadout();
        string currentId = unit.Equipment.Get(definition.EquipmentType);
        if (currentId == equipmentUniqueId)
            return EquipmentLoadoutResult.Success();

        if (index.IsEquipped(equipmentUniqueId))
            return EquipmentLoadoutResult.Fail(EquipmentLoadoutFailure.EquippedByAnotherUnit);

        unit.Equipment.Set(definition.EquipmentType, equipmentUniqueId);
        return EquipmentLoadoutResult.Success(currentId);
    }

    public EquipmentLoadoutResult Unequip(UserInventoryData inventory, UserRosterData roster, string unitId, EquipmentType type)
    {
        if (inventory?.Equipments == null || roster?.OwnedUnits == null || type == EquipmentType.None)
            return EquipmentLoadoutResult.Fail(EquipmentLoadoutFailure.InvalidData);

        UserUnitData unit = roster.OwnedUnits.FirstOrDefault(candidate => candidate?.UnitId == unitId);
        if (unit?.Equipment == null)
            return EquipmentLoadoutResult.Fail(EquipmentLoadoutFailure.UnitNotFound);

        string currentId = unit.Equipment.Get(type);
        if (string.IsNullOrWhiteSpace(currentId))
            return EquipmentLoadoutResult.Success();

        EquipmentInventoryIndex index = new(inventory, roster);
        if (index.IsFull)
            return EquipmentLoadoutResult.Fail(EquipmentLoadoutFailure.InventoryFull);

        unit.Equipment.Set(type, null);
        return EquipmentLoadoutResult.Success(currentId);
    }
}
