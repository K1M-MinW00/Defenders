public class UnitBuildData
{
    public UnitDataSO UnitData { get; private set; }
    public UserUnitData UserData { get; private set; }

    public UnitStats PersistentStats { get; private set; }

    public int Level => UserData.Level;


    public UnitBuildData(UnitDataSO unitData, UserUnitData userData, UnitStats persistentStats)
    {
        UnitData = unitData;
        UserData = userData;
        PersistentStats = persistentStats;
    }
}