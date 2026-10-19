using NUnit.Framework;
using UnityEngine.Localization.Settings;

public sealed class LobbyWaveRewardLocalizationTests
{
    [TestCase("ko", "스테이지\n2-4")]
    [TestCase("en", "Stage\n2-4")]
    public void StageTitle_FormatsStageKey(string localeCode, string expected)
    {
        SelectLocale(localeCode);
        Assert.That(Localize("wave_reward.stage_title", "2-4"), Is.EqualTo(expected));
    }

    [TestCase("ko", "웨이브\n<size=150%>20</size>")]
    [TestCase("en", "WAVE\n<size=150%>20</size>")]
    public void WaveLabel_FormatsRequiredWave(string localeCode, string expected)
    {
        SelectLocale(localeCode);
        Assert.That(Localize("wave_reward.wave", 20), Is.EqualTo(expected));
    }

    [TestCase("ko", "수령 완료")]
    [TestCase("en", "Claimed")]
    public void ClaimedState_UsesSelectedLocale(string localeCode, string expected)
    {
        SelectLocale(localeCode);
        Assert.That(Localize("wave_reward.state.claimed"), Is.EqualTo(expected));
    }

    [TestCase("ko", "잠김")]
    [TestCase("en", "Locked")]
    public void LockedState_UsesSelectedLocale(string localeCode, string expected)
    {
        SelectLocale(localeCode);
        Assert.That(Localize("wave_reward.state.locked"), Is.EqualTo(expected));
    }

    [TestCase("ko", "웨이브 달성 보상을 받았습니다.")]
    [TestCase("en", "Wave milestone rewards claimed.")]
    public void ClaimSuccess_UsesSelectedLocale(string localeCode, string expected)
    {
        SelectLocale(localeCode);
        Assert.That(Localize("wave_reward.claim.succeeded"), Is.EqualTo(expected));
    }

    [TestCase("ko", "이미 받은 보상입니다.")]
    [TestCase("en", "This reward has already been claimed.")]
    public void AlreadyClaimed_UsesSelectedLocale(string localeCode, string expected)
    {
        SelectLocale(localeCode);
        Assert.That(Localize("wave_reward.claim.already_claimed"), Is.EqualTo(expected));
    }

    private static string Localize(string key, params object[] arguments) =>
        LocalizationService.Get(LocalizationTables.Lobby, key, arguments);

    private static void SelectLocale(string localeCode)
    {
        LocalizationSettings.InitializationOperation.WaitForCompletion();
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.GetLocale(localeCode);
    }
}
