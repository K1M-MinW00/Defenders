using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public sealed class MailboxService
{
    private readonly IMailboxRepository repository;
    private readonly string userId;
    private readonly UserDataRoot userData;

    public List<MailData> CachedMails { get; private set; } = new();

    public MailboxService(
        string userId,
        UserDataRoot userData,
        IMailboxRepository repository = null)
    {
        this.userId = userId;
        this.userData = userData;
        this.repository = repository ?? new FirestoreMailboxRepository();
    }

    public async Task LoadMailsAsync()
    {
        List<MailData> mails = await repository.LoadAsync(userId);
        List<string> expiredIds = mails
            .Where(IsExpired)
            .Select(mail => mail.MailId)
            .ToList();

        if (expiredIds.Count > 0)
            await repository.DeleteAsync(userId, expiredIds);

        CachedMails = mails
            .Where(mail => !IsExpired(mail))
            .OrderByDescending(mail => mail.CreatedAt)
            .ToList();
    }

    public Task<MailboxClaimResult> ClaimMailAsync(MailData mail)
    {
        if (mail == null || mail.Claimed || IsExpired(mail))
            return Task.FromResult(MailboxClaimResult.Fail(MailboxClaimFailure.InvalidRequest));

        return ClaimAsync(new[] { mail.MailId });
    }

    public Task<MailboxClaimResult> ClaimAllAsync()
    {
        List<string> claimableIds = CachedMails
            .Where(mail => mail != null && !mail.Claimed && !IsExpired(mail))
            .Select(mail => mail.MailId)
            .Take(100)
            .ToList();

        if (claimableIds.Count == 0)
            return Task.FromResult(MailboxClaimResult.Fail(MailboxClaimFailure.NoClaimableMail));

        return ClaimAsync(claimableIds);
    }

    public async Task DeleteAllAsync()
    {
        List<string> deleteIds = CachedMails
            .Where(mail => mail != null && mail.Claimed)
            .Select(mail => mail.MailId)
            .ToList();

        if (deleteIds.Count == 0)
            return;

        await repository.DeleteAsync(userId, deleteIds);
        CachedMails.RemoveAll(mail => mail != null && deleteIds.Contains(mail.MailId));
    }

    private async Task<MailboxClaimResult> ClaimAsync(IReadOnlyCollection<string> mailIds)
    {
        try
        {
            MailboxClaimResult result = await repository.ClaimAsync(userId, mailIds);

            if (!result.Succeeded)
                return result;

            userData.Resource = result.Resources;
            userData.Inventory = result.Inventory;
            userData.Roster = result.Roster;

            HashSet<string> claimedIds = result.ClaimedMailIds.ToHashSet();

            foreach (MailData cachedMail in CachedMails)
            {
                if (cachedMail != null && claimedIds.Contains(cachedMail.MailId))
                    cachedMail.Claimed = true;
            }

            return result;
        }
        catch (Exception exception)
        {
            Debug.LogError($"[MailboxService] Claim failed: {exception}");
            return MailboxClaimResult.Fail(MailboxClaimFailure.SaveFailed);
        }
    }

    private static bool IsExpired(MailData mail)
    {
        return mail?.ExpireAt == null || mail.ExpireAt.ToDateTime() <= DateTime.UtcNow;
    }
}
