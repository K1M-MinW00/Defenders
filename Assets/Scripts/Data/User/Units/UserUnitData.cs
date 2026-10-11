using Firebase.Firestore;
using System;

[FirestoreData]
[Serializable]
public class UserUnitData
{
    [FirestoreProperty] public string UnitId { get; set; }
    [FirestoreProperty] public int Level { get; set; } = 1;
    [FirestoreProperty] public int Exp { get; set; } = 0;
    [FirestoreProperty] public int LimitBreak { get; set; } = 0;
    [FirestoreProperty] public int Promotion { get; set; } = 0;
    [FirestoreProperty] public int DuplicateCount { get; set; } = 0;
    [FirestoreProperty] public EquipmentLoadout Equipment { get; set; } = new();
    public UserUnitData() { }
}
