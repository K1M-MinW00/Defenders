using NUnit.Framework;
using UnityEditor.Localization;
using UnityEngine.Localization.Tables;

public sealed class GameSceneHudMonsterLocalizationTests
{
    [TestCase("ko", "hud.stage", "스테이지 {0}-{1}")]
    [TestCase("en", "hud.stage", "Stage {0}-{1}")]
    [TestCase("ko", "hud.wave", "웨이브")]
    [TestCase("en", "hud.wave", "WAVE")]
    [TestCase("ko", "monster.info.title", "몬스터 정보")]
    [TestCase("en", "monster.info.title", "Monster Information")]
    [TestCase("ko", "monster.info.empty", "이 웨이브에는 표시할 몬스터가 없습니다.")]
    [TestCase("en", "monster.info.empty", "There are no monsters to display for this wave.")]
    [TestCase("ko", "monster.name.Elite_Orc", "정예 오크")]
    [TestCase("en", "monster.name.Elite_Orc", "Elite Orc")]
    [TestCase("ko", "monster.name.Skeleton_Archer", "스켈레톤 궁수")]
    [TestCase("en", "monster.name.Skeleton_Archer", "Skeleton Archer")]
    public void Text_UsesSelectedLocale(string localeCode, string key, string expected)
    {
        StringTableCollection collection = LocalizationEditorSettings.GetStringTableCollection(LocalizationTables.Stage);
        StringTable table = collection?.GetTable(localeCode) as StringTable;
        Assert.That(table?.GetEntry(key)?.Value, Is.EqualTo(expected));
    }
}
