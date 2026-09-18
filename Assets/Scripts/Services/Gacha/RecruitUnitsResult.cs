using System.Collections.Generic;

public enum RecruitUnitsFailure
{
    None,
    InvalidRequest,
    InvalidBanner,
    InsufficientCurrency,
    EmptyPool,
    SaveFailed,
}

public sealed class RecruitUnitsResult
{
    public bool Succeeded => Failure == RecruitUnitsFailure.None;
    public RecruitUnitsFailure Failure { get; }
    public IReadOnlyList<GachaResult> Results { get; }

    private RecruitUnitsResult(
        RecruitUnitsFailure failure,
        IReadOnlyList<GachaResult> results = null)
    {
        Failure = failure;
        Results = results ?? new List<GachaResult>();
    }

    public static RecruitUnitsResult Success(IReadOnlyList<GachaResult> results) =>
        new(RecruitUnitsFailure.None, results);

    public static RecruitUnitsResult Fail(RecruitUnitsFailure failure) =>
        new(failure);
}
