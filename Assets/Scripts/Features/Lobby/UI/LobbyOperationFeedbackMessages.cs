public static class LobbyOperationFeedbackMessages
{
    public const string AdUnavailable = "광고를 불러오지 못했습니다. 잠시 후 다시 시도해 주세요.";
    public const string MailboxLoadFailed = "우편함을 불러오지 못했습니다. 잠시 후 다시 시도해 주세요.";
    public const string MailboxDeleteFailed = "우편 삭제에 실패했습니다. 잠시 후 다시 시도해 주세요.";

    public static string Get(RecruitUnitsFailure failure)
    {
        return failure switch
        {
            RecruitUnitsFailure.InvalidBanner => "현재 소환 배너를 사용할 수 없습니다.",
            RecruitUnitsFailure.InsufficientCurrency => "소환 재화가 부족합니다.",
            RecruitUnitsFailure.EmptyPool => "소환 가능한 유닛이 없습니다.",
            RecruitUnitsFailure.SaveFailed => "소환 결과 저장에 실패했습니다. 잠시 후 다시 시도해 주세요.",
            _ => "소환을 진행할 수 없습니다.",
        };
    }

    public static string Get(MailboxClaimFailure failure)
    {
        return failure switch
        {
            MailboxClaimFailure.UserNotFound => "사용자 정보를 찾을 수 없습니다.",
            MailboxClaimFailure.NoClaimableMail => "받을 수 있는 우편이 없습니다.",
            MailboxClaimFailure.InvalidReward => "우편 보상 정보가 올바르지 않습니다.",
            MailboxClaimFailure.SaveFailed => "우편 보상 저장에 실패했습니다. 잠시 후 다시 시도해 주세요.",
            _ => "우편 보상을 받을 수 없습니다.",
        };
    }

    public static string Get(PurchaseFuelFailure failure)
    {
        return failure switch
        {
            PurchaseFuelFailure.InsufficientGem => "보석이 부족합니다.",
            PurchaseFuelFailure.Overflow => "연료를 더 보유할 수 없습니다.",
            PurchaseFuelFailure.SaveFailed => "연료 구매 저장에 실패했습니다. 잠시 후 다시 시도해 주세요.",
            _ => "연료를 구매할 수 없습니다.",
        };
    }

    public static string Get(ClaimAdFuelRewardFailure failure)
    {
        return failure switch
        {
            ClaimAdFuelRewardFailure.DailyLimitReached => "오늘 받을 수 있는 광고 보상을 모두 받았습니다.",
            ClaimAdFuelRewardFailure.Overflow => "연료를 더 보유할 수 없습니다.",
            ClaimAdFuelRewardFailure.SaveFailed => "광고 보상 저장에 실패했습니다. 잠시 후 다시 시도해 주세요.",
            _ => "광고 보상을 받을 수 없습니다.",
        };
    }
}
