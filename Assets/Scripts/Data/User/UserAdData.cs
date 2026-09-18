using Firebase.Firestore;

[FirestoreData]
public class UserAdData
{
    [FirestoreProperty] public int FuelAdWatchCount { get; set; }
    [FirestoreProperty] public int GemAdWatchCount { get; set; }
    [FirestoreProperty] public string AdWatchDate { get; set; }
}
public enum DailyAdType
{
    Fuel,
    Gem
}