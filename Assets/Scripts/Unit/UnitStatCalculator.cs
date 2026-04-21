//public static class UnitStatCalculator
//{
//    public static UnitStats CalculateRuntimeBase(UnitStats origin, UnitDataSO data, int star)
//    {
//        return data.ApplyStar(origin, star);
//    }

//    public static UnitStats CalculateFinal(UnitStats baseStats, UnitBuffController buff)
//    {
//        UnitStats result = baseStats;

//        result.Attack = Apply(buff, BuffStatType.Attack, baseStats.Attack);
//        result.MaxHp = Apply(buff, BuffStatType.MaxHp, baseStats.MaxHp);
//        result.AttackPerSec = Apply(buff, BuffStatType.AttackPerSec, baseStats.AttackPerSec);
//        result.DetectRange = Apply(buff, BuffStatType.DetectRange, baseStats.DetectRange);

//        return result;
//    }

//    private static float Apply(UnitBuffController buff, BuffStatType statType, float baseValue)
//    {
//        float add = buff.GetAdditive(statType);
//        float mul = buff.GetMultiplier(statType);

//        return (baseValue + add) * mul;
//    }
//}