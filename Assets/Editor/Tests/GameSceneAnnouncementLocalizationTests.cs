using NUnit.Framework;
using UnityEditor.Localization;
using UnityEngine.Localization.Tables;

public sealed class GameSceneAnnouncementLocalizationTests
{
    [TestCase("ko", "announcement.sector", "섹터")]
    [TestCase("en", "announcement.sector", "Sector")]
    [TestCase("ko", "announcement.boss_wave", "보스 웨이브")]
    [TestCase("en", "announcement.boss_wave", "Boss Wave")]
    public void AnnouncementText_UsesSelectedLocale(string localeCode, string key, string expected)
    {
        StringTableCollection collection = LocalizationEditorSettings.GetStringTableCollection(LocalizationTables.Stage);
        StringTable table = collection?.GetTable(localeCode) as StringTable;
        Assert.That(table?.GetEntry(key)?.Value, Is.EqualTo(expected));
    }
}
