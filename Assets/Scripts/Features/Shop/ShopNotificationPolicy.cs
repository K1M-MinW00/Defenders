using System;
using System.Collections.Generic;
using System.Linq;

public static class ShopNotificationPolicy
{
    public static string BuildMarker(
        ShopTabType tab,
        ShopProductGroup group,
        Func<ShopProductData, ShopProductState> getState,
        DateTimeOffset utcNow)
    {
        if (group == null || getState == null)
            return string.Empty;

        IEnumerable<ShopProductData> products = GetProducts(tab, group);
        IEnumerable<string> parts = products
            .Where(product => product != null && product.IsEnabled)
            .Where(product => ShouldNotify(tab, product, getState(product)))
            .OrderBy(product => product.ProductId, StringComparer.Ordinal)
            .Select(product => BuildProductMarker(tab, product, utcNow));

        return string.Join("|", parts);
    }

    public static bool HasNotification(UserShopData shop, ShopTabType tab, string marker)
    {
        if (string.IsNullOrWhiteSpace(marker))
            return false;

        string seenMarker = shop?.SeenTabs?
            .FirstOrDefault(seen => seen != null && seen.Tab == (int)tab)?.Marker;
        return !string.Equals(seenMarker, marker, StringComparison.Ordinal);
    }

    public static void MarkSeen(UserShopData shop, ShopTabType tab, string marker)
    {
        if (shop == null || string.IsNullOrWhiteSpace(marker))
            return;

        shop.SeenTabs ??= new List<UserShopTabSeenData>();
        UserShopTabSeenData seen = shop.SeenTabs
            .FirstOrDefault(entry => entry != null && entry.Tab == (int)tab);
        if (seen == null)
        {
            seen = new UserShopTabSeenData { Tab = (int)tab };
            shop.SeenTabs.Add(seen);
        }

        seen.Marker = marker;
    }

    private static IEnumerable<ShopProductData> GetProducts(ShopTabType tab, ShopProductGroup group)
    {
        return tab switch
        {
            ShopTabType.Limited => group.LimitedProducts ?? Enumerable.Empty<ShopProductData>(),
            ShopTabType.Package => CombinePackages(group),
            ShopTabType.Recharge => group.RechargeProducts ?? Enumerable.Empty<ShopProductData>(),
            ShopTabType.Exchange => group.ExchangeProducts ?? Enumerable.Empty<ShopProductData>(),
            _ => Enumerable.Empty<ShopProductData>(),
        };
    }

    private static IEnumerable<ShopProductData> CombinePackages(ShopProductGroup group)
    {
        return (group.DailyPackages ?? Enumerable.Empty<ShopProductData>())
            .Concat(group.WeeklyPackages ?? Enumerable.Empty<ShopProductData>())
            .Concat(group.MonthlyPackages ?? Enumerable.Empty<ShopProductData>());
    }

    private static bool ShouldNotify(
        ShopTabType tab,
        ShopProductData product,
        ShopProductState state)
    {
        return tab switch
        {
            ShopTabType.Limited => state.IsOnSale && !state.IsSoldOut,
            ShopTabType.Package => state.IsOnSale && !state.IsSoldOut,
            ShopTabType.Recharge => product is RechargeShopProductData recharge &&
                                    recharge.HasFirstPurchaseBonus &&
                                    state.PurchasedCount == 0,
            ShopTabType.Exchange => state.IsOnSale,
            _ => false,
        };
    }

    private static string BuildProductMarker(
        ShopTabType tab,
        ShopProductData product,
        DateTimeOffset utcNow)
    {
        string period = tab == ShopTabType.Package
            ? ShopTimePolicy.GetPeriodKey(product.ResetPeriod, utcNow)
            : "catalog";
        return product.ProductId + ":" + period;
    }
}
