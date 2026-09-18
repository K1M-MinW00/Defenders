public enum ClaimAdFuelRewardFailure
{
    None,
    InvalidRequest,
    DailyLimitReached,
    Overflow,
    SaveFailed,
}

public sealed class ClaimAdFuelRewardResult
{
    public bool Succeeded => Failure == ClaimAdFuelRewardFailure.None;
    public ClaimAdFuelRewardFailure Failure { get; }

    private ClaimAdFuelRewardResult(ClaimAdFuelRewardFailure failure)
    {
        Failure = failure;
    }

    public static ClaimAdFuelRewardResult Success() => new(ClaimAdFuelRewardFailure.None);
    public static ClaimAdFuelRewardResult Fail(ClaimAdFuelRewardFailure failure) => new(failure);
}
