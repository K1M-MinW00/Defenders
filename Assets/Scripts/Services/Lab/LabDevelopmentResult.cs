using System.Collections.Generic;

public enum LabDevelopmentFailure
{
    None,
    InvalidRequest,
    LevelLocked,
    InsufficientCurrency,
    AllCardsAcquired,
    SaveFailed,
}

public sealed class LabOfferResult
{
    public LabDevelopmentFailure Failure { get; }
    public IReadOnlyList<LabCardDataSO> Cards { get; }
    public int Cost { get; }
    public bool Succeeded => Failure == LabDevelopmentFailure.None;

    private LabOfferResult(LabDevelopmentFailure failure, IReadOnlyList<LabCardDataSO> cards, int cost)
    {
        Failure = failure;
        Cards = cards;
        Cost = cost;
    }

    public static LabOfferResult Success(IReadOnlyList<LabCardDataSO> cards, int cost) => new(LabDevelopmentFailure.None, cards, cost);
    public static LabOfferResult Fail(LabDevelopmentFailure failure) => new(failure, System.Array.Empty<LabCardDataSO>(), 0);
}
