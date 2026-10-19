using NUnit.Framework;
using UnityEngine.Localization.Settings;

public sealed class LobbyQuestLocalizationTests
{
    [TestCase("ko", "quest.title.daily", "일일 퀘스트")]
    [TestCase("en", "quest.title.daily", "Daily Quests")]
    [TestCase("ko", "quest.rewards.daily", "일일 보상")]
    [TestCase("en", "quest.rewards.daily", "Daily Rewards")]
    [TestCase("ko", "quest.rewards.weekly", "주간 보상")]
    [TestCase("en", "quest.rewards.weekly", "Weekly Rewards")]
    public void FixedQuestText_UsesSelectedLocale(string localeCode, string key, string expected)
    {
        SelectLocale(localeCode);
        Assert.That(Localize(key), Is.EqualTo(expected));
    }

    [TestCase("ko", "퀘스트 점수 20점을 획득했습니다.")]
    [TestCase("en", "Claimed 20 quest points.")]
    public void ClaimedScore_FormatsPoints(string localeCode, string expected)
    {
        SelectLocale(localeCode);
        Assert.That(Localize("quest.score.claimed", 20), Is.EqualTo(expected));
    }

    [TestCase("ko", "5시간 07분")]
    [TestCase("en", "5h 07m")]
    public void Timer_FormatsHoursAndMinutes(string localeCode, string expected)
    {
        SelectLocale(localeCode);
        Assert.That(Localize("quest.timer", 5, 7), Is.EqualTo(expected));
    }

    [TestCase("ko", "획득")]
    [TestCase("en", "Claim")]
    public void ClaimAction_UsesSelectedLocale(string localeCode, string expected)
    {
        SelectLocale(localeCode);
        Assert.That(Localize("quest.action.claim"), Is.EqualTo(expected));
    }

    [TestCase("ko", "획득한 보상")]
    [TestCase("en", "Rewards Acquired")]
    public void AcquiredRewardTitle_UsesSelectedLocale(string localeCode, string expected)
    {
        SelectLocale(localeCode);
        Assert.That(Localize("quest.reward.acquired_title"), Is.EqualTo(expected));
    }

    [TestCase("ko", "로그인 1회")]
    [TestCase("en", "Log in once")]
    public void LoginQuestName_UsesSelectedLocale(string localeCode, string expected)
    {
        SelectLocale(localeCode);
        Assert.That(Localize("quest.definition.daily_login"), Is.EqualTo(expected));
    }

    [TestCase("ko", "연료 150개 소모")]
    [TestCase("en", "Spend 150 Fuel")]
    public void FuelQuestName_UsesSelectedLocale(string localeCode, string expected)
    {
        SelectLocale(localeCode);
        Assert.That(Localize("quest.definition.daily_fuel_150"), Is.EqualTo(expected));
    }

    private static string Localize(string key, params object[] arguments) =>
        LocalizationService.Get(LocalizationTables.Lobby, key, arguments);

    private static void SelectLocale(string localeCode)
    {
        LocalizationSettings.InitializationOperation.WaitForCompletion();
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.GetLocale(localeCode);
    }
}
