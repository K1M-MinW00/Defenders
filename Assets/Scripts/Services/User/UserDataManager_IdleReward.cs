using System;
using System.Threading.Tasks;

public partial class UserDataManager
{
    public event Action OnIdleRewardUpdated;

    public IdleRewardPreview GetIdleRewardPreview() =>
        IdleRewardUseCase?.BuildPreview(DateTime.UtcNow);

    public async Task<IdleRewardClaimFailure> ClaimIdleRewardAsync()
    {
        if (IdleRewardUseCase == null)
            return IdleRewardClaimFailure.InvalidData;

        return await RunSerializedMutationAsync(async () =>
        {
            IdleRewardClaimFailure result = await IdleRewardUseCase.ClaimAsync();
            if (result == IdleRewardClaimFailure.None)
            {
                RaiseResourceUpdated();
                RaiseInventoryUpdated();
                OnIdleRewardUpdated?.Invoke();
            }
            return result;
        });
    }
}
