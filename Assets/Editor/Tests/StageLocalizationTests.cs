using NUnit.Framework;
using UnityEngine.Localization.Settings;

public sealed class StageLocalizationTests
{
    [TestCase("ko", "무료 (3회)")]
    [TestCase("en", "Free (3)")]
    public void FreeReroll_FormatsCount(string localeCode, string expected)
    { SelectLocale(localeCode); Assert.That(Localize("hud.free_count", 3), Is.EqualTo(expected)); }

    [TestCase("ko", "남은 몬스터 수: 07")]
    [TestCase("en", "Monsters Remaining: 07")]
    public void RemainingMonsters_FormatsCount(string localeCode, string expected)
    { SelectLocale(localeCode); Assert.That(Localize("hud.monsters.remaining", 7), Is.EqualTo(expected)); }

    [TestCase("ko", "스테이지 2-3")]
    [TestCase("en", "Stage 2-3")]
    public void StageResult_FormatsStageKey(string localeCode, string expected)
    { SelectLocale(localeCode); Assert.That(Localize("result.stage", "2-3"), Is.EqualTo(expected)); }

    [TestCase("ko", "다시 시도")]
    [TestCase("en", "Retry")]
    public void Retry_UsesSelectedLocale(string localeCode, string expected)
    { SelectLocale(localeCode); Assert.That(Localize("result.retry"), Is.EqualTo(expected)); }

    [TestCase("ko", "3 / 10")]
    [TestCase("en", "3 / 10")]
    public void ResultWaves_FormatsProgress(string localeCode, string expected)
    { SelectLocale(localeCode); Assert.That(Localize("result.waves_survived", 3, 10), Is.EqualTo(expected)); }

    [TestCase("ko", "광물이 모자랍니다.")]
    [TestCase("en", "Not enough minerals.")]
    public void InsufficientMinerals_UsesSelectedLocale(string localeCode, string expected)
    { SelectLocale(localeCode); Assert.That(Localize("prepare.insufficient_minerals"), Is.EqualTo(expected)); }

    private static string Localize(string key, params object[] arguments) =>
        LocalizationService.Get(LocalizationTables.Stage, key, arguments);

    private static void SelectLocale(string localeCode)
    {
        LocalizationSettings.InitializationOperation.WaitForCompletion();
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.GetLocale(localeCode);
    }
}
