public enum ShopPurchaseFailure
{
    None,
    InvalidProduct,
    UnsupportedPurchaseType,
    NotOnSale,
    SoldOut,
    InsufficientGem,
    InvalidReward,
    Overflow,
    SaveFailed,
}

public sealed class ShopPurchaseResult
{
    public bool Succeeded => Failure == ShopPurchaseFailure.None;
    public ShopPurchaseFailure Failure { get; }
    public int RemainingPurchases { get; }

    private ShopPurchaseResult(ShopPurchaseFailure failure, int remainingPurchases = -1)
    {
        Failure = failure;
        RemainingPurchases = remainingPurchases;
    }

    public static ShopPurchaseResult Success(int remainingPurchases) =>
        new(ShopPurchaseFailure.None, remainingPurchases);

    public static ShopPurchaseResult Fail(ShopPurchaseFailure failure) => new(failure);
}
