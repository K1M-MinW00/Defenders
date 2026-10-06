using Firebase.Firestore;

[FirestoreData]
public sealed class UserIdleRewardData
{
    [FirestoreProperty] public Timestamp LastClaimAt { get; set; }
}
