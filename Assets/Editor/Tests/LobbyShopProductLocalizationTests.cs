using NUnit.Framework;
using UnityEngine.Localization.Settings;

public sealed class LobbyShopProductLocalizationTests
{
    [TestCase("ko", "첫 구매 2배")]
    [TestCase("en", "First Purchase x2")]
    public void FirstPurchaseBonus_UsesSelectedLocale(string localeCode, string expected)
    {
        SelectLocale(localeCode);
        Assert.That(Localize("shop.first_purchase_bonus"), Is.EqualTo(expected));
    }
    [TestCase("ko", "구매 가능 2/5")]
    [TestCase("en", "Available: 2/5")]
    public void RemainingPurchases_UsesSelectedLocale(string localeCode, string expected)
    {
        SelectLocale(localeCode);
        Assert.That(Localize("shop.remaining_purchases", 2, 5), Is.EqualTo(expected));
    }

    [TestCase("ko", "품절")]
    [TestCase("en", "Sold Out")]
    public void SoldOutStatus_UsesSelectedLocale(string localeCode, string expected)
    {
        SelectLocale(localeCode);
        Assert.That(Localize("shop.status.sold_out"), Is.EqualTo(expected));
    }

    [TestCase("ko", "젬 1,200")]
    [TestCase("en", "1,200 Gems")]
    public void GemCost_FormatsAmount(string localeCode, string expected)
    {
        SelectLocale(localeCode);
        Assert.That(Localize("shop.cost.gems", 1200), Is.EqualTo(expected));
    }

    [TestCase("ko", "광고 보기")]
    [TestCase("en", "Watch Ad")]
    public void AdCost_UsesSelectedLocale(string localeCode, string expected)
    {
        SelectLocale(localeCode);
        Assert.That(Localize("shop.cost.ad"), Is.EqualTo(expected));
    }

    [TestCase("ko", "재고 갱신 중")]
    [TestCase("en", "Refreshing stock")]
    public void StockRefreshing_UsesSelectedLocale(string localeCode, string expected)
    {
        SelectLocale(localeCode);
        Assert.That(Localize("shop.stock.refreshing"), Is.EqualTo(expected));
    }

    [TestCase("ko", "2일 7시간")]
    [TestCase("en", "2d 7h")]
    public void StockRemainingDays_FormatsDuration(string localeCode, string expected)
    {
        SelectLocale(localeCode);
        Assert.That(Localize("shop.stock.remaining_days", 2, 7), Is.EqualTo(expected));
    }

    private static string Localize(string key, params object[] arguments) =>
        LocalizationService.Get(LocalizationTables.Lobby, key, arguments);

    private static void SelectLocale(string localeCode)
    {
        LocalizationSettings.InitializationOperation.WaitForCompletion();
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.GetLocale(localeCode);
    }
}
