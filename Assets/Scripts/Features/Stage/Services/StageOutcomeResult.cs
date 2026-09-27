using System.Collections.Generic;

public enum StageOutcomeFailure
{
    None,
    InvalidRequest,
    InvalidReward,
    SaveFailed,
}

public sealed class StageOutcomeResult
{
    public bool Succeeded => Failure == StageOutcomeFailure.None;
    public StageOutcomeFailure Failure { get; }
    public IReadOnlyList<RewardData> GrantedRewards { get; }

    private StageOutcomeResult(StageOutcomeFailure failure, IReadOnlyList<RewardData> rewards = null)
    {
        Failure = failure;
        GrantedRewards = rewards ?? System.Array.Empty<RewardData>();
    }

    public static StageOutcomeResult Success(IReadOnlyList<RewardData> rewards) =>
        new(StageOutcomeFailure.None, rewards);

    public static StageOutcomeResult Fail(StageOutcomeFailure failure) => new(failure);
}
