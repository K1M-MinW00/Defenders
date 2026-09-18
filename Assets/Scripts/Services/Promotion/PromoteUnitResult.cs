public enum PromoteUnitFailure
{
    None,
    InvalidRequest,
    UnitNotFound,
    MaxPromotion,
    InvalidCost,
    InsufficientMaterials,
    SaveFailed,
}

public sealed class PromoteUnitResult
{
    public bool Succeeded => Failure == PromoteUnitFailure.None;
    public PromoteUnitFailure Failure { get; }
    public int Promotion { get; }

    private PromoteUnitResult(PromoteUnitFailure failure, int promotion = 0)
    {
        Failure = failure;
        Promotion = promotion;
    }

    public static PromoteUnitResult Success(int promotion) =>
        new(PromoteUnitFailure.None, promotion);

    public static PromoteUnitResult Fail(PromoteUnitFailure failure) =>
        new(failure);
}
