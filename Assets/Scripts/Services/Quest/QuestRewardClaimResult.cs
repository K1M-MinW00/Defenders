using System;
using System.Collections.Generic;

public enum QuestRewardClaimFailure
{
    None,
    InvalidReward,
    InsufficientScore,
    AlreadyClaimed,
    GrantFailed,
    SaveFailed,
}

public enum QuestScoreClaimFailure
{
    None,
    InvalidQuest,
    NotCompleted,
    AlreadyClaimed,
    SaveFailed,
}

public sealed class QuestScoreClaimResult
{
    public bool Succeeded => Failure == QuestScoreClaimFailure.None;
    public QuestScoreClaimFailure Failure { get; }

    private QuestScoreClaimResult(QuestScoreClaimFailure failure) => Failure = failure;
    public static QuestScoreClaimResult Success() => new(QuestScoreClaimFailure.None);
    public static QuestScoreClaimResult Fail(QuestScoreClaimFailure failure) => new(failure);
}

public sealed class QuestRewardClaimResult
{
    public bool Succeeded => Failure == QuestRewardClaimFailure.None;
    public QuestRewardClaimFailure Failure { get; }
    public IReadOnlyList<RewardData> GrantedRewards { get; }

    private QuestRewardClaimResult(QuestRewardClaimFailure failure, IReadOnlyList<RewardData> grantedRewards)
    {
        Failure = failure;
        GrantedRewards = grantedRewards ?? Array.Empty<RewardData>();
    }

    public static QuestRewardClaimResult Success(IReadOnlyList<RewardData> grantedRewards) =>
        new(QuestRewardClaimFailure.None, grantedRewards);

    public static QuestRewardClaimResult Fail(QuestRewardClaimFailure failure) =>
        new(failure, Array.Empty<RewardData>());
}

