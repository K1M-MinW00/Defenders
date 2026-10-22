using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Localization;
using UnityEngine.Localization;
using UnityEngine.Localization.Tables;

public static class StageStringTableSetup
{
    private readonly struct Entry
    {
        public Entry(string key, string korean, string english, bool smart = false)
        { Key = key; Korean = korean; English = english; Smart = smart; }
        public string Key { get; }
        public string Korean { get; }
        public string English { get; }
        public bool Smart { get; }
    }

    private static readonly Entry[] Entries =
    {
        new("hud.free_count", "무료 ({0}회)", "Free ({0})", true),
        new("hud.monsters.incoming", "출현 예정: {0:00}", "Incoming: {0:00}", true),
        new("hud.monsters.remaining", "남은 몬스터 수: {0:00}", "Monsters Remaining: {0:00}", true),
        new("hud.stage", "스테이지 {0}-{1}", "Stage {0}-{1}", true),
        new("hud.wave", "웨이브", "WAVE"),
        new("result.stage", "스테이지 {0}", "Stage {0}", true),
        new("result.stage_unknown", "스테이지", "Stage"),
        new("result.retry", "다시 시도", "Retry"),
        new("result.return", "돌아가기", "Return"),
        new("result.processing", "처리 중...", "Processing..."),
        new("result.success", "성공", "Success"),
        new("result.failure", "실패", "Failure"),
        new("result.rewards", "보상 목록", "Rewards"),
        new("result.waves_survived_label", "살아남은 웨이브", "Waves Survived"),
        new("result.waves_survived", "{0} / {1}", "{0} / {1}", true),
        new("result.refunded_fuel", "반환된 연료", "Refunded Fuel"),
        new("result.refund_description", "작전을 실패하면 연료의 대부분을 반환합니다.", "Most of the Fuel is refunded when an operation fails."),
        new("result.save_failed", "결과 저장에 실패했습니다. 다시 시도해 주세요.", "Failed to save the result. Please try again."),
        new("result.scene_service_unavailable", "씬 전환 서비스를 사용할 수 없습니다.", "The scene transition service is unavailable."),
        new("result.lobby_load_failed", "로비로 이동하지 못했습니다. 다시 시도해 주세요.", "Failed to return to the lobby. Please try again."),
        new("prepare.unit_required", "전투를 진행할 유닛이 하나 이상 있어야 합니다.", "At least one unit is required to start the battle."),
        new("prepare.insufficient_minerals", "광물이 모자랍니다.", "Not enough minerals."),
        new("prepare.population_full", "유닛 수가 최대입니다. 인구 수를 증가시키거나 유닛을 판매하세요.", "The unit limit has been reached. Increase population or sell a unit."),
        new("prepare.last_unit_required", "유닛을 1명 이상 배치해야 전투를 시작할 수 있습니다.", "At least one unit must remain deployed to start the battle."),
        new("prepare.start", "출발!", "Start!"),
        new("prepare.summon", "유닛 생성", "Summon"),
        new("prepare.increase_population", "인원수 증가", "Unit Cap +"),
        new("prepare.reroll", "유닛 리롤", "Reroll"),
        new("prepare.reroll_guide", "리롤할 유닛을 이곳으로 드래그하세요.", "Drag a unit here to reroll."),
        new("prepare.sell", "판매", "Sell"),
        new("prepare.sell_guide", "판매할 유닛을 이곳으로 드래그하세요.", "Drag a unit here to sell."),
        new("pause.resume", "계속하기", "Continue"),
        new("pause.exit", "나가기", "Exit"),
        new("pause.push_notifications", "Push 알림", "Push Notifications"),
        new("pause.sound", "사운드", "Sound"),
        new("pause.combat_formation", "전투 부대", "Combat Formation"),
        new("announcement.sector", "섹터", "Sector"),
        new("announcement.boss_wave", "보스 웨이브", "Boss Wave"),
        new("relic_choice.title", "유물을 선택하세요.", "Choose a Relic"),
        new("relic_choice.select", "선택", "Select"),
        new("initialization.refunded", "스테이지를 불러오지 못해 사용한 연료를 반환했습니다.", "Failed to load the stage. The consumed Fuel was refunded."),
        new("initialization.refund_failed", "스테이지를 불러오지 못했으며 연료 반환에도 실패했습니다.", "Failed to load the stage, and the Fuel refund also failed."),
        new("initialization.returned", "스테이지를 불러오지 못해 로비로 돌아왔습니다.", "Failed to load the stage. Returned to the lobby."),
        new("unit_info.active_locked", "잠김 - 3성부터 해금", "Locked - Unlocks at 3 Stars"),
        new("unit_info.passive_locked", "잠김 - 1진급부터 해금", "Locked - Unlocks at Promotion 1"),
        new("unit_info.no_skill_info", "스킬 정보가 없습니다.", "No skill information available."),
        new("unit_info.promotion_effect", "[진급 {0} 효과] ", "[Promotion {0} Effect] ", true),
        new("unit_info.no_skill", "스킬 없음", "No Skill"),
        new("unit_info.active", "액티브", "Active"),
        new("unit_info.passive", "패시브", "Passive"),
        new("monster.description.unavailable", "설명이 준비 중입니다.", "Description coming soon."),
        new("monster.info.title", "몬스터 정보", "Monster Information"),
        new("monster.info.empty", "이 웨이브에는 표시할 몬스터가 없습니다.", "There are no monsters to display for this wave."),
        new("monster.info.boss_skills", "보스 스킬", "Boss Skills"),
        new("monster.info.area_attack", "• 예고 광역 공격", "• Telegraph Area Attack"),
        new("monster.info.shield", "• 보호막: HP {0}%에서 최대 HP의 {1}%", "• Shield: {1}% max HP at {0}% HP", true),
        new("monster.info.enrage", "• 광폭화: 공격력 x{0}, 공격 속도 x{1}", "• Enrage: Damage x{0}, Attack Speed x{1}", true),
        new("monster.info.summon", "• 증원 소환: {0}마리 / {1}초 주기 / 최대 {2}마리", "• Summon Reinforcements: {0} / every {1}s / max {2}", true),
        new("monster.info.status_effects", "부여 상태 효과", "Applied Status Effects"),
        new("monster.info.slow", "• 감속: {0}초 / 이동 속도 {1}% 감소", "• Slow: {0}s / Move Speed -{1}%", true),
        new("monster.info.stun", "• 기절: {0}초", "• Stun: {0}s", true),
        new("monster.info.poison", "• 중독: {0}초 / {1}초마다 공격력의 {2}% 피해", "• Poison: {0}s / {2}% ATK every {1}s", true),
        new("monster.info.burn", "• 화상: {0}초 / {1}초마다 공격력의 {2}% 피해", "• Burn: {0}s / {2}% ATK every {1}s", true),
        new("monster.info.defense_down", "• 방어력 감소: {0}초 / {1}%p 감소", "• Defense Down: {0}s / -{1}%p", true),
        new("monster.info.status_resistance", "상태 저항", "Status Resistance"),
        new("monster.info.status_resistance.none", "• 없음", "• None"),
        new("monster.info.resistance.slow", "• 감속 {0}%", "• Slow {0}%", true),
        new("monster.info.resistance.stun", "• 기절 {0}%", "• Stun {0}%", true),
        new("monster.info.resistance.poison", "• 중독 {0}%", "• Poison {0}%", true),
        new("monster.info.resistance.burn", "• 화상 {0}%", "• Burn {0}%", true),
        new("monster.info.resistance.defense_down", "• 방어력 감소 {0}%", "• Defense Down {0}%", true),
        new("combat.status.immune", "면역", "IMMUNE"),
        new("combat.statistics.rank", "순위", "Rank"),
        new("combat.statistics.unit", "유닛", "Unit"),
        new("combat.statistics.damage", "피해량", "Damage"),
        new("combat.statistics.healing", "회복량", "Healing"),
        new("combat.statistics.damage_taken", "받은 피해", "Damage Taken"),
        new("monster.name.Boss_Elite_Orc", "정예 오크 보스", "Elite Orc Boss"),
        new("monster.name.Boss_Armored_Orc", "갑옷 오크 보스", "Armored Orc Boss"),
        new("monster.name.Boss_Armored_Skeleton", "갑옷 스켈레톤 보스", "Armored Skeleton Boss"),
        new("monster.name.Boss_WereWolf", "늑대인간 보스", "Werewolf Boss"),
        new("monster.description.Boss_Elite_Orc", "빠르고 강력한 정예 오크 보스", "A fast and powerful elite orc boss."),
        new("monster.description.Boss_Armored_Orc", "단단한 갑옷과 강한 공격력을 지닌 오크 보스", "An orc boss with heavy armor and powerful attacks."),
        new("monster.description.Boss_Armored_Skeleton", "갑옷을 두르고 빠르게 공격하는 스켈레톤 보스", "An armored skeleton boss that attacks rapidly."),
        new("monster.description.Boss_WereWolf", "빠르고 거센 연속 공격을 가하는 늑대인간 보스", "A werewolf boss that unleashes fast, ferocious combinations."),
        new("monster.name.Orc", "오크", "Orc"),
        new("monster.name.Elite_Orc", "정예 오크", "Elite Orc"),
        new("monster.name.Armored_Orc", "갑옷 오크", "Armored Orc"),
        new("monster.name.Orc_Rider", "오크 기병", "Orc Rider"),
        new("monster.name.Skeleton", "스켈레톤", "Skeleton"),
        new("monster.name.Armored_Skeleton", "갑옷 스켈레톤", "Armored Skeleton"),
        new("monster.name.Skeleton_Archer", "스켈레톤 궁수", "Skeleton Archer"),
        new("monster.name.Slime", "슬라임", "Slime"),
        new("monster.name.WereWolf", "늑대인간", "Werewolf"),
        new("monster.description.Orc", "빠르게 돌진하는 근접 오크 전사", "A melee orc warrior that charges swiftly."),
        new("monster.description.Elite_Orc", "빠르고 강력한 정예 오크 전사", "A fast and powerful elite orc warrior."),
        new("monster.description.Armored_Orc", "단단한 갑옷과 강한 공격력을 지닌 오크", "An orc with heavy armor and powerful attacks."),
        new("monster.description.Orc_Rider", "빠른 이동과 강한 일격이 특징인 기병", "A mounted fighter known for speed and powerful strikes."),
        new("monster.description.Skeleton", "느리지만 체력이 매우 높은 해골 병사", "A slow skeleton soldier with exceptionally high health."),
        new("monster.description.Armored_Skeleton", "갑옷을 두르고 빠르게 공격하는 해골", "An armored skeleton that attacks rapidly."),
        new("monster.description.Skeleton_Archer", "멀리서 화살을 쏘는 해골 궁수", "A skeleton archer that fires arrows from a distance."),
        new("monster.description.Slime", "빠르게 접근해 연속 공격하는 슬라임", "A slime that closes in quickly and attacks repeatedly."),
        new("monster.description.WereWolf", "빠르고 거센 연속 공격을 가하는 늑대인간", "A werewolf that unleashes fast, ferocious combinations."),
    };

    public static void Configure(Locale korean, Locale english, string tablesDirectory)
    {
        StringTableCollection collection = LocalizationEditorSettings.GetStringTableCollection(LocalizationTables.Stage);
        if (collection == null)
            collection = LocalizationEditorSettings.CreateStringTableCollection(
                LocalizationTables.Stage, tablesDirectory, new List<Locale> { korean, english });

        StringTable ko = GetOrCreate(collection, korean);
        StringTable en = GetOrCreate(collection, english);
        foreach (Entry entry in Entries)
        {
            Set(ko, entry.Key, entry.Korean, entry.Smart);
            Set(en, entry.Key, entry.English, entry.Smart);
        }
        EditorUtility.SetDirty(ko);
        EditorUtility.SetDirty(en);
        EditorUtility.SetDirty(collection.SharedData);
    }

    private static StringTable GetOrCreate(StringTableCollection collection, Locale locale) =>
        collection.GetTable(locale.Identifier) as StringTable ?? collection.AddNewTable(locale.Identifier) as StringTable;

    private static void Set(StringTable table, string key, string value, bool smart)
    {
        StringTableEntry entry = table.GetEntry(key) ?? table.AddEntry(key, value);
        entry.Value = value;
        entry.IsSmart = smart;
    }
}
