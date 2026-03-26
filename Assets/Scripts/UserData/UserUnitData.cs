using Firebase.Firestore;

[FirestoreData]
public class UserUnitData
{
    [FirestoreProperty] public int UnitCodeValue { get; set; }
    [FirestoreProperty] public int Level { get; set; } = 1;
    [FirestoreProperty] public int Promotion { get; set; } = 0;
    [FirestoreProperty] public int LimitBreak { get; set; } = 0;

    // TODO : 장비

    public UnitCode UnitCode
    {
        get => (UnitCode)UnitCodeValue;
        set => UnitCodeValue = (int)value;
    }

    public UserUnitData() { }

    public UserUnitData(UnitCode unitCode)
    {
        UnitCode = unitCode;
        Level = 1;
        Promotion = 1;
        LimitBreak = 0;
    }
}