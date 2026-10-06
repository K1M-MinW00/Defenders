using System.Collections.Generic;
using System.Linq;

public static class UserDataCloner
{
    public static UserProfileData Copy(UserProfileData source)
    {
        if (source == null)
            return null;

        return new UserProfileData
        {
            UserId = source.UserId,
            Nickname = source.Nickname,
            Level = source.Level,
            Exp = source.Exp,
            IconId = source.IconId,
            HasUsedFreeNicknameChange = source.HasUsedFreeNicknameChange,
        };
    }

    public static UserResourceData Copy(UserResourceData source)
    {
        if (source == null)
            return null;

        return new UserResourceData
        {
            Gold = source.Gold,
            ResearchMaterial = source.ResearchMaterial,
            Gem = source.Gem,
            Fuel = source.Fuel,
            MaxFuel = source.MaxFuel,
            LastFuelUpdateTime = source.LastFuelUpdateTime,
        };
    }

    public static UserLabData Copy(UserLabData source)
    {
        return new UserLabData
        {
            AcquiredCardIds = source?.AcquiredCardIds?.Where(id => !string.IsNullOrWhiteSpace(id)).Distinct().ToList()
                ?? new List<string>(),
        };
    }

    public static UserIdleRewardData Copy(UserIdleRewardData source) => new()
    {
        LastClaimAt = source?.LastClaimAt ?? default,
    };

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

    public static UserAdData Copy(UserAdData source)
    {
        return new UserAdData
        {
            FuelAdWatchCount = source?.FuelAdWatchCount ?? 0,
            GemAdWatchCount = source?.GemAdWatchCount ?? 0,
            AdWatchDate = source?.AdWatchDate,
        };
    }

    public static UserShopData Copy(UserShopData source)
    {
        return new UserShopData
        {
            Purchases = source?.Purchases?
                .Where(purchase => purchase != null)
                .Select(purchase => new UserShopPurchaseData
                {
                    ProductId = purchase.ProductId,
                    PeriodKey = purchase.PeriodKey,
                    Count = purchase.Count,
                    LastPurchasedAt = purchase.LastPurchasedAt,
                })
                .ToList() ?? new List<UserShopPurchaseData>(),
            SeenTabs = source?.SeenTabs?
                .Where(seen => seen != null)
                .Select(seen => new UserShopTabSeenData
                {
                    Tab = seen.Tab,
                    Marker = seen.Marker,
                })
                .ToList() ?? new List<UserShopTabSeenData>(),
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
