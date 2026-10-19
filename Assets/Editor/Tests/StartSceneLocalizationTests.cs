using NUnit.Framework;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public sealed class StartSceneLocalizationTests
{
    [TestCase("ko", "startup.action.start", "시작")]
    [TestCase("en", "startup.action.start", "Start")]
    [TestCase("ko", "startup.action.retry", "재시도")]
    [TestCase("en", "startup.action.retry", "Retry")]
    [TestCase("ko", "startup.status.loading_user_data", "사용자 데이터 불러오는 중...")]
    [TestCase("en", "startup.status.loading_user_data", "Loading User Data...")]
    [TestCase("ko", "startup.error.user_data_load_failed", "사용자 데이터를 불러오지 못했습니다.")]
    [TestCase("en", "startup.error.user_data_load_failed", "Failed to load user data.")]
    public void StartupText_UsesSelectedLocale(string localeCode, string key, string expected)
    {
        LocalizationSettings.InitializationOperation.WaitForCompletion();
        Locale locale = LocalizationSettings.AvailableLocales.GetLocale(localeCode);
        Assert.That(locale, Is.Not.Null);
        LocalizationSettings.SelectedLocale = locale;
        Assert.That(LocalizationService.Get(LocalizationTables.Common, key), Is.EqualTo(expected));
    }
}
