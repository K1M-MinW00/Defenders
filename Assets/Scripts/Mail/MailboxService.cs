using Firebase.Firestore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class MailboxService
{
    private FirebaseFirestore db;

    public List<MailData> CachedMails { get; private set; } = new();

    public MailboxService()
    {
        db = FirebaseFirestore.DefaultInstance;
    }

    public async Task LoadMailsAsync(string uid)
    {
        CachedMails.Clear();

        QuerySnapshot snapshot = await db.Collection("mailboxes").Document(uid).Collection("mails").GetSnapshotAsync();

        foreach (DocumentSnapshot doc in snapshot.Documents)
        {
            if (!doc.Exists)
                continue;

            MailData mail = doc.ConvertTo<MailData>();

            if (mail.Deleted)
                continue;

            CachedMails.Add(mail);
        }

        CachedMails = CachedMails.OrderByDescending(x => x.CreatedAt).ToList();
    }

    public async Task ClaimMailAsync(string uid,MailData mail)
    {
        if (mail.Claimed)
            return;

        RewardService.GiveRewards(mail.Rewards);

        mail.Claimed = true;

        await SaveMailAsync(uid, mail);

        await UserDataManager.Instance.SaveAsync();
    }

    public async Task ClaimAllAsync(string uid)
    {
        bool changed = false;

        foreach (MailData mail in CachedMails)
        {
            if (mail.Claimed)
                continue;

            RewardService.GiveRewards(mail.Rewards);

            mail.Claimed = true;

            await SaveMailAsync(uid, mail);

            changed = true;
        }

        if (changed)
        {
            await UserDataManager.Instance.SaveAsync();
        }
    }

    public async Task DeleteAllAsync(string uid)
    {
        List<MailData> deleteTargets =CachedMails.Where(x => x.Claimed).ToList();

        foreach (MailData mail in deleteTargets)
        {
            mail.Deleted = true;

            await SaveMailAsync(uid, mail);
        }

        CachedMails.RemoveAll(x => x.Claimed);
    }

    private async Task SaveMailAsync(string uid,MailData mail)
    {
        await db.Collection("mailboxes").Document(uid).Collection("mails").Document(mail.MailId).SetAsync(mail);
    }
}