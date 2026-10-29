using NUnit.Framework;
using UnityEngine.Localization.Settings;

public sealed class LobbyBattleFixedTextLocalizationTests
{
    [TestCase("ko", "battle.button.challenge", "도전")]
    [TestCase("en", "battle.button.challenge", "Challenge")]
    [TestCase("ko", "battle.button.go", "Go")]
    [TestCase("en", "battle.button.go", "Go")]
    [TestCase("ko", "battle.button.guide", "가이드")]
    [TestCase("en", "battle.button.guide", "Guide")]
    [TestCase("ko", "battle.event.attendance", "출석\n이벤트")]
    [TestCase("en", "battle.event.attendance", "Attendance\nEvent")]
    [TestCase("ko", "battle.event.pass", "배틀\n패스")]
    [TestCase("en", "battle.event.pass", "Battle\nPass")]
    [TestCase("ko", "battle.event.growth", "성장\n이벤트")]
    [TestCase("en", "battle.event.growth", "Growth\nEvent")]
    [TestCase("ko", "battle.event.privilege", "특권")]
    [TestCase("en", "battle.event.privilege", "Privileges")]
    public void BattleFixedText_UsesSelectedLocale(string localeCode, string key, string expected)
    {
        LocalizationSettings.InitializationOperation.WaitForCompletion();
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.GetLocale(localeCode);

        Assert.That(LocalizationService.Get(LocalizationTables.Lobby, key), Is.EqualTo(expected));
    }
}
