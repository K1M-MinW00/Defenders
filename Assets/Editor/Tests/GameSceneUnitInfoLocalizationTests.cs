using NUnit.Framework;
using UnityEditor;
using UnityEditor.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;

public sealed class GameSceneUnitInfoLocalizationTests
{
    [TestCase("ko", "unit_info.active", "액티브")]
    [TestCase("en", "unit_info.active", "Active")]
    [TestCase("ko", "unit_info.passive", "패시브")]
    [TestCase("en", "unit_info.passive", "Passive")]
    public void Header_UsesSelectedLocale(string localeCode, string key, string expected)
    {
        StringTableCollection collection = LocalizationEditorSettings.GetStringTableCollection(LocalizationTables.Stage);
        StringTable table = collection?.GetTable(localeCode) as StringTable;
        Assert.That(table?.GetEntry(key)?.Value, Is.EqualTo(expected));
    }

    [TestCase("ko", "관통 화살로 적을 공격한다.")]
    [TestCase("en", "Fires a piercing arrow.")]
    public void SkillUpgradeDescription_UsesSelectedLocale(string localeCode, string expected)
    {
        LocalizationSettings.InitializationOperation.WaitForCompletion();
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.GetLocale(localeCode);
        SkillDataSO skill = AssetDatabase.LoadAssetAtPath<SkillDataSO>("Assets/GameData/Skills/Archer_Active.asset");
        Assert.That(skill.GetLocalizedUpgradeDescription(0), Is.EqualTo(expected));
    }
}
