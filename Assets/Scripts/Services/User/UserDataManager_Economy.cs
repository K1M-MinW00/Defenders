using System.Threading.Tasks;

public partial class UserDataManager
{
    public async Task<RecruitUnitsResult> RecruitUnitsAsync(RecruitUnitsCommand command)
    {
        if (GachaUseCase == null)
            return RecruitUnitsResult.Fail(RecruitUnitsFailure.InvalidRequest);

        return await RunSerializedMutationAsync(async () =>
        {
            RecruitUnitsResult result = await GachaUseCase.ExecuteAsync(command);
            if (!result.Succeeded)
                return result;

            RaiseResourceUpdated();
            RaiseInventoryUpdated();
            RaiseRosterUpdated();
            return result;
        });
    }

    public async Task<PurchaseFuelResult> PurchaseFuelAsync(int gemCost, int fuelAmount)
    {
        if (PurchaseFuelUseCase == null)
            return PurchaseFuelResult.Fail(PurchaseFuelFailure.InvalidRequest);

        return await RunSerializedMutationAsync(async () =>
        {
            PurchaseFuelResult result = await PurchaseFuelUseCase.ExecuteAsync(gemCost, fuelAmount);
            if (result.Succeeded)
                RaiseResourceUpdated();

            return result;
        });
    }

    public async Task<ClaimAdFuelRewardResult> ClaimAdFuelRewardAsync(int fuelAmount)
    {
        if (ClaimAdFuelRewardUseCase == null)
            return ClaimAdFuelRewardResult.Fail(ClaimAdFuelRewardFailure.InvalidRequest);

        return await RunSerializedMutationAsync(async () =>
        {
            ClaimAdFuelRewardResult result = await ClaimAdFuelRewardUseCase.ExecuteAsync(fuelAmount);
            if (result.Succeeded)
                RaiseResourceUpdated();

            return result;
        });
    }
}
