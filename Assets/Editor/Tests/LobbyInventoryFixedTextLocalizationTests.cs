using NUnit.Framework;
using UnityEngine.Localization.Settings;

public sealed class LobbyInventoryFixedTextLocalizationTests
{
    [TestCase("ko", "inventory.title", "가방")]
    [TestCase("en", "inventory.title", "Inventory")]
    [TestCase("ko", "inventory.tab.items", "아이템")]
    [TestCase("en", "inventory.tab.items", "Items")]
    [TestCase("ko", "inventory.tab.equipment", "장비")]
    [TestCase("en", "inventory.tab.equipment", "Equipment")]
    [TestCase("ko", "inventory.tab.materials", "재료")]
    [TestCase("en", "inventory.tab.materials", "Materials")]
    [TestCase("ko", "inventory.filter.all", "All")]
    [TestCase("en", "inventory.filter.all", "All")]
    [TestCase("ko", "inventory.filter.reset", "Reset")]
    [TestCase("en", "inventory.filter.reset", "Reset")]
    public void InventoryFixedText_UsesSelectedLocale(string localeCode, string key, string expected)
    {
        LocalizationSettings.InitializationOperation.WaitForCompletion();
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.GetLocale(localeCode);

        Assert.That(LocalizationService.Get(LocalizationTables.Lobby, key), Is.EqualTo(expected));
    }
}
