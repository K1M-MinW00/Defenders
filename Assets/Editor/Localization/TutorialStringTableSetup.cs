using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Localization;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine.Localization;
using UnityEngine.Localization.Tables;

public static class TutorialStringTableSetup
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
        new("battle.summon_wave_1", "유닛을 생성해 보세요.", "Create your first unit."),
        new("battle.move_wave_1", "유닛을 드래그해 원하는 위치에 배치해 보세요.", "Drag the unit to place it where you want."),
        new("battle.start_wave_1", "준비가 끝났습니다. 웨이브 1을 시작하세요.", "Your unit is ready. Start Wave 1."),
        new("battle.wait_wave_1_clear", "유닛이 적을 막아내는 모습을 지켜보세요.", "Watch your unit defend the field."),
        new("battle.summon_wave_2", "유닛을 하나 더 생성해 보세요.", "Create another unit."),
        new("battle.wait_wave_2_fusion", "같은 1성 유닛 두 기가 자동으로 합성됩니다.", "Two matching 1-star units fuse automatically."),
        new("battle.start_wave_2", "2성 유닛과 함께 웨이브 2를 시작하세요.", "Start Wave 2 with your 2-star unit."),
        new("battle.wait_wave_2_clear", "성급이 오르면 유닛이 더욱 강해집니다.", "Higher-star units are much stronger."),
        new("battle.summon_wave_3", "3성 합성을 준비하기 위해 유닛을 하나 더 생성하세요.", "Create another unit to prepare a 3-star fusion."),
        new("battle.start_wave_3", "엘리트 웨이브를 시작하세요.", "Start the Elite wave."),
        new("battle.wait_elite_clear", "엘리트 적을 처치하면 특별한 유물을 획득할 수 있습니다.", "Defeat the Elite enemies to earn a relic."),
        new("battle.choose_relic", "원하는 유물 하나를 선택하세요.", "Choose one relic."),
        new("battle.summon_post_relic", "유닛을 하나 더 생성해 보세요.", "Create another unit."),
        new("battle.reroll", "{unitB}을(를) 끌어서 리롤 영역에 놓으세요.", "Drag {unitB} onto the Reroll area."),
        new("battle.wait_three_star_fusion", "리롤로 얻은 {unitA}이(가) 자동 합성되어 3성이 됩니다.", "The rerolled {unitA} units automatically fuse into a 3-star unit."),
        new("battle.increase_population", "인구수를 늘리면 더 많은 유닛을 배치할 수 있습니다.", "Increase population to deploy more units."),
        new("battle.free_play", "이제 남은 웨이브를 자유롭게 진행하세요.", "Continue through the remaining waves freely."),
        new("battle.clear", "첫 전투에서 승리했습니다!", "You won your first battle!"),
        new("battle.return_to_lobby", "돌아가기 버튼을 눌러 로비로 이동하세요.", "Press Return to go to the Lobby."),

        new("lobby.unit_tab", "유닛 탭에서 전투에 참여할 부대와 보유 유닛을 관리할 수 있습니다. 유닛 탭을 눌러보세요.", "The Unit tab manages your battle formation and owned units. Open it now."),
        new("lobby.owned_unit", "보유 유닛을 선택하면 훈련, 진급, 한계돌파와 장비를 확인할 수 있습니다. 유닛 하나를 선택하세요.", "Select an owned unit to review Training, Promotion, Limit Break, and Equipment."),
        new("lobby.training", "훈련 재료를 선택하고 훈련하여 유닛을 한 번 성장시켜 보세요.", "Select training materials and train the unit once."),
        new("lobby.promotion", "진급은 전용 재료로 능력치와 스킬을 강화합니다. 진급 탭을 눌러 확인하세요.", "Promotion improves stats and skills with dedicated materials. Open the Promotion tab."),
        new("lobby.limit_break", "한계돌파는 중복 유닛을 사용해 성장 한계를 확장합니다. 한계돌파 탭을 눌러 확인하세요.", "Limit Break uses duplicate units to expand growth potential. Open the Limit Break tab."),
        new("lobby.equipment", "장비는 유닛의 능력치를 추가로 강화합니다. 장비 탭을 눌러 확인하세요.", "Equipment provides additional stat upgrades. Open the Equipment tab."),
        new("lobby.close_detail", "획득한 장비를 이곳에서 착용하고 강화할 수 있습니다. 확인을 마쳤다면 유닛 상세 화면을 닫으세요.", "Equip and enhance acquired gear here. Close the unit details when you are ready."),
        new("lobby.laboratory_unlocked", "연구소가 해금되었습니다. 연구소에서 새로운 연구를 개발해 전투 능력을 강화할 수 있습니다.", "The Laboratory is unlocked. Develop research there to strengthen your combat abilities."),
        new("lobby.laboratory_explanation", "연구 재화를 사용해 연구 후보를 확인하고 하나를 선택할 수 있습니다. 계정 레벨이 오를수록 더 많은 연구 기회가 열립니다.", "Spend research currency to reveal candidates and choose one. More research opportunities unlock as your account level rises."),

        new("level_up.title", "레벨업", "LEVEL UP"),
        new("level_up.max_fuel", "연료 최대치", "MAX FUEL"),
        new("level_up.research_unlocked", "새로운 연구를 할 수 있습니다.", "New research is now available."),
        new("level_up.dismiss", "바깥 영역을 탭하여 닫으세요.", "Tap outside to close."),

        new("loading.status", "불러오는 중", "LOADING"),
        new("loading.tip.1", "같은 유닛과 같은 성급이 만나면 더 높은 성급으로 자동 합성됩니다.", "Matching units of the same rank automatically fuse into a higher rank."),
        new("loading.tip.2", "준비 단계에서는 유닛을 드래그해 자유롭게 배치할 수 있습니다.", "During preparation, drag units to reposition them on the battlefield."),
        new("loading.tip.3", "필요하지 않은 유닛은 판매해 전투 재화를 회수할 수 있습니다.", "Sell units you no longer need to recover battle currency."),
        new("loading.tip.4", "인구수를 늘리면 전장에 더 많은 유닛을 배치할 수 있습니다.", "Increase population to deploy more units on the battlefield."),
        new("loading.tip.5", "엘리트 적을 처치하면 전투에 도움이 되는 유물을 선택할 수 있습니다.", "Defeat Elite enemies to choose a relic that empowers your run."),
        new("loading.tip.6", "리롤은 유닛의 성급을 유지하면서 다른 유닛으로 변경합니다.", "Reroll changes a unit while preserving its current rank."),
    };

    public static void Configure(Locale korean, Locale english, string tablesDirectory)
    {
        StringTableCollection collection = LocalizationEditorSettings.GetStringTableCollection(LocalizationTables.Tutorial);
        if (collection == null)
            collection = LocalizationEditorSettings.CreateStringTableCollection(
                LocalizationTables.Tutorial, tablesDirectory, new List<Locale> { korean, english });

        StringTable ko = GetOrCreate(collection, korean);
        StringTable en = GetOrCreate(collection, english);
        foreach (Entry value in Entries)
        {
            Set(ko, value.Key, value.Ko);
            Set(en, value.Key, value.En);
        }

        EditorUtility.SetDirty(collection);
        EditorUtility.SetDirty(collection.SharedData);
        EditorUtility.SetDirty(ko);
        EditorUtility.SetDirty(en);
        LocalizationEditorSettings.EditorEvents.RaiseCollectionModified(null, collection);
        AssetDatabase.SaveAssets();
    }

    public static void BuildAddressables()
    {
        AddressableAssetSettings.BuildPlayerContent();
    }

    private static StringTable GetOrCreate(StringTableCollection collection, Locale locale) =>
        collection.GetTable(locale.Identifier) as StringTable ??
        collection.AddNewTable(locale.Identifier) as StringTable;

    private static void Set(StringTable table, string key, string value)
    {
        StringTableEntry entry = table.GetEntry(key) ?? table.AddEntry(key, value);
        entry.Value = value;
        entry.IsSmart = false;
    }
}
