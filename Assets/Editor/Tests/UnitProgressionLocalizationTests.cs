using NUnit.Framework;
using UnityEditor;
using UnityEngine.Localization.Settings;

public sealed class UnitProgressionLocalizationTests
{
    [OneTimeSetUp]
    public void InitializeLocalization() => LocalizationSettings.InitializationOperation.WaitForCompletion();

    [TestCase("ko", "섹터\n1-3")]
    [TestCase("en", "Sector\n1-3")]
    public void BattleStage_UsesSelectedLocale(string localeCode, string expected)
    {
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.GetLocale(localeCode);
        Assert.That(LocalizationService.Get(LocalizationTables.Lobby, "battle.stage", 1, 3), Is.EqualTo(expected));
    }

    [TestCase("ko", "Archer.asset", "Archer")]
    [TestCase("en", "Archer.asset", "Archer")]
    [TestCase("ko", "Knight Templar.asset", "Knight Templar")]
    [TestCase("en", "Knight Templar.asset", "Knight Templar")]
    public void UnitName_UsesEnglishNameForBothLocales(string localeCode, string assetName, string expected)
    {
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.GetLocale(localeCode);
        UnitDataSO unit = AssetDatabase.LoadAssetAtPath<UnitDataSO>($"Assets/Resources/GameData/Units/{assetName}");

        Assert.That(unit.LocalizedName, Is.EqualTo(expected));
    }

    [TestCase("ko", "관통 일격")]
    [TestCase("en", "Piercing Shot")]
    public void SkillName_UsesSelectedLocale(string localeCode, string expected)
    {
        SkillDataSO skill = AssetDatabase.LoadAssetAtPath<SkillDataSO>("Assets/GameData/Units/Archer/Skills/Archer_Active.asset");
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.GetLocale(localeCode);
        Assert.That(skill.LocalizedName, Is.EqualTo(expected));
    }

    [TestCase("ko", "공격력 10% 증가")]
    [TestCase("en", "Attack +10%")]
    public void LimitBreakEffect_UsesSelectedLocale(string localeCode, string expected)
    {
        UnitDataSO unit = AssetDatabase.LoadAssetAtPath<UnitDataSO>("Assets/Resources/GameData/Units/Archer.asset");
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.GetLocale(localeCode);
        Assert.That(unit.GetLocalizedLimitBreakDescription(0), Is.EqualTo(expected));
    }
}
