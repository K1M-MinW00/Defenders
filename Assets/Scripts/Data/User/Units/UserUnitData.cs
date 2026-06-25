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
    public bool CanReceive => LimitBreak + DuplicateCount < 5;
    public UserUnitData() { }

    public void AddExp(int amount)
    {
        if(Level >= 50)
        {
            Exp = 0;
            return;
        }    

        if (amount <= 0)
            return;

        Exp += amount;

        ProcessLevelUp();

        UserDataManager.Instance.MarkDirty();
    }

    private void ProcessLevelUp()
    {
        while (Level < 50)
        {
            int requiredExp = UnitExpTable.GetRequiredExp(Level);

            if (Exp < requiredExp)
                break;

            Exp -= requiredExp;
            Level++;
        }

        if(Level >= 50)
        {
            Level = 50;
            Exp = 0;
        }
    }
}