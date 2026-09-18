public enum PurchaseFuelFailure
{
    None,
    InvalidRequest,
    InsufficientGem,
    Overflow,
    SaveFailed,
}

public sealed class PurchaseFuelResult
{
    public bool Succeeded => Failure == PurchaseFuelFailure.None;
    public PurchaseFuelFailure Failure { get; }

    private PurchaseFuelResult(PurchaseFuelFailure failure)
    {
        Failure = failure;
    }

    public static PurchaseFuelResult Success() => new(PurchaseFuelFailure.None);
    public static PurchaseFuelResult Fail(PurchaseFuelFailure failure) => new(failure);
}
