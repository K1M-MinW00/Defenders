using Firebase.Firestore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public sealed class FirestoreUserDataRepository : IUserDataRepository
{
    private const string UsersCollection = "users";
    private const string CreatedAtField = "CreatedAt";
    private const string UpdatedAtField = "UpdatedAt";

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
        DocumentSnapshot snapshot = await userRef.GetSnapshotAsync();

        if (!snapshot.Exists)
            return UserDataLoadResult.NotFound();

        UserDataRoot data = snapshot.ConvertTo<UserDataRoot>();

        if (!snapshot.ToDictionary().ContainsKey(CreatedAtField))
        {
            await userRef.UpdateAsync(new Dictionary<string, object>
            {
                { CreatedAtField, FieldValue.ServerTimestamp },
                { UpdatedAtField, FieldValue.ServerTimestamp },
            });
        }

        return UserDataLoadResult.Found(data);
    }

    public Task CreateAsync(string userId, UserDataRoot data)
    {
        ValidateData(data);

        return GetUserDocument(userId).SetAsync(BuildRootFields(data, includeCreatedAt: true));
    }

    public Task SaveAllAsync(string userId, UserDataRoot data)
    {
        ValidateData(data);

        return GetUserDocument(userId).UpdateAsync(BuildRootFields(data, includeCreatedAt: false));
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

        return GetUserDocument(userId).UpdateAsync(fields);
    }

    private Task SaveSectionAsync<T>(string userId, string fieldName, T value)
    {
        if (value == null)
            throw new ArgumentNullException(nameof(value));

        return GetUserDocument(userId).UpdateAsync(new Dictionary<string, object>
        {
            { fieldName, value },
            { UpdatedAtField, FieldValue.ServerTimestamp },
        });
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
}
