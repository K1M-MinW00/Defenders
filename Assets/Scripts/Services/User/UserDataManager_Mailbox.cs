using System.Threading.Tasks;

public partial class UserDataManager
{
    public async Task<MailboxClaimResult> ClaimMailAsync(MailData mail)
    {
        if (MailboxService == null)
            return MailboxClaimResult.Fail(MailboxClaimFailure.InvalidRequest);

        return await RunSerializedMutationAsync(async () =>
        {
            MailboxClaimResult result = await MailboxService.ClaimMailAsync(mail);
            NotifyMailboxRewardChanged(result);
            return result;
        });
    }

    public async Task<MailboxClaimResult> ClaimAllMailAsync()
    {
        if (MailboxService == null)
            return MailboxClaimResult.Fail(MailboxClaimFailure.InvalidRequest);

        return await RunSerializedMutationAsync(async () =>
        {
            MailboxClaimResult result = await MailboxService.ClaimAllAsync();
            NotifyMailboxRewardChanged(result);
            return result;
        });
    }

    private void NotifyMailboxRewardChanged(MailboxClaimResult result)
    {
        if (result == null || !result.Succeeded)
            return;

        RaiseResourceUpdated();
        RaiseInventoryUpdated();
        RaiseRosterUpdated();
    }
}
