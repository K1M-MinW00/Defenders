using NUnit.Framework;
using UnityEditor;
using UnityEngine.Localization.Settings;

public sealed class LabCardLocalizationTests
{
    [OneTimeSetUp]
    public void InitializeLocalization() => LocalizationSettings.InitializationOperation.WaitForCompletion();

    [TestCase("ko", "화력 증폭", "아군 유닛의 공격력이 증가합니다.")]
    [TestCase("en", "Firepower Amplification", "Increases allied units' Attack.")]
    public void LabCard_UsesSelectedLocale(string localeCode, string expectedName, string expectedDescription)
    {
        LabCardDataSO card = AssetDatabase.LoadAssetAtPath<LabCardDataSO>(
            "Assets/Resources/GameData/LabCards/attack.asset");
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.GetLocale(localeCode);
        Assert.That(card.LocalizedName, Is.EqualTo(expectedName));
        Assert.That(card.LocalizedDescription, Is.EqualTo(expectedDescription));
    }
}
