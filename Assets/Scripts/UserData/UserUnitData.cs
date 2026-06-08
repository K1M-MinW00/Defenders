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
    public UserUnitData() { }

    public UserUnitData(string unitId, int level = 1)
    {
        UnitId = unitId;
        Level = level;
        Exp = 0;
    }
    public void AddExp(int amount)
    {
        if (amount <= 0)
            return;

        Exp += amount;

        ProcessLevelUp();

        UserDataManager.Instance.MarkDirty();
    }

    private void ProcessLevelUp()
    {
        while (CanLevelUp())
        {
            int requiredExp = UnitExpTable.GetRequiredExp(Level);

            Exp -= requiredExp;
            Level++;
        }
    }

    private bool CanLevelUp()
    {
        if (Level >= GetMaxLevel())
            return false;

        int requiredExp = UnitExpTable.GetRequiredExp(Level);

        return Exp >= requiredExp;
    }

    public int GetMaxLevel()
    {
        return 50;
    }
}