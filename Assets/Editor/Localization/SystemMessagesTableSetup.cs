using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Localization;
using UnityEngine.Localization;
using UnityEngine.Localization.Tables;

public static class SystemMessagesTableSetup
{
    private static readonly (string Key, string Korean, string English)[] Entries =
    {
        ("common.save_failed", "저장에 실패했습니다. 잠시 후 다시 시도해 주세요.", "Failed to save. Please try again shortly."),
        ("lobby.ad_unavailable", "광고를 불러오지 못했습니다. 잠시 후 다시 시도해 주세요.", "Unable to load the ad. Please try again shortly."),
        ("mailbox.load_failed", "우편함을 불러오지 못했습니다. 잠시 후 다시 시도해 주세요.", "Unable to load the mailbox. Please try again shortly."),
        ("mailbox.delete_failed", "우편 삭제에 실패했습니다. 잠시 후 다시 시도해 주세요.", "Failed to delete the mail. Please try again shortly."),
        ("mailbox.user_not_found", "사용자 정보를 찾을 수 없습니다.", "User information could not be found."),
        ("mailbox.nothing_to_claim", "받을 수 있는 우편이 없습니다.", "There is no mail to claim."),
        ("mailbox.invalid_reward", "우편 보상 정보가 올바르지 않습니다.", "The mail reward information is invalid."),
        ("mailbox.claim_save_failed", "우편 보상 저장에 실패했습니다. 잠시 후 다시 시도해 주세요.", "Failed to save the mail reward. Please try again shortly."),
        ("mailbox.claim_unavailable", "우편 보상을 받을 수 없습니다.", "Unable to claim the mail reward."),
        ("recruit.invalid_banner", "현재 소환 배너를 사용할 수 없습니다.", "This recruit banner is currently unavailable."),
        ("recruit.insufficient_currency", "소환 재화가 부족합니다.", "Not enough recruit currency."),
        ("recruit.empty_pool", "소환 가능한 유닛이 없습니다.", "No units are available to recruit."),
        ("recruit.save_failed", "소환 결과 저장에 실패했습니다. 잠시 후 다시 시도해 주세요.", "Failed to save the recruit results. Please try again shortly."),
        ("recruit.unavailable", "소환을 진행할 수 없습니다.", "Unable to recruit units."),
        ("fuel.insufficient_gems", "보석이 부족합니다.", "Not enough Gems."),
        ("fuel.capacity_reached", "연료를 더 보유할 수 없습니다.", "You cannot hold any more Fuel."),
        ("fuel.purchase_save_failed", "연료 구매 저장에 실패했습니다. 잠시 후 다시 시도해 주세요.", "Failed to save the Fuel purchase. Please try again shortly."),
        ("fuel.purchase_unavailable", "연료를 구매할 수 없습니다.", "Unable to purchase Fuel."),
        ("fuel.ad_daily_limit", "오늘 받을 수 있는 광고 보상을 모두 받았습니다.", "You have claimed all ad rewards available today."),
        ("fuel.ad_save_failed", "광고 보상 저장에 실패했습니다. 잠시 후 다시 시도해 주세요.", "Failed to save the ad reward. Please try again shortly."),
        ("fuel.ad_reward_unavailable", "광고 보상을 받을 수 없습니다.", "Unable to claim the ad reward."),
        ("shop.processing_failed", "구매 처리에 실패했습니다.", "Failed to process the purchase."),
        ("shop.completed", "구매가 완료되었습니다.", "Purchase complete."),
        ("shop.insufficient_gems", "Gem이 부족합니다.", "Not enough Gems."),
        ("shop.sold_out", "구매 가능한 수량을 모두 구매했습니다.", "You have purchased the maximum available quantity."),
        ("shop.not_on_sale", "현재 판매 중인 상품이 아닙니다.", "This product is not currently on sale."),
        ("shop.unsupported_purchase_type", "아직 지원하지 않는 결제 방식입니다.", "This purchase method is not supported yet."),
        ("shop.invalid_reward", "상품 보상 정보가 올바르지 않습니다.", "The product reward information is invalid."),
        ("shop.capacity_exceeded", "보유 한도를 초과해 구매할 수 없습니다.", "The purchase would exceed your inventory limit."),
        ("shop.invalid_request", "구매 요청이 올바르지 않습니다.", "The purchase request is invalid."),
        ("unit.not_found", "유닛 정보를 찾을 수 없습니다.", "Unit information could not be found."),
        ("unit.train.max_level", "이미 최고 레벨입니다.", "This unit is already at the maximum level."),
        ("unit.train.insufficient_gold", "골드가 부족합니다.", "Not enough Gold."),
        ("unit.train.insufficient_materials", "훈련 재료가 부족합니다.", "Not enough training materials."),
        ("unit.train.unavailable", "훈련을 진행할 수 없습니다.", "Unable to train this unit."),
        ("unit.promote.max_level", "이미 최고 진급 단계입니다.", "This unit is already at the maximum Promotion tier."),
        ("unit.promote.invalid_cost", "진급 비용 정보가 올바르지 않습니다.", "The Promotion cost information is invalid."),
        ("unit.promote.insufficient_materials", "진급 재료가 부족합니다.", "Not enough Promotion materials."),
        ("unit.promote.unavailable", "진급을 진행할 수 없습니다.", "Unable to promote this unit."),
        ("unit.limit_break.max_level", "이미 최고 한계돌파 단계입니다.", "This unit is already at the maximum Limit Break tier."),
        ("unit.limit_break.insufficient_materials", "한계돌파 재료가 부족합니다.", "Not enough Limit Break materials."),
        ("unit.limit_break.unavailable", "한계돌파를 진행할 수 없습니다.", "Unable to Limit Break this unit."),
        ("formation.invalid_roster", "현재 전투 명단을 불러올 수 없습니다.", "Unable to load the current battle roster."),
        ("formation.unit_not_owned", "보유하지 않은 유닛입니다.", "You do not own this unit."),
        ("formation.unit_not_selected", "선택 명단에 없는 유닛입니다.", "This unit is not in the selected roster."),
        ("formation.unit_already_selected", "이미 선택된 유닛입니다.", "This unit is already selected."),
        ("formation.save_failed", "명단 저장에 실패했습니다. 잠시 후 다시 시도해 주세요.", "Failed to save the roster. Please try again shortly."),
        ("formation.unavailable", "전투 명단을 변경할 수 없습니다.", "Unable to change the battle roster."),
    };

    public static void Configure(Locale korean, Locale english, string tablesDirectory)
    {
        StringTableCollection collection =
            LocalizationEditorSettings.GetStringTableCollection(LocalizationTables.SystemMessages);
        if (collection == null)
        {
            collection = LocalizationEditorSettings.CreateStringTableCollection(
                LocalizationTables.SystemMessages,
                tablesDirectory,
                new List<Locale> { korean, english });
        }

        StringTable koreanTable = GetOrCreateTable(collection, korean);
        StringTable englishTable = GetOrCreateTable(collection, english);
        foreach ((string key, string koreanValue, string englishValue) in Entries)
        {
            SetEntry(koreanTable, key, koreanValue);
            SetEntry(englishTable, key, englishValue);
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

    private static void SetEntry(StringTable table, string key, string value)
    {
        StringTableEntry entry = table.GetEntry(key);
        if (entry == null)
            table.AddEntry(key, value);
        else
            entry.Value = value;
    }
}
