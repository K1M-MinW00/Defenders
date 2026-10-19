using NUnit.Framework;
using UnityEngine.Localization.Settings;

public sealed class LobbyResourcePanelLocalizationTests
{
    [TestCase("ko", "다음 충전까지 00:12:34")]
    [TestCase("en", "Next Fuel in 00:12:34")]
    public void FuelRecovery_UsesSelectedLocale(string localeCode, string expected)
    {
        SelectLocale(localeCode);

        Assert.That(Localize("fuel.next_recovery", "00:12:34"), Is.EqualTo(expected));
    }

    [TestCase("ko", "섹터 7 기준 시간당 보상")]
    [TestCase("en", "Hourly rewards for Sector 7")]
    public void IdleRewardSector_UsesSelectedLocale(string localeCode, string expected)
    {
        SelectLocale(localeCode);

        Assert.That(Localize("idle.sector_hourly_rewards", 7), Is.EqualTo(expected));
    }

    [TestCase("ko", "2시간 15분 / 최대 12시간")]
    [TestCase("en", "2h 15m / Max 12h")]
    public void IdleRewardTime_UsesSelectedLocale(string localeCode, string expected)
    {
        SelectLocale(localeCode);

        Assert.That(Localize("idle.accumulated_time", 2, 15, 12), Is.EqualTo(expected));
    }

    [TestCase("ko", "1,200<color=#56FF69> (+300)</color> /시간")]
    [TestCase("en", "1,200<color=#56FF69> (+300)</color> /hr")]
    public void IdleRewardRate_UsesSelectedLocale(string localeCode, string expected)
    {
        SelectLocale(localeCode);

        Assert.That(Localize("idle.hourly_rate", 1200, 300), Is.EqualTo(expected));
    }

    private static string Localize(string key, params object[] arguments) =>
        LocalizationService.Get(LocalizationTables.Lobby, key, arguments);

    private static void SelectLocale(string localeCode)
    {
        LocalizationSettings.InitializationOperation.WaitForCompletion();
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.GetLocale(localeCode);
    }
}
