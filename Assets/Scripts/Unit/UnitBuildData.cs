public class UnitBuildData
{
    public UnitDataSO UnitData { get; private set; }
    public UserUnitData UserData { get; private set; }

    public UnitStats PersistentStats { get; private set; }

    public int Level => UserData.Level;
    public int Promotion => UserData.Promotion;
    public int LimitBreak => UserData.LimitBreak;

    public bool IsPassiveUnlocked => UnitData.PassiveSkill != null &&
                                     UnitData.PassiveSkill.IsUnlocked(Promotion);

    public bool IsActiveUnlocked => UnitData.ActiveSkill != null &&
                                    UnitData.ActiveSkill.IsUnlocked(Promotion);

    public UnitBuildData(UnitDataSO unitData, UserUnitData userData, UnitStats persistentStats)
    {
        UnitData = unitData;
        UserData = userData;
        PersistentStats = persistentStats;
    }
}