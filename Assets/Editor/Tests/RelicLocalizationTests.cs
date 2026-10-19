using NUnit.Framework;
using UnityEngine.Localization.Settings;

public sealed class RelicLocalizationTests
{
    [TestCase("ko", "전쟁군주의 인장")]
    [TestCase("en", "Warlord's Seal")]
    public void RelicName_UsesSelectedLocale(string localeCode, string expected)
    { SelectLocale(localeCode); Assert.That(StageRelicCatalog.All[0].Name, Is.EqualTo(expected)); }

    [TestCase("ko", "적 이동 속도 -20%")]
    [TestCase("en", "Enemy Move Speed -20%")]
    public void RelicSummary_UsesSelectedLocale(string localeCode, string expected)
    { SelectLocale(localeCode); Assert.That(StageRelicCatalog.All[3].ShortDescription, Is.EqualTo(expected)); }

    [TestCase("ko", "무작위 2성 유닛 1명을 즉시 획득합니다.")]
    [TestCase("en", "Immediately gain one random two-star unit.")]
    public void RelicDescription_UsesSelectedLocale(string localeCode, string expected)
    { SelectLocale(localeCode); Assert.That(StageRelicCatalog.All[5].Description, Is.EqualTo(expected)); }

    [TestCase("ko", "운")]
    [TestCase("en", "FATE")]
    public void RelicFallbackSymbol_UsesSelectedLocale(string localeCode, string expected)
    { SelectLocale(localeCode); Assert.That(StageRelicCatalog.All[7].Symbol, Is.EqualTo(expected)); }

    private static void SelectLocale(string localeCode)
    {
        LocalizationSettings.InitializationOperation.WaitForCompletion();
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.GetLocale(localeCode);
    }
}
