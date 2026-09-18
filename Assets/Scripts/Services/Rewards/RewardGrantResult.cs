public enum RewardGrantFailure
{
    None,
    InvalidData,
    InvalidReward,
    Overflow,
}

public sealed class RewardGrantResult
{
    public bool Succeeded => Failure == RewardGrantFailure.None;
    public RewardGrantFailure Failure { get; }
    public UserResourceData Resources { get; }
    public UserInventoryData Inventory { get; }
    public UserRosterData Roster { get; }

    private RewardGrantResult(
        RewardGrantFailure failure,
        UserResourceData resources = null,
        UserInventoryData inventory = null,
        UserRosterData roster = null)
    {
        Failure = failure;
        Resources = resources;
        Inventory = inventory;
        Roster = roster;
    }

    public static RewardGrantResult Success(
        UserResourceData resources,
        UserInventoryData inventory,
        UserRosterData roster) =>
        new(RewardGrantFailure.None, resources, inventory, roster);

    public static RewardGrantResult Fail(RewardGrantFailure failure) => new(failure);
}
