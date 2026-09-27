using System.Collections.Generic;
using System.Threading.Tasks;

public partial class UserDataManager
{
    public async Task<StageEntryFuelResult> ConsumeStageEntryFuelAsync(int amount)
    {
        if (StageEntryFuelUseCase == null)
            return StageEntryFuelResult.Fail(StageEntryFuelFailure.InvalidRequest);

        return await RunSerializedMutationAsync(async () =>
        {
            StageEntryFuelResult result = await StageEntryFuelUseCase.ConsumeAsync(amount);
            if (result.Succeeded)
                RaiseResourceUpdated();
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

    public async Task<StageOutcomeResult> CompleteStageAsync(StageDataSO stage)
    {
        if (StageOutcomeUseCase == null)
            return StageOutcomeResult.Fail(StageOutcomeFailure.InvalidRequest);

        return await RunSerializedMutationAsync(async () =>
        {
            StageOutcomeResult result = await StageOutcomeUseCase.CompleteAsync(stage);
            if (result.Succeeded)
                RaiseStageOutcomeUpdated();
            return result;
        });
    }

    public async Task<StageOutcomeResult> FailStageAsync(
        StageDataSO stage,
        int clearedWaveCount)
    {
        if (StageOutcomeUseCase == null)
            return StageOutcomeResult.Fail(StageOutcomeFailure.InvalidRequest);

        return await RunSerializedMutationAsync(async () =>
        {
            StageOutcomeResult result =
                await StageOutcomeUseCase.FailAsync(stage, clearedWaveCount);
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
    }
}
