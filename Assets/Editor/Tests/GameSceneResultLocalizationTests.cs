using NUnit.Framework;
using UnityEditor.Localization;
using UnityEngine.Localization.Tables;

public sealed class GameSceneResultLocalizationTests
{
    [TestCase("ko", "result.success", "성공")]
    [TestCase("en", "result.success", "Success")]
    [TestCase("ko", "result.failure", "실패")]
    [TestCase("en", "result.failure", "Failure")]
    [TestCase("ko", "result.rewards", "보상 목록")]
    [TestCase("en", "result.rewards", "Rewards")]
    [TestCase("ko", "result.waves_survived_label", "살아남은 웨이브")]
    [TestCase("en", "result.waves_survived_label", "Waves Survived")]
    [TestCase("ko", "result.refunded_fuel", "반환된 연료")]
    [TestCase("en", "result.refunded_fuel", "Refunded Fuel")]
    [TestCase("ko", "result.refund_description", "작전을 실패하면 연료의 대부분을 반환합니다.")]
    [TestCase("en", "result.refund_description", "Most of the Fuel is refunded when an operation fails.")]
    public void FixedText_UsesSelectedLocale(string localeCode, string key, string expected)
    {
        StringTableCollection collection = LocalizationEditorSettings.GetStringTableCollection(LocalizationTables.Stage);
        StringTable table = collection?.GetTable(localeCode) as StringTable;
        Assert.That(table?.GetEntry(key)?.Value, Is.EqualTo(expected));
    }
}
