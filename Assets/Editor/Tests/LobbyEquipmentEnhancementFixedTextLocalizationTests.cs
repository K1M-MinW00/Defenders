using NUnit.Framework;
using UnityEngine.Localization.Settings;

public sealed class LobbyEquipmentEnhancementFixedTextLocalizationTests
{
    [TestCase("ko", "equipment.enhancement.title", "장비 강화")]
    [TestCase("en", "equipment.enhancement.title", "Equipment Enhancement")]
    [TestCase("ko", "equipment.enhancement.level_up", "레벨업")]
    [TestCase("en", "equipment.enhancement.level_up", "Level Up")]
    [TestCase("ko", "equipment.enhancement.reset", "되돌리기")]
    [TestCase("en", "equipment.enhancement.reset", "Reset")]
    [TestCase("ko", "equipment.enhancement.auto_select", "자동 선택")]
    [TestCase("en", "equipment.enhancement.auto_select", "Auto Select")]
    [TestCase("ko", "equipment.enhancement.select_materials", "강화석을 선택하세요")]
    [TestCase("en", "equipment.enhancement.select_materials", "Select enhancement stones.")]
    public void EnhancementText_UsesSelectedLocale(string localeCode, string key, string expected)
    {
        LocalizationSettings.InitializationOperation.WaitForCompletion();
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.GetLocale(localeCode);

        Assert.That(LocalizationService.Get(LocalizationTables.Lobby, key), Is.EqualTo(expected));
    }
}
