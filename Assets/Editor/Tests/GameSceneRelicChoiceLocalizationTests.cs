using NUnit.Framework;
using UnityEditor.Localization;
using UnityEngine.Localization.Tables;

public sealed class GameSceneRelicChoiceLocalizationTests
{
    [TestCase("ko", "relic_choice.title", "유물을 선택하세요.")]
    [TestCase("en", "relic_choice.title", "Choose a Relic")]
    [TestCase("ko", "relic_choice.select", "선택")]
    [TestCase("en", "relic_choice.select", "Select")]
    public void RelicChoiceText_UsesSelectedLocale(string localeCode, string key, string expected)
    {
        StringTableCollection collection = LocalizationEditorSettings.GetStringTableCollection(LocalizationTables.Stage);
        StringTable table = collection?.GetTable(localeCode) as StringTable;
        Assert.That(table?.GetEntry(key)?.Value, Is.EqualTo(expected));
    }
}
