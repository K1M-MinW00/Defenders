using NUnit.Framework;
using UnityEngine.Localization.Settings;

public sealed class LobbyBattleLocalizationTests
{
    [TestCase("ko", "정복 보상")]
    [TestCase("en", "Conquest\nRewards")]
    public void ConquestRewardShortcut_UsesIntentionalLineBreak(string localeCode, string expected)
    {
        SelectLocale(localeCode);

        string label = LocalizationService.Get(LocalizationTables.Lobby, "shortcut.conquest_rewards");

        Assert.That(label, Is.EqualTo(expected));
    }

    [TestCase("ko", "battle.event.attendance", "출석\n이벤트")]
    [TestCase("en", "battle.event.attendance", "Attendance\nEvent")]
    [TestCase("ko", "battle.event.pass", "배틀\n패스")]
    [TestCase("en", "battle.event.pass", "Battle\nPass")]
    [TestCase("ko", "battle.event.growth", "성장\n이벤트")]
    [TestCase("en", "battle.event.growth", "Growth\nEvent")]
    public void EventShortcut_UsesIntentionalLineBreak(string localeCode, string key, string expected)
    {
        SelectLocale(localeCode);

        Assert.That(LocalizationService.Get(LocalizationTables.Lobby, key), Is.EqualTo(expected));
    }

    [TestCase("ko", "스테이지 입장에는 연료 10이 필요합니다.")]
    [TestCase("en", "You need 10 Fuel to enter this stage.")]
    public void FuelRequirement_UsesSelectedLocaleAndFormatsCost(string localeCode, string expected)
    {
        SelectLocale(localeCode);

        string message = LocalizationService.Get(
            LocalizationTables.Lobby,
            "battle.fuel.insufficient",
            10);

        Assert.That(message, Is.EqualTo(expected));
    }

    [TestCase("ko", "12 / 60")]
    [TestCase("en", "12 / 60")]
    public void FuelStatus_FormatsCurrentAndMaximum(string localeCode, string expected)
    {
        SelectLocale(localeCode);

        string message = LocalizationService.Get(
            LocalizationTables.Lobby,
            "battle.fuel",
            12,
            60);

        Assert.That(message, Is.EqualTo(expected));
    }

    [TestCase("ko", "전투에 참여할 유닛을 편성해 주세요.")]
    [TestCase("en", "Assign units to the battle roster first.")]
    public void EmptyFormationMessage_UsesSelectedLocale(string localeCode, string expected)
    {
        SelectLocale(localeCode);

        string message = LocalizationService.Get(
            LocalizationTables.Lobby,
            "battle.start.empty_formation");

        Assert.That(message, Is.EqualTo(expected));
    }

    private static void SelectLocale(string localeCode)
    {
        LocalizationSettings.InitializationOperation.WaitForCompletion();
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.GetLocale(localeCode);
    }
}
