using Firebase.Firestore;

[FirestoreData]
public class UserGachaData
{
    [FirestoreProperty] public int NormalPity { get; set; } = new();
    [FirestoreProperty] public int SpecialPity { get; set; } = new();
}