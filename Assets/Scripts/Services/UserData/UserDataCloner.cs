using System.Collections.Generic;
using System.Linq;

public static class UserDataCloner
{
    public static UserResourceData Copy(UserResourceData source)
    {
        if (source == null)
            return null;

        return new UserResourceData
        {
            Gold = source.Gold,
            Gem = source.Gem,
            Fuel = source.Fuel,
            MaxFuel = source.MaxFuel,
            LastFuelUpdateTime = source.LastFuelUpdateTime,
        };
    }

    public static UserInventoryData Copy(UserInventoryData source)
    {
        return new UserInventoryData
        {
            Materials = CopyStackItems(source?.Materials),
            Consumables = CopyStackItems(source?.Consumables),
            Equipments = source?.Equipments?
                .Where(item => item != null)
                .Select(item => new EquipmentItemData
                {
                    UniqueId = item.UniqueId,
                    ItemId = item.ItemId,
                    Level = item.Level,
                })
                .ToList() ?? new List<EquipmentItemData>(),
        };
    }

    public static UserRosterData Copy(UserRosterData source)
    {
        return new UserRosterData
        {
            Power = source?.Power ?? 0,
            SelectedUnitIds = source?.SelectedUnitIds?.ToList() ?? new List<string>(),
            OwnedUnits = source?.OwnedUnits?
                .Where(unit => unit != null)
                .Select(unit => new UserUnitData
                {
                    UnitId = unit.UnitId,
                    Level = unit.Level,
                    Exp = unit.Exp,
                    LimitBreak = unit.LimitBreak,
                    Promotion = unit.Promotion,
                    DuplicateCount = unit.DuplicateCount,
                })
                .ToList() ?? new List<UserUnitData>(),
        };
    }

    public static UserGachaData Copy(UserGachaData source)
    {
        return new UserGachaData
        {
            NormalPity = source?.NormalPity ?? 0,
            SpecialPity = source?.SpecialPity ?? 0,
        };
    }

    private static List<InventoryStackItem> CopyStackItems(IEnumerable<InventoryStackItem> source)
    {
        return source?
            .Where(item => item != null)
            .Select(item => new InventoryStackItem { ItemId = item.ItemId, Count = item.Count })
            .ToList() ?? new List<InventoryStackItem>();
    }
}
