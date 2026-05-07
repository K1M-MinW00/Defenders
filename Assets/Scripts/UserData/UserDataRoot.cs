using Firebase.Firestore;

[FirestoreData]
public class UserDataRoot
{
    [FirestoreProperty] public UserProfileData Profile { get; set; } = new();
    [FirestoreProperty] public UserResourceData Resources { get; set; } = new();
    [FirestoreProperty] public UserRosterData Roster { get; set; } = new();
    [FirestoreProperty] public UserProgressData Progress { get; set; } = new();

    // public UsernventoryData Inventory;
}
