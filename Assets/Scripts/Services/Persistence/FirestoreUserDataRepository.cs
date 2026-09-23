using Firebase;
using Firebase.Firestore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public sealed class FirestoreUserDataRepository : IUserDataRepository
{
    private const string UsersCollection = "users";
    private const string CreatedAtField = "CreatedAt";
    private const string UpdatedAtField = "UpdatedAt";
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

        if (!storedFields.ContainsKey(CreatedAtField))
            missingTimestampFields.Add(CreatedAtField, FieldValue.ServerTimestamp);

        if (!storedFields.ContainsKey(UpdatedAtField))
            missingTimestampFields.Add(UpdatedAtField, FieldValue.ServerTimestamp);

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
        SaveSectionAsync(userId, "Profile", profile);

    public Task SaveResourcesAsync(string userId, UserResourceData resources) =>
        SaveSectionAsync(userId, "Resource", resources);

    public Task SaveProgressAsync(string userId, UserProgressData progress) =>
        SaveSectionAsync(userId, "Progress", progress);

    public Task SaveRosterAsync(string userId, UserRosterData roster) =>
        SaveSectionAsync(userId, "Roster", roster);

    public Task SaveInventoryAsync(string userId, UserInventoryData inventory) =>
        SaveSectionAsync(userId, "Inventory", inventory);

    public Task SaveGachaAsync(string userId, UserGachaData gacha) =>
        SaveSectionAsync(userId, "Gacha", gacha);

    public Task SaveAdAsync(string userId, UserAdData ad) =>
        SaveSectionAsync(userId, "Ad", ad);

    public Task SaveSectionsAsync(string userId, UserDataUpdate update)
    {
        if (update == null)
            throw new ArgumentNullException(nameof(update));

        Dictionary<string, object> fields = new()
        {
            { UpdatedAtField, FieldValue.ServerTimestamp },
        };

        AddSection(fields, "Profile", update.Profile);
        AddSection(fields, "Resource", update.Resources);
        AddSection(fields, "Progress", update.Progress);
        AddSection(fields, "Roster", update.Roster);
        AddSection(fields, "Inventory", update.Inventory);
        AddSection(fields, "Gacha", update.Gacha);
        AddSection(fields, "Ad", update.Ad);

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
                { UpdatedAtField, FieldValue.ServerTimestamp },
            }),
            $"Save user data section '{fieldName}'");
    }

    private DocumentReference GetUserDocument(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User ID is null or empty.", nameof(userId));

        return firestore.Collection(UsersCollection).Document(userId);
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
            { "SchemaVersion", data.SchemaVersion },
            { "Profile", data.Profile },
            { "Resource", data.Resource },
            { "Roster", data.Roster },
            { "Progress", data.Progress },
            { "Inventory", data.Inventory },
            { "Gacha", data.Gacha },
            { "Ad", data.Ad },
            { UpdatedAtField, FieldValue.ServerTimestamp },
        };

        if (includeCreatedAt)
            fields.Add(CreatedAtField, FieldValue.ServerTimestamp);

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
