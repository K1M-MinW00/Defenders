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
    public UserProfileData Profile { get; }

    private RewardGrantResult(
        RewardGrantFailure failure,
        UserResourceData resources = null,
        UserInventoryData inventory = null,
        UserRosterData roster = null,
        UserProfileData profile = null)
    {
        Failure = failure;
        Resources = resources;
        Inventory = inventory;
        Roster = roster;
        Profile = profile;
    }

    public static RewardGrantResult Success(
        UserResourceData resources,
        UserInventoryData inventory,
        UserRosterData roster,
        UserProfileData profile) =>
        new(RewardGrantFailure.None, resources, inventory, roster, profile);

    public static RewardGrantResult Fail(RewardGrantFailure failure) => new(failure);
}
