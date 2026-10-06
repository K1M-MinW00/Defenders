using Firebase;
using Firebase.Firestore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public sealed class FirestoreUserDataRepository : IUserDataRepository, IIdleRewardRepository
{
    private const int MaxAttempts = 3;

    private readonly FirebaseFirestore firestore;

    public FirestoreUserDataRepository()
        : this(FirebaseFirestore.DefaultInstance)
    {
    }

    public FirestoreUserDataRepository(FirebaseFirestore firestore)
    {
        this.firestore = firestore ?? throw new ArgumentNullException(nameof(firestore));
    }

    public async Task<UserDataLoadResult> LoadAsync(string userId)
    {
        DocumentReference userRef = GetUserDocument(userId);
        DocumentSnapshot snapshot = await ExecuteWithRetryAsync(
            () => userRef.GetSnapshotAsync(),
            "Load user data");

        if (!snapshot.Exists)
            return UserDataLoadResult.NotFound();

        Dictionary<string, object> storedFields = snapshot.ToDictionary();
        Dictionary<string, object> missingTimestampFields = new();

        if (!storedFields.ContainsKey(FirestoreDataContract.UserFields.CreatedAt))
            missingTimestampFields.Add(FirestoreDataContract.UserFields.CreatedAt, FieldValue.ServerTimestamp);

        if (!storedFields.ContainsKey(FirestoreDataContract.UserFields.UpdatedAt))
            missingTimestampFields.Add(FirestoreDataContract.UserFields.UpdatedAt, FieldValue.ServerTimestamp);

        if (missingTimestampFields.Count > 0)
        {
            await ExecuteWithRetryAsync(
                () => userRef.UpdateAsync(missingTimestampFields),
                "Repair user timestamps");
            snapshot = await ExecuteWithRetryAsync(
                () => userRef.GetSnapshotAsync(),
                "Reload repaired user data");
        }

        UserDataRoot data = snapshot.ConvertTo<UserDataRoot>();
        return UserDataLoadResult.Found(data);
    }

    public Task CreateAsync(string userId, UserDataRoot data)
    {
        ValidateData(data);

        return ExecuteWithRetryAsync(
            () => GetUserDocument(userId).SetAsync(BuildRootFields(data, includeCreatedAt: true)),
            "Create user data",
            allowRetry: false);
    }

    public Task SaveAllAsync(string userId, UserDataRoot data)
    {
        ValidateData(data);

        return ExecuteWithRetryAsync(
            () => GetUserDocument(userId).UpdateAsync(BuildRootFields(data, includeCreatedAt: false)),
            "Save all user data");
    }

    public Task SaveProfileAsync(string userId, UserProfileData profile) =>
        SaveSectionAsync(userId, FirestoreDataContract.UserFields.Profile, profile);

    public Task SaveResourcesAsync(string userId, UserResourceData resources) =>
        SaveSectionAsync(userId, FirestoreDataContract.UserFields.Resource, resources);

    public Task SaveProgressAsync(string userId, UserProgressData progress) =>
        SaveSectionAsync(userId, FirestoreDataContract.UserFields.Progress, progress);

    public Task SaveRosterAsync(string userId, UserRosterData roster) =>
        SaveSectionAsync(userId, FirestoreDataContract.UserFields.Roster, roster);

    public async Task<IdleRewardTransactionResult> ClaimAsync(string userId, IdleRewardConfigSO config)
    {
        if (string.IsNullOrWhiteSpace(userId) || config == null)
            return IdleRewardTransactionResult.Fail(IdleRewardClaimFailure.InvalidData);

        DocumentReference userReference = GetUserDocument(userId);
        DocumentReference serverTimeReference = firestore
            .Collection(FirestoreDataContract.ServerTimesCollection)
            .Document(userId);

        await ExecuteWithRetryAsync(
            () => serverTimeReference.SetAsync(new Dictionary<string, object>
            {
                { FirestoreDataContract.ServerTimestampField, FieldValue.ServerTimestamp },
            }, SetOptions.MergeAll),
            "Synchronize idle reward server time");

        return await ExecuteWithRetryAsync(() => firestore.RunTransactionAsync(async transaction =>
        {
            DocumentSnapshot userSnapshot = await transaction.GetSnapshotAsync(userReference);
            DocumentSnapshot serverTimeSnapshot = await transaction.GetSnapshotAsync(serverTimeReference);
            if (!userSnapshot.Exists || !serverTimeSnapshot.Exists ||
                !serverTimeSnapshot.TryGetValue(FirestoreDataContract.ServerTimestampField, out Timestamp serverTimestamp))
                return IdleRewardTransactionResult.Fail(IdleRewardClaimFailure.InvalidData);

            UserDataRoot currentData = userSnapshot.ConvertTo<UserDataRoot>();
            IdleRewardPreview preview = IdleRewardUseCase.BuildPreview(currentData, serverTimestamp.ToDateTime(), config);
            if (preview == null) return IdleRewardTransactionResult.Fail(IdleRewardClaimFailure.InvalidData);
            if (!preview.CanClaim) return IdleRewardTransactionResult.Fail(IdleRewardClaimFailure.NotReady);

            List<RewardData> rewards = preview.AccumulatedRewards.Select(entry => new RewardData
            {
                Type = entry.Type,
                Id = entry.ItemId,
                Amount = entry.TotalAmount,
            }).ToList();
            RewardGrantResult grant = RewardGrantCalculator.Calculate(currentData, rewards);
            if (!grant.Succeeded) return IdleRewardTransactionResult.Fail(IdleRewardClaimFailure.InvalidData);

            UserIdleRewardData nextIdleReward = new() { LastClaimAt = serverTimestamp };
            transaction.Update(userReference, new Dictionary<string, object>
            {
                { FirestoreDataContract.UserFields.Resource, grant.Resources },
                { FirestoreDataContract.UserFields.Inventory, grant.Inventory },
                { FirestoreDataContract.UserFields.IdleReward, nextIdleReward },
                { FirestoreDataContract.UserFields.UpdatedAt, FieldValue.ServerTimestamp },
            });

            return IdleRewardTransactionResult.Success(grant.Resources, grant.Inventory, nextIdleReward);
        }), "Claim idle reward transaction");
    }

    public Task SaveSectionsAsync(string userId, UserDataUpdate update)
    {
        if (update == null)
            throw new ArgumentNullException(nameof(update));

        Dictionary<string, object> fields = new()
        {
            { FirestoreDataContract.UserFields.UpdatedAt, FieldValue.ServerTimestamp },
        };

        AddSection(fields, FirestoreDataContract.UserFields.Profile, update.Profile);
        AddSection(fields, FirestoreDataContract.UserFields.Resource, update.Resources);
        AddSection(fields, FirestoreDataContract.UserFields.Progress, update.Progress);
        AddSection(fields, FirestoreDataContract.UserFields.Roster, update.Roster);
        AddSection(fields, FirestoreDataContract.UserFields.Inventory, update.Inventory);
        AddSection(fields, FirestoreDataContract.UserFields.Gacha, update.Gacha);
        AddSection(fields, FirestoreDataContract.UserFields.Ad, update.Ad);
        AddSection(fields, FirestoreDataContract.UserFields.Shop, update.Shop);
        AddSection(fields, FirestoreDataContract.UserFields.Lab, update.Lab);
        AddSection(fields, FirestoreDataContract.UserFields.IdleReward, update.IdleReward);

        if (fields.Count == 1)
            throw new ArgumentException("At least one user data section is required.", nameof(update));

        return ExecuteWithRetryAsync(
            () => GetUserDocument(userId).UpdateAsync(fields),
            "Save user data sections");
    }

    private Task SaveSectionAsync<T>(string userId, string fieldName, T value)
    {
        if (value == null)
            throw new ArgumentNullException(nameof(value));

        return ExecuteWithRetryAsync(
            () => GetUserDocument(userId).UpdateAsync(new Dictionary<string, object>
            {
                { fieldName, value },
                { FirestoreDataContract.UserFields.UpdatedAt, FieldValue.ServerTimestamp },
            }),
            $"Save user data section '{fieldName}'");
    }

    private DocumentReference GetUserDocument(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User ID is null or empty.", nameof(userId));

        return firestore.Collection(FirestoreDataContract.UsersCollection).Document(userId);
    }

    private static void AddSection<T>(Dictionary<string, object> fields, string fieldName, T value)
        where T : class
    {
        if (value != null)
            fields.Add(fieldName, value);
    }

    private static Dictionary<string, object> BuildRootFields(UserDataRoot data, bool includeCreatedAt)
    {
        Dictionary<string, object> fields = new()
        {
            { FirestoreDataContract.UserFields.SchemaVersion, data.SchemaVersion },
            { FirestoreDataContract.UserFields.Profile, data.Profile },
            { FirestoreDataContract.UserFields.Resource, data.Resource },
            { FirestoreDataContract.UserFields.Roster, data.Roster },
            { FirestoreDataContract.UserFields.Progress, data.Progress },
            { FirestoreDataContract.UserFields.Inventory, data.Inventory },
            { FirestoreDataContract.UserFields.Gacha, data.Gacha },
            { FirestoreDataContract.UserFields.Ad, data.Ad },
            { FirestoreDataContract.UserFields.Shop, data.Shop },
            { FirestoreDataContract.UserFields.Lab, data.Lab },
            { FirestoreDataContract.UserFields.IdleReward, data.IdleReward },
            { FirestoreDataContract.UserFields.UpdatedAt, FieldValue.ServerTimestamp },
        };

        if (includeCreatedAt)
            fields.Add(FirestoreDataContract.UserFields.CreatedAt, FieldValue.ServerTimestamp);

        return fields;
    }

    private static void ValidateData(UserDataRoot data)
    {
        if (data == null)
            throw new ArgumentNullException(nameof(data));
    }

    private static async Task ExecuteWithRetryAsync(
        Func<Task> operation,
        string operationName,
        bool allowRetry = true)
    {
        await ExecuteWithRetryAsync(async () =>
        {
            await operation();
            return true;
        }, operationName, allowRetry);
    }

    private static async Task<T> ExecuteWithRetryAsync<T>(
        Func<Task<T>> operation,
        string operationName,
        bool allowRetry = true)
    {
        int maxAttempts = allowRetry ? MaxAttempts : 1;

        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                return await operation();
            }
            catch (FirebaseException exception)
                when (attempt < maxAttempts && IsTransient(exception.ErrorCode))
            {
                int delayMilliseconds = 250 * attempt * attempt;
                Debug.LogWarning(
                    $"[FirestoreUserDataRepository] {operationName} transient failure " +
                    $"(code: {exception.ErrorCode}, attempt: {attempt}/{maxAttempts}). Retrying...");
                await Task.Delay(delayMilliseconds);
            }
            catch (Exception exception)
            {
                Debug.LogError($"[FirestoreUserDataRepository] {operationName} failed: {exception}");
                throw;
            }
        }

        throw new InvalidOperationException($"{operationName} retry loop ended unexpectedly.");
    }

    private static bool IsTransient(int errorCode)
    {
        // Firestore uses gRPC canonical status codes.
        return errorCode == 4 ||  // DeadlineExceeded
               errorCode == 8 ||  // ResourceExhausted
               errorCode == 10 || // Aborted
               errorCode == 13 || // Internal
               errorCode == 14;   // Unavailable
    }
}
