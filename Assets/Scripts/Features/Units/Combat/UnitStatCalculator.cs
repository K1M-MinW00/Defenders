public static class UnitStatCalculator
{
    public static UnitStats Calculate(UnitDataSO unitData, UserUnitData userUnit)
    {
        UnitStats stats = unitData.GetOriginStats(userUnit.Level);

        UnitStatModifier modifier = new();

        ApplyPromotion(modifier, unitData, userUnit.Promotion);

        ApplyLimitBreak(modifier, unitData, userUnit.LimitBreak);

        // ApplyEquipment(modifier, userUnit);

        ApplyModifier(ref stats, modifier);

        return stats;
    }

    private static void ApplyPromotion(UnitStatModifier modifier, UnitDataSO unitData, int promotion)
    {
        if (promotion >= 1)
            modifier.AttackPercent += 10;

        if (promotion >= 2)
            modifier.MaxHpPercent += 10;
    }

    private static void ApplyLimitBreak(UnitStatModifier modifier,UnitDataSO unitData,int limitBreak)
    {
        if (limitBreak <= 0)
            return;

        for (int i = 0; i < limitBreak; i++)
        {
            LimitBreakData effect = unitData.limitBreaks[i];

            AddPercentModifier(modifier, effect.statType, effect.value);
        }
    }

    private static void AddPercentModifier(UnitStatModifier modifier,StatType statType,float value)
    {
        switch (statType)
        {
            case StatType.Attack:
                modifier.AttackPercent += value;
                break;

            case StatType.MaxHp:
                modifier.MaxHpPercent += value;
                break;

            case StatType.AttackPerSec:
                modifier.AttackPerSecPercent += value;
                break;

            case StatType.DetectRange:
                modifier.DetectRangePercent += value;
                break;

            case StatType.CritChance:
                modifier.CritChancePercent += value;
                break;

            case StatType.CritDamage:
                modifier.CritDamagePercent += value;
                break;

            case StatType.EnergyRecovery:
                modifier.EnergyRecoveryPercent += value;
                break;
        }
    }
    private static void ApplyModifier(ref UnitStats stats, UnitStatModifier modifier)
    {
        stats.Attack = (stats.Attack + modifier.AttackFlat) * (1f + modifier.AttackPercent / 100f);

        stats.MaxHp = (stats.MaxHp + modifier.MaxHpFlat) * (1f + modifier.MaxHpPercent / 100f);

        stats.AttackPerSec = (stats.AttackPerSec + modifier.AttackPerSecFlat) * (1f + modifier.AttackPerSecPercent / 100f);

        stats.DetectRange = (stats.DetectRange + modifier.DetectRangeFlat) * (1f + modifier.DetectRangePercent / 100f);

        stats.CritChance = (stats.CritChance + modifier.CritChanceFlat) * (1f + modifier.CritChancePercent / 100f);

        stats.CritDamage = (stats.CritDamage + modifier.CritDamageFlat) * (1f + modifier.CritDamagePercent / 100f);

        stats.EnergyRecovery = (stats.EnergyRecovery + modifier.EnergyRecoveryFlat) * (1f + modifier.EnergyRecoveryPercent / 100f);
    }
}