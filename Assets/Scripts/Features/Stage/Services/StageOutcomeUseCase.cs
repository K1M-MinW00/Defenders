using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public sealed class StageOutcomeUseCase
{
    private readonly IUserDataRepository repository;
    private readonly string userId;
    private readonly UserDataRoot userData;

    public StageOutcomeUseCase(IUserDataRepository repository, string userId, UserDataRoot userData)
    {
        this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
        this.userId = string.IsNullOrWhiteSpace(userId)
            ? throw new ArgumentException("User ID is null or empty.", nameof(userId))
            : userId;
        this.userData = userData ?? throw new ArgumentNullException(nameof(userData));
    }

    public Task<StageOutcomeResult> CompleteAsync(StageDataSO stage)
    {
        if (stage == null)
            return Task.FromResult(StageOutcomeResult.Fail(StageOutcomeFailure.InvalidRequest));

        UserProgressData nextProgress;
        try
        {
            nextProgress = StageProgressRules.AdvanceAfterStageClear(
                userData.Progress,
                stage.sector,
                stage.stage);
        }
        catch
        {
            return Task.FromResult(StageOutcomeResult.Fail(StageOutcomeFailure.InvalidRequest));
        }

        return GrantAndSaveAsync(stage.clearRewards, nextProgress);
    }

    public Task<StageOutcomeResult> FailAsync(
        StageDataSO stage,
        int clearedWaveCount,
        IReadOnlyList<RewardData> failureRewards)
    {
        if (stage == null)
            return Task.FromResult(StageOutcomeResult.Fail(StageOutcomeFailure.InvalidRequest));

        UserProgressData nextProgress;
        try
        {
            nextProgress = StageProgressRules.RecordClearedWave(
                userData.Progress,
                stage.sector,
                stage.stage,
                clearedWaveCount);
        }
        catch
        {
            return Task.FromResult(StageOutcomeResult.Fail(StageOutcomeFailure.InvalidRequest));
        }

        return GrantAndSaveAsync(failureRewards, nextProgress);
    }

    private async Task<StageOutcomeResult> GrantAndSaveAsync(
        IReadOnlyList<RewardData> rewards,
        UserProgressData nextProgress)
    {
        RewardGrantResult grant = RewardGrantCalculator.Calculate(userData, rewards);
        if (!grant.Succeeded)
            return StageOutcomeResult.Fail(StageOutcomeFailure.InvalidReward);

        try
        {
            await repository.SaveSectionsAsync(userId, new UserDataUpdate
            {
                Resources = grant.Resources,
                Inventory = grant.Inventory,
                Roster = grant.Roster,
                Progress = nextProgress,
            });
        }
        catch
        {
            return StageOutcomeResult.Fail(StageOutcomeFailure.SaveFailed);
        }

        userData.Resource = grant.Resources;
        userData.Inventory = grant.Inventory;
        userData.Roster = grant.Roster;
        userData.Progress = nextProgress;
        return StageOutcomeResult.Success(rewards);
    }
}
