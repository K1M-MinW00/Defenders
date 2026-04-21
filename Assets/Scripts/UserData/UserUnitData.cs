using Firebase.Firestore;
using System;

[FirestoreData]
[Serializable]
public class UserUnitData
{
    [FirestoreProperty] public int UnitCodeValue { get; set; }
    [FirestoreProperty] public int Level { get; set; } = 1;


    public UnitCode UnitCode
    {
        get => (UnitCode)UnitCodeValue;
        set => UnitCodeValue = (int)value;
    }

    public UserUnitData() { }

    public UserUnitData(UnitCode unitCode, int level = 1)
    {
        UnitCode = unitCode;
        Level = level;
    }
}