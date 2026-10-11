using UnityEngine;

public class StageUnitInitData
{
    public UnitDataSO UnitData { get; }
    public UserUnitData UserData { get; }
    public int InitialStar { get; }
    public UserInventoryData Inventory { get; }
    public StageUnitInitData(UnitDataSO unitData, UserUnitData userData, int initialStar = 1, UserInventoryData inventory = null)
    {
        UnitData = unitData;
        UserData = userData;
        InitialStar = Mathf.Clamp(initialStar, 1, 4);
        Inventory = inventory;
    }
}
