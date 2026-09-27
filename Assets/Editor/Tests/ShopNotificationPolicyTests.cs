#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public sealed class ShopNotificationPolicyTests
{
    private readonly List<ScriptableObject> created = new();

    [TearDown]
    public void TearDown()
    {
        foreach (ScriptableObject asset in created)
            UnityEngine.Object.DestroyImmediate(asset);
        created.Clear();
    }

    [Test]
    public void PackageMarker_ChangesWhenDailyPeriodResets()
    {
        PackageShopProductData product = Create<PackageShopProductData>();
        product.ProductId = "daily";
        product.Tab = ShopTabType.Package;
        product.ResetPeriod = ShopResetPeriod.Daily;
        product.IsEnabled = true;
        ShopProductGroup group = Create<ShopProductGroup>();
        group.DailyPackages = new List<ShopProductData> { product };

        ShopProductState available = new(true, false, 0, 1, null);
        string first = ShopNotificationPolicy.BuildMarker(
            ShopTabType.Package,
            group,
            _ => available,
            new DateTimeOffset(2026, 9, 27, 0, 0, 0, TimeSpan.Zero));
        string next = ShopNotificationPolicy.BuildMarker(
            ShopTabType.Package,
            group,
            _ => available,
            new DateTimeOffset(2026, 9, 28, 0, 0, 0, TimeSpan.Zero));

        Assert.That(next, Is.Not.EqualTo(first));
    }

    [Test]
    public void MarkSeen_HidesBadgeUntilMarkerChanges()
    {
        UserShopData shop = new();
        const string marker = "exchange_gold:catalog";

        Assert.That(
            ShopNotificationPolicy.HasNotification(shop, ShopTabType.Exchange, marker),
            Is.True);

        ShopNotificationPolicy.MarkSeen(shop, ShopTabType.Exchange, marker);

        Assert.That(
            ShopNotificationPolicy.HasNotification(shop, ShopTabType.Exchange, marker),
            Is.False);
        Assert.That(
            ShopNotificationPolicy.HasNotification(
                shop,
                ShopTabType.Exchange,
                marker + "|exchange_fuel:catalog"),
            Is.True);
    }

    [Test]
    public void RechargeBadge_RequiresRemainingFirstPurchaseBonus()
    {
        RechargeShopProductData product = Create<RechargeShopProductData>();
        product.ProductId = "gem_pack";
        product.Tab = ShopTabType.Recharge;
        product.IsEnabled = true;
        product.HasFirstPurchaseBonus = true;
        ShopProductGroup group = Create<ShopProductGroup>();
        group.RechargeProducts = new List<ShopProductData> { product };

        string marker = ShopNotificationPolicy.BuildMarker(
            ShopTabType.Recharge,
            group,
            _ => new ShopProductState(true, false, 0, 0, null),
            DateTimeOffset.UtcNow);
        string purchasedMarker = ShopNotificationPolicy.BuildMarker(
            ShopTabType.Recharge,
            group,
            _ => new ShopProductState(true, false, 1, 0, null),
            DateTimeOffset.UtcNow);

        Assert.That(marker, Is.Not.Empty);
        Assert.That(purchasedMarker, Is.Empty);
    }

    private T Create<T>() where T : ScriptableObject
    {
        T asset = ScriptableObject.CreateInstance<T>();
        created.Add(asset);
        return asset;
    }
}
#endif
