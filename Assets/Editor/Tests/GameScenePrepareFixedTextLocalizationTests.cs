using NUnit.Framework;
using UnityEditor.Localization;
using UnityEngine.Localization.Tables;

public sealed class GameScenePrepareFixedTextLocalizationTests
{
    [TestCase("ko", "prepare.start", "출발!")]
    [TestCase("en", "prepare.start", "Start!")]
    [TestCase("ko", "prepare.summon", "유닛 생성")]
    [TestCase("en", "prepare.summon", "Summon")]
    [TestCase("ko", "prepare.increase_population", "인원수 증가")]
    [TestCase("en", "prepare.increase_population", "Unit Cap +")]
    [TestCase("ko", "prepare.reroll", "유닛 리롤")]
    [TestCase("en", "prepare.reroll", "Reroll")]
    [TestCase("ko", "prepare.reroll_guide", "리롤할 유닛을 이곳으로 드래그하세요.")]
    [TestCase("en", "prepare.reroll_guide", "Drag a unit here to reroll.")]
    [TestCase("ko", "prepare.sell", "판매")]
    [TestCase("en", "prepare.sell", "Sell")]
    [TestCase("ko", "prepare.sell_guide", "판매할 유닛을 이곳으로 드래그하세요.")]
    [TestCase("en", "prepare.sell_guide", "Drag a unit here to sell.")]
    public void PrepareText_UsesSelectedLocale(string localeCode, string key, string expected)
    {
        StringTableCollection collection = LocalizationEditorSettings.GetStringTableCollection(LocalizationTables.Stage);
        StringTable table = collection?.GetTable(localeCode) as StringTable;
        Assert.That(table?.GetEntry(key)?.Value, Is.EqualTo(expected));
    }
}
