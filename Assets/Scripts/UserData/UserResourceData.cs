using Firebase.Firestore;

[FirestoreData]
public class UserResourceData
{
    [FirestoreProperty] public int Gold { get; set; }
    [FirestoreProperty] public int Gem { get; set; }
    [FirestoreProperty] public int Fuel { get; set; }
    [FirestoreProperty] public long LastFuelUpdateTime { get; set; }
}