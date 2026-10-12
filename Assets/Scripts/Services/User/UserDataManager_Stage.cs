using System.Collections.Generic;
using System.Threading.Tasks;

public partial class UserDataManager
{
    public bool HasClaimableWaveMilestoneReward() =>
        WaveMilestoneRewardRules.HasClaimableReward(UserData?.Progress, GameConfig.Stages);

    public Task<WaveMilestoneClaimResult> ClaimWaveMilestoneRewardAsync(int requiredWave) =>
        ClaimWaveMilestoneRewardInternalAsync(requiredWave, false);

    public Task<WaveMilestoneClaimResult> ClaimAllWaveMilestoneRewardsAsync() =>
        ClaimWaveMilestoneRewardInternalAsync(0, true);

    private async Task<WaveMilestoneClaimResult> ClaimWaveMilestoneRewardInternalAsync(int requiredWave, bool all)
    {
        if (WaveMilestoneRewardClaimUseCase == null)
            return WaveMilestoneClaimResult.Fail(WaveMilestoneClaimFailure.InvalidRequest);

        return await RunSerializedMutationAsync(async () =>
        {
            WaveMilestoneClaimResult result = all
                ? await WaveMilestoneRewardClaimUseCase.ClaimAllAsync()
                : await WaveMilestoneRewardClaimUseCase.ClaimAsync(requiredWave);
            if (result.Succeeded)
                RaiseStageOutcomeUpdated();
            return result;
        });
    }

    public async Task<StageEntryFuelResult> ConsumeStageEntryFuelAsync(int amount)
    {
        if (StageEntryFuelUseCase == null)
            return StageEntryFuelResult.Fail(StageEntryFuelFailure.InvalidRequest);

        return await RunSerializedMutationAsync(async () =>
        {
            StageEntryFuelResult result = await StageEntryFuelUseCase.ConsumeAsync(amount);
            if (result.Succeeded)
            {
                RaiseResourceUpdated();
                RaiseQuestUpdated();
            }
            return result;
        });
    }

    public async Task<bool> RefundStageEntryFuelAsync(int amount)
    {
        if (StageEntryFuelUseCase == null)
            return false;

        return await RunSerializedMutationAsync(async () =>
        {
            bool refunded = await StageEntryFuelUseCase.RefundAsync(amount);
            if (refunded)
                RaiseResourceUpdated();
            return refunded;
        });
    }

    public async Task<StageOutcomeResult> CompleteStageAsync(StageDataSO stage, int fusionCount = 0, int fourStarFusionCount = 0)
    {
        if (StageOutcomeUseCase == null)
            return StageOutcomeResult.Fail(StageOutcomeFailure.InvalidRequest);

        return await RunSerializedMutationAsync(async () =>
        {
            StageOutcomeResult result = await StageOutcomeUseCase.CompleteAsync(stage, fusionCount, fourStarFusionCount);
            if (result.Succeeded)
                RaiseStageOutcomeUpdated();
            return result;
        });
    }

    public async Task<StageOutcomeResult> FailStageAsync(
        StageDataSO stage,
        int clearedWaveCount,
        int entryFuelCost,
        int fusionCount = 0,
        int fourStarFusionCount = 0)
    {
        if (StageOutcomeUseCase == null)
            return StageOutcomeResult.Fail(StageOutcomeFailure.InvalidRequest);

        return await RunSerializedMutationAsync(async () =>
        {
            StageOutcomeResult result =
                await StageOutcomeUseCase.FailAsync(stage, clearedWaveCount, entryFuelCost, fusionCount, fourStarFusionCount);
            if (result.Succeeded)
                RaiseStageOutcomeUpdated();
            return result;
        });
    }

    private void RaiseStageOutcomeUpdated()
    {
        RaiseResourceUpdated();
        RaiseProfileUpdated();
        RaiseInventoryUpdated();
        RaiseRosterUpdated();
        RaiseProgressUpdated();
        RaiseQuestUpdated();
    }
}
