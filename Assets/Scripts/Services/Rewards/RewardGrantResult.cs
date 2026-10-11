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
    public int DiscardedEquipmentCount { get; }

    private RewardGrantResult(
        RewardGrantFailure failure,
        UserResourceData resources = null,
        UserInventoryData inventory = null,
        UserRosterData roster = null,
        UserProfileData profile = null,
        int discardedEquipmentCount = 0)
    {
        Failure = failure;
        Resources = resources;
        Inventory = inventory;
        Roster = roster;
        Profile = profile;
        DiscardedEquipmentCount = discardedEquipmentCount;
    }

    public static RewardGrantResult Success(
        UserResourceData resources,
        UserInventoryData inventory,
        UserRosterData roster,
        UserProfileData profile,
        int discardedEquipmentCount = 0) =>
        new(RewardGrantFailure.None, resources, inventory, roster, profile, discardedEquipmentCount);

    public static RewardGrantResult Fail(RewardGrantFailure failure) => new(failure);
}
