using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Localization;
using UnityEngine.Localization;
using UnityEngine.Localization.Tables;

public static class RelicStringTableSetup
{
    private readonly struct Entry
    {
        public Entry(string key, string ko, string en) { Key = key; Ko = ko; En = en; }
        public string Key { get; }
        public string Ko { get; }
        public string En { get; }
    }

    private static readonly Entry[] Entries =
    {
        new("WarlordsSeal.name", "전쟁군주의 인장", "Warlord's Seal"),
        new("WarlordsSeal.short", "아군 공격력 +15%", "Ally Attack +15%"),
        new("WarlordsSeal.description", "이번 스테이지 동안 모든 아군 유닛의 공격력이 15% 증가합니다.", "Increases all allied units' Attack by 15% for this stage."),
        new("WarlordsSeal.symbol", "공", "ATK"),
        new("HawkeyeCrystal.name", "매의 눈 수정", "Hawkeye Crystal"),
        new("HawkeyeCrystal.short", "치명타 확률 +20%p", "Critical Chance +20%p"),
        new("HawkeyeCrystal.description", "이번 스테이지 동안 모든 아군 유닛의 치명타 확률이 20%p 증가합니다.", "Increases all allied units' Critical Chance by 20 percentage points for this stage."),
        new("HawkeyeCrystal.symbol", "치", "CRIT"),
        new("BerserkersSpring.name", "광전사의 태엽", "Berserker's Spring"),
        new("BerserkersSpring.short", "아군 공격 속도 +20%", "Ally Attack Speed +20%"),
        new("BerserkersSpring.description", "이번 스테이지 동안 모든 아군 유닛의 공격 속도가 20% 증가합니다.", "Increases all allied units' Attack Speed by 20% for this stage."),
        new("BerserkersSpring.symbol", "속", "SPD"),
        new("FrostShackles.name", "서리의 족쇄", "Frost Shackles"),
        new("FrostShackles.short", "적 이동 속도 -20%", "Enemy Move Speed -20%"),
        new("FrostShackles.description", "이번 스테이지 동안 모든 적의 이동 속도가 20% 감소합니다.", "Reduces all enemies' Move Speed by 20% for this stage."),
        new("FrostShackles.symbol", "빙", "ICE"),
        new("ReinforcementFlare.name", "지원군 신호탄", "Reinforcement Flare"),
        new("ReinforcementFlare.short", "1성 유닛 1~5명 획득", "Gain 1-5 one-star units"),
        new("ReinforcementFlare.description", "무작위 1성 유닛을 1~5명 즉시 획득합니다. 이 효과는 인구 제한을 무시합니다.", "Immediately gain 1-5 random one-star units, ignoring the population limit."),
        new("ReinforcementFlare.symbol", "증", "REIN"),
        new("EliteSummons.name", "정예 소환장", "Elite Summons"),
        new("EliteSummons.short", "2성 유닛 1명 획득", "Gain one two-star unit"),
        new("EliteSummons.description", "무작위 2성 유닛 1명을 즉시 획득합니다.", "Immediately gain one random two-star unit."),
        new("EliteSummons.symbol", "정", "ELT"),
        new("AbundantVein.name", "풍요의 광맥", "Abundant Vein"),
        new("AbundantVein.short", "광물 10~30개 획득", "Gain 10-30 Minerals"),
        new("AbundantVein.description", "광물을 10~30개 무작위로 즉시 획득합니다.", "Immediately gain a random amount of 10-30 Minerals."),
        new("AbundantVein.symbol", "광", "ORE"),
        new("DiceOfFate.name", "운명의 주사위", "Dice of Fate"),
        new("DiceOfFate.short", "유닛 리롤 10회 무료", "10 free unit rerolls"),
        new("DiceOfFate.description", "이번 스테이지에서 다음 10회의 유닛 리롤 비용이 무료가 됩니다.", "The next 10 unit rerolls are free during this stage."),
        new("DiceOfFate.symbol", "운", "FATE"),
    };

    public static void Configure(Locale korean, Locale english, string tablesDirectory)
    {
        StringTableCollection collection = LocalizationEditorSettings.GetStringTableCollection(LocalizationTables.Relics);
        if (collection == null)
            collection = LocalizationEditorSettings.CreateStringTableCollection(
                LocalizationTables.Relics, tablesDirectory, new List<Locale> { korean, english });
        StringTable ko = GetOrCreate(collection, korean);
        StringTable en = GetOrCreate(collection, english);
        foreach (Entry value in Entries)
        {
            Set(ko, value.Key, value.Ko);
            Set(en, value.Key, value.En);
        }
        EditorUtility.SetDirty(ko);
        EditorUtility.SetDirty(en);
        EditorUtility.SetDirty(collection.SharedData);
    }

    private static StringTable GetOrCreate(StringTableCollection collection, Locale locale) =>
        collection.GetTable(locale.Identifier) as StringTable ?? collection.AddNewTable(locale.Identifier) as StringTable;

    private static void Set(StringTable table, string key, string value)
    {
        StringTableEntry entry = table.GetEntry(key) ?? table.AddEntry(key, value);
        entry.Value = value;
        entry.IsSmart = false;
    }
}
