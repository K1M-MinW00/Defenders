using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Localization;
using UnityEngine.Localization;
using UnityEngine.Localization.Tables;

public static class LobbyStringTableSetup
{
    private readonly struct Entry
    {
        public Entry(string key, string korean, string english, bool isSmart = false)
        {
            Key = key;
            Korean = korean;
            English = english;
            IsSmart = isSmart;
        }

        public string Key { get; }
        public string Korean { get; }
        public string English { get; }
        public bool IsSmart { get; }
    }

    private static readonly Entry[] Entries =
    {
        new("nav.recruit", "모집", "Recruit"),
        new("nav.units", "유닛", "Units"),
        new("nav.battle", "전투", "Battle"),
        new("nav.lab", "연구소", "Lab"),
        new("nav.shop", "상점", "Shop"),
        new("shortcut.supply", "보급품", "Supplies"),
        new("shortcut.conquest_rewards", "정복 보상", "Conquest\nRewards"),
        new("shortcut.mailbox", "우편함", "Mailbox"),
        new("shortcut.inventory", "가방", "Bag"),
        new("shortcut.quests", "퀘스트", "Quests"),
        new("recruit.title", "유닛 모집", "Unit Recruitment"),
        new("recruit.tab.normal", "일반\n모집", "Normal\nRecruitment"),
        new("recruit.tab.special", "특별\n모집", "Special\nRecruitment"),
        new("recruit.rate_preview", "확률 미리보기", "View Rates"),
        new("recruit.rate.title", "확률 정보", "Rate Information"),
        new("recruit.result.title", "모집 결과", "Recruitment Results"),
        new("recruit.presentation.new_unit", "새로운 유닛", "New Unit"),
        new("recruit.presentation.skip", "건너뛰기", "Skip"),
        new("recruit.rarity.normal", "노말", "Normal"),
        new("recruit.rarity.rare", "레어", "Rare"),
        new("recruit.rarity.epic", "에픽", "Epic"),
        new("recruit.rarity.legend", "전설", "Legendary"),
        new("recruit.button.use", "사용하기", "Use"),
        new("recruit.button.back", "돌아가기", "Back"),
        new("unit.title", "유닛", "Units"),
        new("unit.roster.battle", "전투 부대", "Battle Roster"),
        new("unit.roster.reserve", "대기 명단", "Reserve Roster"),
        new("unit.button.codex", "도감", "Codex"),
        new("unit.button.formation", "편성", "Formation"),
        new("unit.stat.attack", "공격력", "Attack"),
        new("unit.stat.health", "체력", "HP"),
        new("unit.training.select_materials", "재료 선택", "Select Materials"),
        new("unit.training.level_one", "+1 레벨", "+1 Level"),
        new("unit.training.level_max", "최대 레벨", "Max Level"),
        new("unit.training.title", "훈련", "Training"),
        new("unit.nav.promotion", "진급", "Promotion"),
        new("unit.nav.limit_break", "한계돌파", "Limit Break"),
        new("unit.nav.equipment", "장비", "Equipment"),
        new("unit.promotion.effects", "진급 효과", "Promotion Effects"),
        new("unit.promotion.cost", "소모 재화", "Cost"),
        new("unit.skill.active", "액티브", "Active"),
        new("unit.skill.passive", "패시브", "Passive"),
        new("unit.limit_break.effects", "한계돌파 효과", "Limit Break Effects"),
        new("unit.promotion.effect.passive_unlock", "패시브 스킬 해금", "Unlock Passive Skill"),
        new("unit.promotion.effect.active_health", "액티브 스킬 효과 추가, 체력 10% 증가", "Add Active Skill Effect, HP +10%"),
        new("unit.promotion.effect.passive_attack", "패시브 스킬 효과 추가, 공격력 10% 증가", "Add Passive Skill Effect, Attack +10%"),
        new("equipment.slot.main_weapon", "주무기", "Main Weapon"),
        new("equipment.slot.sub_weapon", "보조무기", "Secondary Weapon"),
        new("equipment.slot.helmet", "투구", "Helmet"),
        new("equipment.slot.armor", "갑옷", "Armor"),
        new("equipment.stats.title", "장비 스탯", "Equipment Stats"),
        new("equipment.modification.title", "개조 효과", "Modification Effects"),
        new("equipment.change.title", "장비 변경", "Change Equipment"),
        new("equipment.change.guide", "장비를 착용하거나 교체할 수 있습니다.", "Equip or replace equipment."),
        new("equipment.change.empty", "현재 착용한 장비가 없습니다.", "No equipment is currently equipped."),
        new("equipment.button.equip", "장착", "Equip"),
        new("equipment.enhancement.title", "장비 강화", "Equipment Enhancement"),
        new("equipment.enhancement.level_up", "레벨업", "Level Up"),
        new("equipment.enhancement.reset", "되돌리기", "Reset"),
        new("equipment.enhancement.auto_select", "자동 선택", "Auto Select"),
        new("equipment.enhancement.select_materials", "강화석을 선택하세요", "Select enhancement stones."),
        new("equipment.button.unequip", "해제", "Unequip"),
        new("equipment.button.enhance", "강화", "Enhance"),
        new("equipment.button.replace", "장비 교체", "Replace"),
        new("inventory.title", "가방", "Inventory"),
        new("inventory.tab.items", "아이템", "Items"),
        new("inventory.tab.equipment", "장비", "Equipment"),
        new("inventory.tab.materials", "재료", "Materials"),
        new("inventory.filter.all", "All", "All"),
        new("inventory.filter.reset", "Reset", "Reset"),
        new("inventory.dismantle.title", "분해", "Dismantle"),
        new("inventory.dismantle.guide", "분해할 장비를 선택하세요.", "Select equipment to dismantle."),
        new("inventory.dismantle.rewards", "분해 보상", "Dismantle Rewards"),
        new("inventory.dismantle.rewards_acquired", "분해 보상 획득", "Dismantle Rewards Acquired"),
        new("recruit.confirm_gems", "젬 {0:N0}개를 사용하여 모집하시겠습니까?", "Spend {0:N0} Gems to recruit?", true),
        new("recruit.pity", "앞으로 {0}회 모집 안에 전설 유닛 확정 획득", "Legendary unit guaranteed within {0} recruits", true),
        new("recruit.rate.legend_units", "전설 유닛", "Legendary Units"),
        new("recruit.rate.epic_units", "에픽 유닛", "Epic Units"),
        new("recruit.rate.rare_units", "희귀 유닛", "Rare Units"),
        new("recruit.rate.pickup", "{0} (픽업)", "{0} (Rate Up)", true),
        new("shop.current_gems", "보유 젬 {0:N0}", "Current Gems: {0:N0}", true),
        new("shop.price_gems", "가격: 젬 {0:N0}", "Price: {0:N0} Gems", true),
        new("shop.remaining_gems", "구매 후 젬 {0:N0}", "Gems after purchase: {0:N0}", true),
        new("shop.remaining_gems_insufficient", "구매 후 젬 부족", "Not enough Gems after purchase"),
        new("shop.confirm_purchase", "이 상품을 구매하시겠습니까?", "Purchase this product?"),
        new("shop.insufficient_gems", "젬이 부족합니다.", "Not enough Gems."),
        new("shop.processing", "구매 처리 중", "Processing purchase..."),
        new("shop.result_success_title", "구매 완료!", "Purchase Complete!"),
        new("shop.result_failure_title", "구매 실패", "Purchase Failed"),
        new("shop.remaining_purchases", "구매 가능 {0}/{1}", "Available: {0}/{1}", true),
        new("shop.status.coming_soon", "준비 중", "Coming Soon"),
        new("shop.status.sold_out", "품절", "Sold Out"),
        new("shop.status.sale_ended", "판매 종료", "Sale Ended"),
        new("shop.cost.gems", "젬 {0:N0}", "{0:N0} Gems", true),
        new("shop.cost.iap", "₩{0:N0}", "₩{0:N0}", true),
        new("shop.cost.ad", "광고 보기", "Watch Ad"),
        new("shop.cost.gold", "골드 {0:N0}", "{0:N0} Gold", true),
        new("shop.stock.refreshing", "재고 갱신 중", "Refreshing stock"),
        new("shop.stock.remaining_days", "{0}일 {1}시간", "{0}d {1}h", true),
        new("shop.first_purchase_bonus", "첫 구매 2배", "First Purchase x2"),
        new("shop.tab.limited", "한정", "Limited"),
        new("shop.tab.package", "패키지", "Packages"),
        new("shop.tab.recharge", "충전", "Recharge"),
        new("shop.tab.exchange", "교환소", "Exchange"),
        new("shop.section.daily_package", "일간 패키지", "Daily Packages"),
        new("shop.section.weekly_package", "주간 패키지", "Weekly Packages"),
        new("shop.section.monthly_package", "월간 패키지", "Monthly Packages"),
        new("shop.purchase.title", "상품 구매", "Purchase Product"),
        new("shop.purchase.buy", "구매", "Purchase"),
        new("shop.purchase.cancel", "취소", "Cancel"),
        new("shop.purchase.recharge", "Gem 충전", "Recharge Gems"),
        new("shop.purchase.close", "확인", "OK"),
        new("battle.stage", "섹터\n{0}-{1}", "Sector\n{0}-{1}", true),
        new("battle.fuel", "{0} / {1}", "{0} / {1}", true),
        new("battle.button.challenge", "도전", "Challenge"),
        new("battle.button.go", "Go", "Go"),
        new("battle.button.guide", "가이드", "Guide"),
        new("battle.event.attendance", "출석\n이벤트", "Attendance\nEvent"),
        new("battle.event.pass", "배틀\n패스", "Battle\nPass"),
        new("battle.event.growth", "성장\n이벤트", "Growth\nEvent"),
        new("battle.event.privilege", "특권", "Privileges"),
        new("battle.start.empty_formation", "전투에 참여할 유닛을 편성해 주세요.", "Assign units to the battle roster first."),
        new("battle.start.invalid_formation", "편성 정보가 올바르지 않습니다.", "The battle roster is invalid."),
        new("battle.start.invalid_progress", "스테이지 진행 정보를 확인할 수 없습니다.", "Unable to verify stage progress."),
        new("battle.start.unavailable", "전투를 시작할 수 없습니다.", "Unable to start the battle."),
        new("battle.scene.invalid", "전투 씬이 빌드 설정에 없습니다.", "The battle scene is missing from the build settings."),
        new("battle.scene.already_loading", "다른 화면으로 이동 중입니다.", "Another screen is currently loading."),
        new("battle.scene.load_failed", "전투 화면을 불러오지 못했습니다.", "Unable to load the battle screen."),
        new("battle.scene.refund_failed_suffix", " 연료 반환에도 실패했습니다.", " Fuel refund also failed."),
        new("battle.scene.exception_refunded", "전투 화면을 불러오지 못해 사용한 연료를 반환했습니다.", "Unable to load the battle screen. The consumed Fuel was refunded."),
        new("battle.scene.exception_refund_failed", "전투 화면을 불러오지 못했으며 연료 반환에도 실패했습니다.", "Unable to load the battle screen, and the Fuel refund also failed."),
        new("battle.fuel.insufficient", "스테이지 입장에는 연료 {0}이 필요합니다.", "You need {0} Fuel to enter this stage.", true),
        new("battle.fuel.save_failed", "연료 사용 정보를 저장하지 못했습니다.", "Unable to save the Fuel consumption."),
        new("battle.fuel.unavailable", "연료를 사용할 수 없습니다.", "Unable to use Fuel."),
        new("fuel.status", "{0}/{1}", "{0}/{1}", true),
        new("fuel.next_recovery", "다음 충전까지 {0}", "Next Fuel in {0}", true),
        new("fuel.full_recovery", "최대 충전까지 {0}", "Full recovery in {0}", true),
        new("fuel.daily_ads", "일일 광고 시청 ({0}/{1})", "Daily ads ({0}/{1})", true),
        new("fuel.gem_cost", "젬 {0:N0}", "{0:N0} Gems", true),
        new("fuel.purchase.title", "연료 구매", "Purchase Fuel"),
        new("fuel.purchase.ad", "<sprite=4>광고", "<sprite=4>Ad"),
        new("idle.loading_user_data", "유저 데이터를 불러오는 중입니다.", "Loading user data."),
        new("idle.current_rewards", "현재 보상", "Current Rewards"),
        new("idle.max_supply_support", "최대 보급품 지원 : 12시간", "Maximum supply support: 12 hours"),
        new("idle.claim.button", "획득", "Claim"),
        new("idle.sector_hourly_rewards", "섹터 {0} 기준 시간당 보상", "Hourly rewards for Sector {0}", true),
        new("idle.accumulated_time", "{0}시간 {1}분 / 최대 {2}시간", "{0}h {1}m / Max {2}h", true),
        new("idle.hourly_rate_zero", "0 /시간", "0 /hr"),
        new("idle.hourly_rate", "{0:N0}<color=#56FF69> (+{1:N0})</color> /시간", "{0:N0}<color=#56FF69> (+{1:N0})</color> /hr", true),
        new("idle.claim.not_ready", "수령할 보상이 없습니다.", "There are no rewards to claim."),
        new("idle.claim.failed", "보상 수령에 실패했습니다.", "Failed to claim rewards."),
        new("idle.claim.succeeded", "방치형 보상을 획득했습니다.", "Idle rewards claimed."),
        new("nickname.confirm.free", "무료 변경", "Change for Free"),
        new("nickname.confirm.paid", "변경 (보석 {0:N0})", "Change ({0:N0} Gems)", true),
        new("nickname.title", "닉네임", "Nickname"),
        new("nickname.rule", "2~12자 (공백, 특수문자, 비속어 불가)", "2–12 characters (no spaces, symbols, or profanity)"),
        new("nickname.placeholder", "이름을 입력하세요", "Enter a nickname"),
        new("nickname.cancel", "취소", "Cancel"),
        new("nickname.failure.invalid", "닉네임은 2~12자로 입력해주세요.", "Enter a nickname between 2 and 12 characters."),
        new("nickname.failure.insufficient_gems", "닉네임 변경에 필요한 보석이 부족합니다.", "Not enough Gems to change your nickname."),
        new("nickname.failure.already_taken", "이미 사용 중인 닉네임입니다.", "This nickname is already in use."),
        new("nickname.failure.busy", "닉네임 변경을 처리 중입니다.", "A nickname change is already in progress."),
        new("nickname.failure.save_failed", "닉네임 변경에 실패했습니다. 잠시 후 다시 시도해주세요.", "Failed to change the nickname. Please try again shortly."),
        new("profile.icon.save_failed", "프로필 아이콘 저장에 실패했습니다.", "Failed to save the profile icon."),
        new("profile.title", "프로필", "Profile"),
        new("profile.cancel", "취소", "Cancel"),
        new("profile.confirm", "확인", "Confirm"),
        new("lab.title", "연구소", "Lab"),
        new("lab.develop", "기술 개발", "Develop Technology"),
        new("lab.capacity", "보유 {0}/{1}  다음 슬롯 레벨 {2}", "Owned {0}/{1}  Next slot at Lv. {2}", true),
        new("lab.failure.insufficient_currency", "재화가 부족합니다.", "Not enough research materials."),
        new("lab.failure.level_locked", "계정 레벨을 더 올리면 새로운 기술을 개발할 수 있습니다.", "Increase your account level to develop new technologies."),
        new("lab.failure.all_cards_acquired", "모든 기술 카드를 획득했습니다.", "You have acquired all technology cards."),
        new("lab.failure.save_failed", "저장에 실패했습니다. 잠시 후 다시 시도해 주세요.", "Failed to save. Please try again shortly."),
        new("lab.failure.unavailable", "기술 개발을 진행할 수 없습니다.", "Unable to develop a technology."),
        new("wave_reward.loading_user_data", "유저 데이터를 불러오는 중입니다.", "Loading user data."),
        new("wave_reward.title", "웨이브 달성 보상", "Wave Milestone Rewards"),
        new("wave_reward.claim_all", "전부 수령", "Claim All"),
        new("wave_reward.stage_title", "스테이지\n{0}", "Stage\n{0}", true),
        new("wave_reward.wave", "웨이브\n<size=150%>{0}</size>", "WAVE\n<size=150%>{0}</size>", true),
        new("wave_reward.state.claimed", "수령 완료", "Claimed"),
        new("wave_reward.state.claim", "수령", "Claim"),
        new("wave_reward.state.locked", "잠김", "Locked"),
        new("wave_reward.claim.succeeded", "웨이브 달성 보상을 받았습니다.", "Wave milestone rewards claimed."),
        new("wave_reward.claim.locked", "아직 달성하지 않은 보상입니다.", "This reward milestone has not been reached yet."),
        new("wave_reward.claim.already_claimed", "이미 받은 보상입니다.", "This reward has already been claimed."),
        new("wave_reward.claim.save_failed", "보상 저장에 실패했습니다.", "Failed to save the rewards."),
        new("wave_reward.claim.unavailable", "보상을 받을 수 없습니다.", "Unable to claim the rewards."),
        new("quest.loading", "퀘스트 정보를 불러오는 중입니다.", "Loading quest information."),
        new("quest.refresh_failed", "퀘스트 정보를 갱신하지 못했습니다.", "Failed to refresh quest information."),
        new("quest.score.claim_failed", "퀘스트 점수를 획득하지 못했습니다.", "Failed to claim quest points."),
        new("quest.score.claimed", "퀘스트 점수 {0}점을 획득했습니다.", "Claimed {0} quest points.", true),
        new("quest.score.already_claimed", "이미 점수를 획득한 퀘스트입니다.", "The points for this quest have already been claimed."),
        new("quest.reward.unavailable", "퀘스트 보상을 받을 수 없습니다.", "Unable to claim the quest reward."),
        new("quest.reward.insufficient_score", "퀘스트 점수가 부족합니다.", "Not enough quest points."),
        new("quest.reward.already_claimed", "이미 받은 보상입니다.", "This reward has already been claimed."),
        new("quest.reward.save_failed", "보상 저장에 실패했습니다.", "Failed to save the reward."),
        new("quest.navigation_missing", "이동할 화면을 찾지 못했습니다.", "Unable to find the destination screen."),
        new("quest.timer", "{0}시간 {1:00}분", "{0}h {1:00}m", true),
        new("quest.title.daily", "일일 퀘스트", "Daily Quests"),
        new("quest.rewards.daily", "일일 보상", "Daily Rewards"),
        new("quest.rewards.weekly", "주간 보상", "Weekly Rewards"),
        new("quest.action.claimed", "획득함", "Claimed"),
        new("quest.action.claim", "획득", "Claim"),
        new("quest.action.go", "이동", "Go"),
        new("quest.reward.preview_title", "보상 정보", "Reward Details"),
        new("quest.reward.acquired_title", "획득한 보상", "Rewards Acquired"),
        new("quest.definition.daily_login", "로그인 1회", "Log in once"),
        new("quest.definition.daily_four_star_fusion", "4성 유닛 1회 합성", "Fuse a 4-star unit once"),
        new("quest.definition.daily_recruit", "유닛 1회 모집", "Recruit a unit once"),
        new("quest.definition.daily_unit_level_up", "유닛 레벨업 1회", "Level up a unit once"),
        new("quest.definition.daily_lab_development", "연구소 개발 1회", "Develop a Lab technology once"),
        new("quest.definition.daily_wave_clear", "웨이브 10회 클리어", "Clear 10 waves"),
        new("quest.definition.daily_unit_fusion", "유닛 합성 10회", "Fuse units 10 times"),
        new("quest.definition.daily_fuel_150", "연료 150개 소모", "Spend 150 Fuel"),
        new("quest.definition.daily_fuel_50", "연료 50개 소모", "Spend 50 Fuel"),
        new("mail.expired", "만료됨", "Expired"),
        new("mail.expires_days", "{0}일 후 만료", "Expires in {0} days", true),
        new("mail.expires_hours", "{0}시간 후 만료", "Expires in {0} hours", true),
        new("mail.expires_minutes", "{0}분 후 만료", "Expires in {0} minutes", true),
        new("mail.expires_soon", "곧 만료", "Expires soon"),
        new("mail.title", "우편함", "Mailbox"),
        new("mail.receive_all", "전부 수령", "Claim All"),
        new("mail.delete_all", "일괄 삭제", "Delete All"),
        new("recruit.panel.title", "모집", "Recruit Units"),
        new("recruit.button.recruit", "모집", "Recruit"),
    };

    public static void Configure(Locale korean, Locale english, string tablesDirectory)
    {
        StringTableCollection collection =
            LocalizationEditorSettings.GetStringTableCollection(LocalizationTables.Lobby);
        if (collection == null)
        {
            collection = LocalizationEditorSettings.CreateStringTableCollection(
                LocalizationTables.Lobby,
                tablesDirectory,
                new List<Locale> { korean, english });
        }

        StringTable koreanTable = GetOrCreateTable(collection, korean);
        StringTable englishTable = GetOrCreateTable(collection, english);
        foreach (Entry entry in Entries)
        {
            SetEntry(koreanTable, entry.Key, entry.Korean, entry.IsSmart);
            SetEntry(englishTable, entry.Key, entry.English, entry.IsSmart);
        }

        EditorUtility.SetDirty(koreanTable);
        EditorUtility.SetDirty(englishTable);
        EditorUtility.SetDirty(collection.SharedData);
    }

    private static StringTable GetOrCreateTable(StringTableCollection collection, Locale locale)
    {
        return collection.GetTable(locale.Identifier) as StringTable ??
               collection.AddNewTable(locale.Identifier) as StringTable;
    }

    private static void SetEntry(StringTable table, string key, string value, bool isSmart)
    {
        StringTableEntry entry = table.GetEntry(key) ?? table.AddEntry(key, value);
        entry.Value = value;
        entry.IsSmart = isSmart;
    }
}
