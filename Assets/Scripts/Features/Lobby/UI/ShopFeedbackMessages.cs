public static class ShopFeedbackMessages
{
    public static string Get(ShopPurchaseResult result)
    {
        if (result == null)
            return "구매 처리에 실패했습니다.";
        if (result.Succeeded)
            return "구매가 완료되었습니다.";

        return result.Failure switch
        {
            ShopPurchaseFailure.InsufficientGem => "Gem이 부족합니다.",
            ShopPurchaseFailure.SoldOut => "구매 가능한 수량을 모두 구매했습니다.",
            ShopPurchaseFailure.NotOnSale => "현재 판매 중인 상품이 아닙니다.",
            ShopPurchaseFailure.UnsupportedPurchaseType => "아직 지원하지 않는 결제 방식입니다.",
            ShopPurchaseFailure.SaveFailed => "저장에 실패했습니다. 잠시 후 다시 시도해 주세요.",
            ShopPurchaseFailure.InvalidReward => "상품 보상 정보가 올바르지 않습니다.",
            ShopPurchaseFailure.Overflow => "보유 한도를 초과해 구매할 수 없습니다.",
            _ => "구매 요청이 올바르지 않습니다.",
        };
    }
}
