public static class LobbyOperationFeedbackMessages
{
    public static string AdUnavailable => Localize("lobby.ad_unavailable");
    public static string MailboxLoadFailed => Localize("mailbox.load_failed");
    public static string MailboxDeleteFailed => Localize("mailbox.delete_failed");

    public static string Get(RecruitUnitsFailure failure)
    {
        return failure switch
        {
            RecruitUnitsFailure.InvalidBanner => Localize("recruit.invalid_banner"),
            RecruitUnitsFailure.InsufficientCurrency => Localize("recruit.insufficient_currency"),
            RecruitUnitsFailure.EmptyPool => Localize("recruit.empty_pool"),
            RecruitUnitsFailure.SaveFailed => Localize("recruit.save_failed"),
            _ => Localize("recruit.unavailable"),
        };
    }

    public static string Get(MailboxClaimFailure failure)
    {
        return failure switch
        {
            MailboxClaimFailure.UserNotFound => Localize("mailbox.user_not_found"),
            MailboxClaimFailure.NoClaimableMail => Localize("mailbox.nothing_to_claim"),
            MailboxClaimFailure.InvalidReward => Localize("mailbox.invalid_reward"),
            MailboxClaimFailure.SaveFailed => Localize("mailbox.claim_save_failed"),
            _ => Localize("mailbox.claim_unavailable"),
        };
    }

    public static string Get(PurchaseFuelFailure failure)
    {
        return failure switch
        {
            PurchaseFuelFailure.InsufficientGem => Localize("fuel.insufficient_gems"),
            PurchaseFuelFailure.Overflow => Localize("fuel.capacity_reached"),
            PurchaseFuelFailure.SaveFailed => Localize("fuel.purchase_save_failed"),
            _ => Localize("fuel.purchase_unavailable"),
        };
    }

    public static string Get(ClaimAdFuelRewardFailure failure)
    {
        return failure switch
        {
            ClaimAdFuelRewardFailure.DailyLimitReached => Localize("fuel.ad_daily_limit"),
            ClaimAdFuelRewardFailure.Overflow => Localize("fuel.capacity_reached"),
            ClaimAdFuelRewardFailure.SaveFailed => Localize("fuel.ad_save_failed"),
            _ => Localize("fuel.ad_reward_unavailable"),
        };
    }

    private static string Localize(string key) =>
        LocalizationService.Get(LocalizationTables.SystemMessages, key);
}
