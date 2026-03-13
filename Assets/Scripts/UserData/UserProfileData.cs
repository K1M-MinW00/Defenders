
using Firebase.Firestore;

[FirestoreData]
public class UserProfileData
{
    [FirestoreProperty] public string UserId { get; set; } = string.Empty;
    [FirestoreProperty] public int Level { get; set; } = 1;
    [FirestoreProperty] public int Exp { get; set; } = 0;
}