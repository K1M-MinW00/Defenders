using System.Collections.Generic;

public enum MailboxClaimFailure
{
    None,
    InvalidRequest,
    UserNotFound,
    NoClaimableMail,
    InvalidReward,
    SaveFailed,
}

public sealed class MailboxClaimResult
{
    public bool Succeeded => Failure == MailboxClaimFailure.None;
    public MailboxClaimFailure Failure { get; }
    public IReadOnlyList<string> ClaimedMailIds { get; }
    public UserResourceData Resources { get; }
    public UserInventoryData Inventory { get; }
    public UserRosterData Roster { get; }

    private MailboxClaimResult(
        MailboxClaimFailure failure,
        IReadOnlyList<string> claimedMailIds = null,
        UserResourceData resources = null,
        UserInventoryData inventory = null,
        UserRosterData roster = null)
    {
        Failure = failure;
        ClaimedMailIds = claimedMailIds ?? new List<string>();
        Resources = resources;
        Inventory = inventory;
        Roster = roster;
    }

    public static MailboxClaimResult Success(
        IReadOnlyList<string> claimedMailIds,
        UserResourceData resources,
        UserInventoryData inventory,
        UserRosterData roster) =>
        new(MailboxClaimFailure.None, claimedMailIds, resources, inventory, roster);

    public static MailboxClaimResult Fail(MailboxClaimFailure failure) => new(failure);
}
