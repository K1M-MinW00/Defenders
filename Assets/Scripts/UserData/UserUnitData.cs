using Firebase.Firestore;
using System;

[FirestoreData]
[Serializable]
public class UserUnitData
{
    [FirestoreProperty] public string UnitId { get; set; }
    [FirestoreProperty] public int Level { get; set; } = 1;

    public UserUnitData() { }

    public UserUnitData(string unitId, int level = 1)
    {
        UnitId = unitId;
        Level = level;
    }
}