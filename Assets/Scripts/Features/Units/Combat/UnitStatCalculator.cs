public static class UnitStatCalculator
{
    public static UnitStats Calculate(UnitDataSO unitData, UserUnitData userUnit)
    {
        UnitStats stats = unitData.GetOriginStats(userUnit.Level);

        // TODO : 진급, 한계돌파, 장비 추가
        return stats;
    }

    // 진급 
}