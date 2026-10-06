using Firebase.Firestore;

[FirestoreData]
public class UserDataRoot
{
    [FirestoreProperty] public int SchemaVersion { get; set; }
    [FirestoreProperty] public Timestamp CreatedAt { get; set; }
    [FirestoreProperty] public Timestamp UpdatedAt { get; set; }
    [FirestoreProperty] public UserProfileData Profile { get; set; } = new();
    [FirestoreProperty] public UserResourceData Resource { get; set; } = new();
    [FirestoreProperty] public UserRosterData Roster { get; set; } = new();
    [FirestoreProperty] public UserProgressData Progress { get; set; } = new();
    [FirestoreProperty] public UserInventoryData Inventory {  get; set; } = new();
    [FirestoreProperty] public UserGachaData Gacha { get; set; } = new();
    [FirestoreProperty] public UserAdData Ad { get; set; } = new();
    [FirestoreProperty] public UserShopData Shop { get; set; } = new();
    [FirestoreProperty] public UserLabData Lab { get; set; } = new();
    [FirestoreProperty] public UserIdleRewardData IdleReward { get; set; } = new();
}
