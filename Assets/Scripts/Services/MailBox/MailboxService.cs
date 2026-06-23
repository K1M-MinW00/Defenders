using Firebase.Firestore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class MailboxService
{
    private const string MailboxCollection = "mailboxes";
    private const string MailCollection = "mails";

    private readonly FirebaseFirestore db;
    public List<MailData> CachedMails { get; private set; } = new();

    private string UserId => UserDataManager.Instance.CurrentUserId;

    public MailboxService()
    {
        db = FirebaseFirestore.DefaultInstance;
    }

    private bool IsExpired(MailData mail)
    {
        return mail.ExpireAt.ToDateTime() <= DateTime.UtcNow;
    }

    public async Task LoadMailsAsync()
    {
        CachedMails.Clear();

        QuerySnapshot snapshot = await db.Collection(MailboxCollection).Document(UserId).Collection(MailCollection).GetSnapshotAsync();

        foreach (DocumentSnapshot doc in snapshot.Documents)
        {
            if (!doc.Exists)
                continue;

            MailData mail = doc.ConvertTo<MailData>();

            if (IsExpired(mail))
            {
                await DeleteMailAsync(mail);
                continue;
            }

            CachedMails.Add(mail);
        }

        CachedMails = CachedMails.OrderByDescending(x => x.CreatedAt).ToList();
    }

    public async Task ClaimMailAsync(MailData mail)
    {
        if (mail == null || mail.Claimed)
            return;

        UserDataManager.Instance.RewardService.GiveRewards(mail.Rewards);

        mail.Claimed = true;

        await SaveMailAsync(mail);

        await UserDataManager.Instance.SaveAsync();
    }

    public async Task ClaimAllAsync()
    {
        bool changed = false;

        foreach (MailData mail in CachedMails)
        {
            if (mail.Claimed)
                continue;

            UserDataManager.Instance.RewardService.GiveRewards(mail.Rewards);

            mail.Claimed = true;

            await SaveMailAsync(mail);

            changed = true;
        }

        if (changed)
        {
            await UserDataManager.Instance.SaveAsync();
        }
    }

    public async Task DeleteAllAsync()
    {
        List<MailData> deleteTargets = CachedMails.Where(x => x.Claimed).ToList();

        foreach (MailData mail in deleteTargets)
        {
            await DeleteMailAsync(mail);
        }

        CachedMails.RemoveAll(x => x.Claimed);
    }

    private async Task SaveMailAsync(MailData mail)
    {
        await db.Collection(MailboxCollection).Document(UserId).Collection(MailCollection).Document(mail.MailId).SetAsync(mail);
    }
    private async Task DeleteMailAsync(MailData mail)
    {
        await db.Collection(MailboxCollection).Document(UserId).Collection(MailCollection).Document(mail.MailId).DeleteAsync();
    }
}