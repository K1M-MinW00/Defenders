using NUnit.Framework;
using UnityEngine.Localization.Settings;

public sealed class LobbyUnitProgressionFixedTextLocalizationTests
{
    [TestCase("ko", "unit.nav.promotion", "진급")]
    [TestCase("en", "unit.nav.promotion", "Promotion")]
    [TestCase("ko", "unit.nav.limit_break", "한계돌파")]
    [TestCase("en", "unit.nav.limit_break", "Limit Break")]
    [TestCase("ko", "unit.nav.equipment", "장비")]
    [TestCase("en", "unit.nav.equipment", "Equipment")]
    [TestCase("ko", "unit.promotion.effects", "진급 효과")]
    [TestCase("en", "unit.promotion.effects", "Promotion Effects")]
    [TestCase("ko", "unit.promotion.cost", "소모 재화")]
    [TestCase("en", "unit.promotion.cost", "Cost")]
    [TestCase("ko", "unit.skill.active", "액티브")]
    [TestCase("en", "unit.skill.active", "Active")]
    [TestCase("ko", "unit.skill.passive", "패시브")]
    [TestCase("en", "unit.skill.passive", "Passive")]
    [TestCase("ko", "unit.limit_break.effects", "한계돌파 효과")]
    [TestCase("en", "unit.limit_break.effects", "Limit Break Effects")]
    public void ProgressionText_UsesSelectedLocale(string localeCode, string key, string expected)
    {
        LocalizationSettings.InitializationOperation.WaitForCompletion();
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.GetLocale(localeCode);

        Assert.That(LocalizationService.Get(LocalizationTables.Lobby, key), Is.EqualTo(expected));
    }
}
