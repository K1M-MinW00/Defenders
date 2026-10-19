using NUnit.Framework;
using UnityEngine.Localization.Settings;

public sealed class LobbyEquipmentDetailFixedTextLocalizationTests
{
    [TestCase("ko", "unit.nav.equipment", "장비")]
    [TestCase("en", "unit.nav.equipment", "Equipment")]
    [TestCase("ko", "equipment.button.unequip", "해제")]
    [TestCase("en", "equipment.button.unequip", "Unequip")]
    [TestCase("ko", "equipment.button.enhance", "강화")]
    [TestCase("en", "equipment.button.enhance", "Enhance")]
    [TestCase("ko", "equipment.button.replace", "장비 교체")]
    [TestCase("en", "equipment.button.replace", "Replace")]
    public void EquipmentDetailText_UsesSelectedLocale(string localeCode, string key, string expected)
    {
        LocalizationSettings.InitializationOperation.WaitForCompletion();
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.GetLocale(localeCode);

        Assert.That(LocalizationService.Get(LocalizationTables.Lobby, key), Is.EqualTo(expected));
    }
}
