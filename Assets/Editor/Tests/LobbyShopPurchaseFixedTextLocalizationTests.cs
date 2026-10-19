using NUnit.Framework;
using UnityEngine.Localization.Settings;

public sealed class LobbyShopPurchaseFixedTextLocalizationTests
{
    [TestCase("ko", "shop.purchase.title", "상품 구매")]
    [TestCase("en", "shop.purchase.title", "Purchase Product")]
    [TestCase("ko", "shop.purchase.buy", "구매")]
    [TestCase("en", "shop.purchase.buy", "Purchase")]
    [TestCase("ko", "shop.purchase.cancel", "취소")]
    [TestCase("en", "shop.purchase.cancel", "Cancel")]
    [TestCase("ko", "shop.purchase.recharge", "Gem 충전")]
    [TestCase("en", "shop.purchase.recharge", "Recharge Gems")]
    [TestCase("ko", "shop.purchase.close", "확인")]
    [TestCase("en", "shop.purchase.close", "OK")]
    public void ShopPurchaseText_UsesSelectedLocale(string localeCode, string key, string expected)
    {
        LocalizationSettings.InitializationOperation.WaitForCompletion();
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.GetLocale(localeCode);

        Assert.That(LocalizationService.Get(LocalizationTables.Lobby, key), Is.EqualTo(expected));
    }
}
