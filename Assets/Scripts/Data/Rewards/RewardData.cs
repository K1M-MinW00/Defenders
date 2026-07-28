using Firebase.Firestore;

[FirestoreData]
public class RewardData
{
    [FirestoreProperty] public RewardType Type { get; set; }
    [FirestoreProperty] public string Id { get; set; }
    [FirestoreProperty] public int Amount { get; set; }
}
