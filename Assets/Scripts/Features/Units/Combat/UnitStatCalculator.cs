public static class UnitStatCalculator
{
    public static UnitStats Calculate(UnitDataSO unitData, UserUnitData userUnit)
    {
        UnitStats stats = unitData.GetOriginStats(userUnit.Level);

        ApplyLimitBreak(ref stats, unitData, userUnit.LimitBreak);
        // TODO : 진급, 장비 추가
        return stats;
    }

    private static void ApplyLimitBreak(ref UnitStats stats, UnitDataSO unitData, int limitBreak)
    {
        if (limitBreak <= 0)
            return;

        for (int i = 0; i < limitBreak; i++)
        {
            LimitBreakData effect = unitData.limitBreaks[i];
            ApplyPercentModifier(ref stats, effect.statType, effect.value);
        }
    }

    private static void ApplyPercentModifier(ref UnitStats stats, StatType statType, float value)
    {
        float multiplier = 1f + value / 100f;

        switch (statType)
        {
            case StatType.Attack:
                stats.Attack *= multiplier;
                break;
            case StatType.MaxHp:
                stats.MaxHp *= multiplier;
                break;
            case StatType.AttackPerSec:
                stats.AttackPerSec *= multiplier;
                break;
            case StatType.DetectRange:
                stats.DetectRange *= multiplier;
                break;
            case StatType.CritChance:
                stats.CritChance *= multiplier;
                break;
            case StatType.CritDamage:
                stats.CritDamage *= multiplier;
                break;
            case StatType.EnergyRecovery:
                stats.EnergyRecovery *= multiplier;
                break;
        }
    }
}