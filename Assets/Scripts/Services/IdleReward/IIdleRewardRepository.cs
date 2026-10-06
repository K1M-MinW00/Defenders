using System.Threading.Tasks;

public interface IIdleRewardRepository
{
    Task<IdleRewardTransactionResult> ClaimAsync(string userId, IdleRewardConfigSO config);
}

public sealed class IdleRewardTransactionResult
{
    public IdleRewardClaimFailure Failure { get; }
    public UserResourceData Resources { get; }
    public UserInventoryData Inventory { get; }
    public UserIdleRewardData IdleReward { get; }

    private IdleRewardTransactionResult(
        IdleRewardClaimFailure failure,
        UserResourceData resources = null,
        UserInventoryData inventory = null,
        UserIdleRewardData idleReward = null)
    {
        Failure = failure;
        Resources = resources;
        Inventory = inventory;
        IdleReward = idleReward;
    }

    public static IdleRewardTransactionResult Success(UserResourceData resources, UserInventoryData inventory, UserIdleRewardData idleReward) =>
        new(IdleRewardClaimFailure.None, resources, inventory, idleReward);

    public static IdleRewardTransactionResult Fail(IdleRewardClaimFailure failure) => new(failure);
}
