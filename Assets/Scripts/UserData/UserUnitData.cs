using Firebase.Firestore;

[FirestoreData]
public class UserUnitData
{
    [FirestoreProperty] public string UnitId { get; set; } = string.Empty;
    [FirestoreProperty] public int Level { get; set; } = 1;
    [FirestoreProperty] public int Promotion { get; set; } = 1;
    [FirestoreProperty] public int LimitBreak { get; set; } = 0;

    // TODO : 장비
}