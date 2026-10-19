using NUnit.Framework;
using UnityEngine.Localization.Settings;

public sealed class LobbyShopFixedTextLocalizationTests
{
    [TestCase("ko", "shop.tab.limited", "한정")]
    [TestCase("en", "shop.tab.limited", "Limited")]
    [TestCase("ko", "shop.tab.package", "패키지")]
    [TestCase("en", "shop.tab.package", "Packages")]
    [TestCase("ko", "shop.tab.recharge", "충전")]
    [TestCase("en", "shop.tab.recharge", "Recharge")]
    [TestCase("ko", "shop.tab.exchange", "교환소")]
    [TestCase("en", "shop.tab.exchange", "Exchange")]
    [TestCase("ko", "shop.section.daily_package", "일간 패키지")]
    [TestCase("en", "shop.section.daily_package", "Daily Packages")]
    [TestCase("ko", "shop.section.weekly_package", "주간 패키지")]
    [TestCase("en", "shop.section.weekly_package", "Weekly Packages")]
    [TestCase("ko", "shop.section.monthly_package", "월간 패키지")]
    [TestCase("en", "shop.section.monthly_package", "Monthly Packages")]
    public void ShopFixedText_UsesSelectedLocale(string localeCode, string key, string expected)
    {
        LocalizationSettings.InitializationOperation.WaitForCompletion();
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.GetLocale(localeCode);

        Assert.That(LocalizationService.Get(LocalizationTables.Lobby, key), Is.EqualTo(expected));
    }
}
