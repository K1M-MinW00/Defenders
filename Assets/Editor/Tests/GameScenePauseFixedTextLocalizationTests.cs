using NUnit.Framework;
using UnityEditor.Localization;
using UnityEngine.Localization.Tables;

public sealed class GameScenePauseFixedTextLocalizationTests
{
    [TestCase("ko", "pause.resume", "계속하기")]
    [TestCase("en", "pause.resume", "Continue")]
    [TestCase("ko", "pause.exit", "나가기")]
    [TestCase("en", "pause.exit", "Exit")]
    [TestCase("ko", "pause.push_notifications", "Push 알림")]
    [TestCase("en", "pause.push_notifications", "Push Notifications")]
    [TestCase("ko", "pause.sound", "사운드")]
    [TestCase("en", "pause.sound", "Sound")]
    [TestCase("ko", "pause.combat_formation", "전투 부대")]
    [TestCase("en", "pause.combat_formation", "Combat Formation")]
    public void PauseText_UsesSelectedLocale(string localeCode, string key, string expected)
    {
        StringTableCollection collection = LocalizationEditorSettings.GetStringTableCollection(LocalizationTables.Stage);
        StringTable table = collection?.GetTable(localeCode) as StringTable;
        Assert.That(table?.GetEntry(key)?.Value, Is.EqualTo(expected));
    }
}
