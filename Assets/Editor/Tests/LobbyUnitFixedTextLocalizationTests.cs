using NUnit.Framework;
using UnityEngine.Localization.Settings;

public sealed class LobbyUnitFixedTextLocalizationTests
{
    [TestCase("ko", "unit.title", "유닛")]
    [TestCase("en", "unit.title", "Units")]
    [TestCase("ko", "unit.roster.battle", "전투 부대")]
    [TestCase("en", "unit.roster.battle", "Battle Roster")]
    [TestCase("ko", "unit.roster.reserve", "대기 명단")]
    [TestCase("en", "unit.roster.reserve", "Reserve Roster")]
    [TestCase("ko", "unit.button.codex", "도감")]
    [TestCase("en", "unit.button.codex", "Codex")]
    [TestCase("ko", "unit.button.formation", "편성")]
    [TestCase("en", "unit.button.formation", "Formation")]
    [TestCase("ko", "unit.stat.attack", "공격력")]
    [TestCase("en", "unit.stat.attack", "Attack")]
    [TestCase("ko", "unit.stat.health", "체력")]
    [TestCase("en", "unit.stat.health", "HP")]
    [TestCase("ko", "unit.training.select_materials", "재료 선택")]
    [TestCase("en", "unit.training.select_materials", "Select Materials")]
    [TestCase("ko", "unit.training.level_one", "+1 레벨")]
    [TestCase("en", "unit.training.level_one", "+1 Level")]
    [TestCase("ko", "unit.training.level_max", "최대 레벨")]
    [TestCase("en", "unit.training.level_max", "Max Level")]
    [TestCase("ko", "unit.training.title", "훈련")]
    [TestCase("en", "unit.training.title", "Training")]
    public void FixedUnitText_UsesSelectedLocale(string localeCode, string key, string expected)
    {
        LocalizationSettings.InitializationOperation.WaitForCompletion();
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.GetLocale(localeCode);

        Assert.That(LocalizationService.Get(LocalizationTables.Lobby, key), Is.EqualTo(expected));
    }
}
