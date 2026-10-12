using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public sealed class StageOutcomeUseCase
{
    private readonly IUserDataRepository repository;
    private readonly string userId;
    private readonly UserDataRoot userData;
    private readonly IStageCatalog stages;

    public StageOutcomeUseCase(IUserDataRepository repository, string userId, UserDataRoot userData, IStageCatalog stages = null)
    {
        this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
        this.userId = string.IsNullOrWhiteSpace(userId)
            ? throw new ArgumentException("User ID is null or empty.", nameof(userId))
            : userId;
        this.userData = userData ?? throw new ArgumentNullException(nameof(userData));
        this.stages = stages;
    }

    public Task<StageOutcomeResult> CompleteAsync(StageDataSO stage, int fusionCount = 0, int fourStarFusionCount = 0)
    {
        if (stage == null)
            return Task.FromResult(StageOutcomeResult.Fail(StageOutcomeFailure.InvalidRequest));

        UserProgressData nextProgress;
        try
        {
            nextProgress = StageProgressRules.AdvanceAfterStageClear(
                userData.Progress,
                stage.sector,
                stage.stage,
                stages);
        }
        catch
        {
            return Task.FromResult(StageOutcomeResult.Fail(StageOutcomeFailure.InvalidRequest));
        }

        return GrantAndSaveAsync(stage.clearRewards, nextProgress, stage.waves?.Count ?? 0, fusionCount, fourStarFusionCount);
    }

    public Task<StageOutcomeResult> FailAsync(
        StageDataSO stage,
        int clearedWaveCount,
        int entryFuelCost,
        int fusionCount = 0,
        int fourStarFusionCount = 0)
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

        IReadOnlyList<RewardData> rewards = StageFailureRefundPolicy.BuildRewards(
            stage.failureRewards,
            entryFuelCost);
        return GrantAndSaveAsync(rewards, nextProgress, clearedWaveCount, fusionCount, fourStarFusionCount);
    }

    private async Task<StageOutcomeResult> GrantAndSaveAsync(
        IReadOnlyList<RewardData> rewards,
        UserProgressData nextProgress,
        int clearedWaveCount,
        int fusionCount,
        int fourStarFusionCount)
    {
        RewardGrantResult grant = RewardGrantCalculator.Calculate(userData, rewards);
        if (!grant.Succeeded)
            return StageOutcomeResult.Fail(StageOutcomeFailure.InvalidReward);
        UserQuestData nextQuest = UserDataCloner.Copy(userData.Quest);
        QuestProgressService.Apply(nextQuest, new QuestEvent(QuestEventType.WaveCleared, clearedWaveCount), DateTimeOffset.UtcNow);
        QuestProgressService.Apply(nextQuest, new QuestEvent(QuestEventType.UnitFused, fusionCount), DateTimeOffset.UtcNow);
        QuestProgressService.Apply(nextQuest, new QuestEvent(QuestEventType.UnitReachedFourStar, fourStarFusionCount), DateTimeOffset.UtcNow);

        try
        {
            await repository.SaveSectionsAsync(userId, new UserDataUpdate
            {
                Resources = grant.Resources,
                Inventory = grant.Inventory,
                Roster = grant.Roster,
                Profile = grant.Profile,
                Progress = nextProgress,
                Quest = nextQuest,
            });
        }
        catch
        {
            return StageOutcomeResult.Fail(StageOutcomeFailure.SaveFailed);
        }

        userData.Resource = grant.Resources;
        userData.Inventory = grant.Inventory;
        userData.Roster = grant.Roster;
        userData.Profile = grant.Profile;
        userData.Progress = nextProgress;
        userData.Quest = nextQuest;
        return StageOutcomeResult.Success(rewards);
    }
}
