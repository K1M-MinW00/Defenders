using Firebase.Firestore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public sealed class FirestoreMailboxRepository : IMailboxRepository
{
    private const string UsersCollection = "users";
    private const string MailboxCollection = "mailboxes";
    private const string MailCollection = "mails";
    private const int MaxClaimCount = 100;
    private const int DeleteBatchSize = 450;

    private readonly FirebaseFirestore firestore;

    public FirestoreMailboxRepository()
        : this(FirebaseFirestore.DefaultInstance)
    {
    }

    public FirestoreMailboxRepository(FirebaseFirestore firestore)
    {
        this.firestore = firestore ?? throw new ArgumentNullException(nameof(firestore));
    }

    public async Task<List<MailData>> LoadAsync(string userId)
    {
        ValidateUserId(userId);
        QuerySnapshot snapshot = await GetMailCollection(userId).GetSnapshotAsync();
        List<MailData> mails = new();

        foreach (DocumentSnapshot document in snapshot.Documents)
        {
            if (!document.Exists)
                continue;

            MailData mail = document.ConvertTo<MailData>();

            if (mail == null)
                continue;

            mail.MailId = document.Id;
            mails.Add(mail);
        }

        return mails;
    }

    public Task<MailboxClaimResult> ClaimAsync(
        string userId,
        IReadOnlyCollection<string> mailIds)
    {
        ValidateUserId(userId);

        List<string> distinctIds = mailIds?
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Distinct()
            .ToList();

        if (distinctIds == null || distinctIds.Count == 0 || distinctIds.Count > MaxClaimCount)
            return Task.FromResult(MailboxClaimResult.Fail(MailboxClaimFailure.InvalidRequest));

        DocumentReference userReference = firestore.Collection(UsersCollection).Document(userId);
        List<DocumentReference> mailReferences = distinctIds
            .Select(id => GetMailCollection(userId).Document(id))
            .ToList();

        return firestore.RunTransactionAsync(async transaction =>
        {
            DocumentSnapshot userSnapshot = await transaction.GetSnapshotAsync(userReference);

            if (!userSnapshot.Exists)
                return MailboxClaimResult.Fail(MailboxClaimFailure.UserNotFound);

            List<DocumentSnapshot> mailSnapshots = new(mailReferences.Count);

            foreach (DocumentReference mailReference in mailReferences)
                mailSnapshots.Add(await transaction.GetSnapshotAsync(mailReference));

            DateTime now = DateTime.UtcNow;
            List<MailData> claimableMails = mailSnapshots
                .Where(snapshot => snapshot.Exists)
                .Select(snapshot =>
                {
                    MailData mail = snapshot.ConvertTo<MailData>();

                    if (mail != null)
                        mail.MailId = snapshot.Id;

                    return mail;
                })
                .Where(mail => mail != null && !mail.Claimed && !IsExpired(mail, now))
                .ToList();

            if (claimableMails.Count == 0)
                return MailboxClaimResult.Fail(MailboxClaimFailure.NoClaimableMail);

            List<RewardData> rewards = claimableMails
                .Where(mail => mail.Rewards != null)
                .SelectMany(mail => mail.Rewards)
                .ToList();
            UserDataRoot currentData = userSnapshot.ConvertTo<UserDataRoot>();
            RewardGrantResult grantResult = RewardGrantCalculator.Calculate(currentData, rewards);

            if (!grantResult.Succeeded)
                return MailboxClaimResult.Fail(MailboxClaimFailure.InvalidReward);

            transaction.Update(userReference, new Dictionary<string, object>
            {
                { "Resource", grantResult.Resources },
                { "Inventory", grantResult.Inventory },
                { "Roster", grantResult.Roster },
                { "UpdatedAt", FieldValue.ServerTimestamp },
            });

            HashSet<string> claimedIds = claimableMails.Select(mail => mail.MailId).ToHashSet();

            foreach (DocumentReference mailReference in mailReferences)
            {
                if (claimedIds.Contains(mailReference.Id))
                    transaction.Update(mailReference, "Claimed", true);
            }

            return MailboxClaimResult.Success(
                claimedIds.ToList(),
                grantResult.Resources,
                grantResult.Inventory,
                grantResult.Roster);
        });
    }

    public async Task DeleteAsync(string userId, IReadOnlyCollection<string> mailIds)
    {
        ValidateUserId(userId);
        List<string> distinctIds = mailIds?
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Distinct()
            .ToList();

        if (distinctIds == null || distinctIds.Count == 0)
            return;

        for (int offset = 0; offset < distinctIds.Count; offset += DeleteBatchSize)
        {
            WriteBatch batch = firestore.StartBatch();

            foreach (string mailId in distinctIds.Skip(offset).Take(DeleteBatchSize))
                batch.Delete(GetMailCollection(userId).Document(mailId));

            await batch.CommitAsync();
        }
    }

    private CollectionReference GetMailCollection(string userId)
    {
        return firestore.Collection(MailboxCollection)
            .Document(userId)
            .Collection(MailCollection);
    }

    private static bool IsExpired(MailData mail, DateTime now)
    {
        return mail.ExpireAt == null || mail.ExpireAt.ToDateTime() <= now;
    }

    private static void ValidateUserId(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User ID is null or empty.", nameof(userId));
    }
}
