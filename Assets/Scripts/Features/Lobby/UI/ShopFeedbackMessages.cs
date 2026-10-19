public static class ShopFeedbackMessages
{
    public static string Get(ShopPurchaseResult result)
    {
        if (result == null)
            return Localize("shop.processing_failed");
        if (result.Succeeded)
            return Localize("shop.completed");

        return result.Failure switch
        {
            ShopPurchaseFailure.InsufficientGem => Localize("shop.insufficient_gems"),
            ShopPurchaseFailure.SoldOut => Localize("shop.sold_out"),
            ShopPurchaseFailure.NotOnSale => Localize("shop.not_on_sale"),
            ShopPurchaseFailure.UnsupportedPurchaseType => Localize("shop.unsupported_purchase_type"),
            ShopPurchaseFailure.SaveFailed => Localize("common.save_failed"),
            ShopPurchaseFailure.InvalidReward => Localize("shop.invalid_reward"),
            ShopPurchaseFailure.Overflow => Localize("shop.capacity_exceeded"),
            _ => Localize("shop.invalid_request"),
        };
    }

    private static string Localize(string key) =>
        LocalizationService.Get(LocalizationTables.SystemMessages, key);
}
