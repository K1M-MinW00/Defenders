public enum LimitBreakUnitFailure
{
    None,
    InvalidRequest,
    UnitNotFound,
    MaxLimitBreak,
    InsufficientDuplicates,
    SaveFailed,
}

public sealed class LimitBreakUnitResult
{
    public bool Succeeded => Failure == LimitBreakUnitFailure.None;
    public LimitBreakUnitFailure Failure { get; }
    public int LimitBreak { get; }
    public int DuplicateCount { get; }

    private LimitBreakUnitResult(
        LimitBreakUnitFailure failure,
        int limitBreak = 0,
        int duplicateCount = 0)
    {
        Failure = failure;
        LimitBreak = limitBreak;
        DuplicateCount = duplicateCount;
    }

    public static LimitBreakUnitResult Success(int limitBreak, int duplicateCount) =>
        new(LimitBreakUnitFailure.None, limitBreak, duplicateCount);

    public static LimitBreakUnitResult Fail(LimitBreakUnitFailure failure) =>
        new(failure);
}
