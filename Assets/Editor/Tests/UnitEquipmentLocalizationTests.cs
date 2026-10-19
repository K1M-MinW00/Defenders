using NUnit.Framework;
using UnityEngine.Localization.Settings;

public sealed class UnitEquipmentLocalizationTests
{
    [TestCase("ko", "공격력  +150")]
    [TestCase("en", "Attack  +150")]
    public void StatValue_UsesSelectedLocale(string localeCode, string expected)
    {
        SelectLocale(localeCode);
        Assert.That(
            EquipmentLocalizedText.StatValue(EquipmentStatType.Attack, 150),
            Is.EqualTo(expected));
    }

    [TestCase("ko", "치명타 확률  +12.5%")]
    [TestCase("en", "Critical Chance  +12.5%")]
    public void BonusValue_FormatsPercentage(string localeCode, string expected)
    {
        SelectLocale(localeCode);
        Assert.That(
            EquipmentLocalizedText.BonusValue(
                EquipmentStatType.CritChance,
                EquipmentModifierType.Percent,
                12.5f),
            Is.EqualTo(expected));
    }

    [TestCase("ko", "다음 LV.8")]
    [TestCase("en", "Next LV.8")]
    public void NextLevel_UsesSelectedLocale(string localeCode, string expected)
    {
        SelectLocale(localeCode);
        Assert.That(Localize("equipment.enhance.next_level", 8), Is.EqualTo(expected));
    }

    [TestCase("ko", "2진급")]
    [TestCase("en", "Promotion 2")]
    public void Promotion_FormatsTier(string localeCode, string expected)
    {
        SelectLocale(localeCode);
        Assert.That(Localize("unit.promotion", 2), Is.EqualTo(expected));
    }

    [TestCase("ko", "유닛이 이미 최고 품질에 도달했습니다.")]
    [TestCase("en", "This unit has already reached maximum quality.")]
    public void MaximumLimitBreak_UsesSelectedLocale(string localeCode, string expected)
    {
        SelectLocale(localeCode);
        Assert.That(Localize("unit.limit_break.max"), Is.EqualTo(expected));
    }

    private static string Localize(string key, params object[] arguments) =>
        LocalizationService.Get(LocalizationTables.Items, key, arguments);

    private static void SelectLocale(string localeCode)
    {
        LocalizationSettings.InitializationOperation.WaitForCompletion();
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.GetLocale(localeCode);
    }
}
