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
    [TestCase("ko", "monster.name.Wraith_Shaman", "망령 주술사")]
    [TestCase("en", "monster.name.Wraith_Shaman", "Wraith Shaman")]
    [TestCase("ko", "monster.description.Wraith_Shaman", "전열 뒤에서 영혼불을 발사하며 천천히 압박하는 튼튼한 중거리 언데드 주술사")]
    [TestCase("en", "monster.description.Wraith_Shaman", "A durable mid-range undead shaman that pressures defenders with soulfire from behind the front line.")]
    public void Text_UsesSelectedLocale(string localeCode, string key, string expected)
    {
        StringTableCollection collection = LocalizationEditorSettings.GetStringTableCollection(LocalizationTables.Stage);
        StringTable table = collection?.GetTable(localeCode) as StringTable;
        Assert.That(table?.GetEntry(key)?.Value, Is.EqualTo(expected));
    }
}
