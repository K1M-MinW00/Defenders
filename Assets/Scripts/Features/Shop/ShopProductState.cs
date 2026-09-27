using System;

public readonly struct ShopProductState
{
    public bool IsOnSale { get; }
    public bool IsSoldOut { get; }
    public int PurchasedCount { get; }
    public int RemainingCount { get; }
    public DateTimeOffset? NextResetUtc { get; }

    public ShopProductState(
        bool isOnSale,
        bool isSoldOut,
        int purchasedCount,
        int remainingCount,
        DateTimeOffset? nextResetUtc)
    {
        IsOnSale = isOnSale;
        IsSoldOut = isSoldOut;
        PurchasedCount = purchasedCount;
        RemainingCount = remainingCount;
        NextResetUtc = nextResetUtc;
    }
}
