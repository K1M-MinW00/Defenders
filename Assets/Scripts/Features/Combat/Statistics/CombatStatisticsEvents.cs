using System;

public static class CombatStatisticsEvents
{
    public static event Action<UnitController, float> DamageDealt;
    public static event Action<UnitController, float> HealingDone;
    public static event Action<UnitController, float> DamageTaken;

    public static void ReportDamageDealt(UnitController unit, float amount)
    {
        if (unit != null && amount > 0f)
            DamageDealt?.Invoke(unit, amount);
    }

    public static void ReportHealingDone(UnitController unit, float amount)
    {
        if (unit != null && amount > 0f)
            HealingDone?.Invoke(unit, amount);
    }

    public static void ReportDamageTaken(UnitController unit, float amount)
    {
        if (unit != null && amount > 0f)
            DamageTaken?.Invoke(unit, amount);
    }
}
