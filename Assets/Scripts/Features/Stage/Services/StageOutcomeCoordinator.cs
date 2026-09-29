using System;
using System.Threading.Tasks;

public sealed class StageOutcomeCoordinator
{
    private readonly StageRewardService rewardService;

    private StageDataSO pendingStage;
    private bool pendingIsClear;
    private int pendingClearedWaveCount;
    private Task<StageOutcomeResult> saveTask;

    public bool HasPendingOutcome => pendingStage != null;
    public bool PendingIsClear => pendingIsClear;

    public StageOutcomeCoordinator(StageRewardService rewardService)
    {
        this.rewardService = rewardService ?? throw new ArgumentNullException(nameof(rewardService));
    }

    public Task<StageOutcomeResult> SaveAsync(
        StageDataSO stage,
        bool isClear,
        int clearedWaveCount)
    {
        if (stage == null)
            return Task.FromResult(StageOutcomeResult.Fail(StageOutcomeFailure.InvalidRequest));

        if (saveTask != null)
            return saveTask;

        pendingStage = stage;
        pendingIsClear = isClear;
        pendingClearedWaveCount = clearedWaveCount;
        return StartPendingSaveAsync();
    }

    public Task<StageOutcomeResult> RetryPendingAsync()
    {
        if (!HasPendingOutcome)
            return Task.FromResult(StageOutcomeResult.Fail(StageOutcomeFailure.InvalidRequest));

        return saveTask ?? StartPendingSaveAsync();
    }

    public void Reset()
    {
        pendingStage = null;
        pendingIsClear = false;
        pendingClearedWaveCount = 0;
        saveTask = null;
    }

    private async Task<StageOutcomeResult> StartPendingSaveAsync()
    {
        saveTask = SaveCoreAsync();

        try
        {
            StageOutcomeResult result = await saveTask;
            if (result.Succeeded)
            {
                pendingStage = null;
                pendingClearedWaveCount = 0;
            }

            return result;
        }
        finally
        {
            saveTask = null;
        }
    }

    private Task<StageOutcomeResult> SaveCoreAsync()
    {
        return pendingIsClear
            ? rewardService.GiveStageClearRewardAsync(pendingStage)
            : rewardService.GiveStageFailRewardAsync(pendingStage, pendingClearedWaveCount);
    }
}
