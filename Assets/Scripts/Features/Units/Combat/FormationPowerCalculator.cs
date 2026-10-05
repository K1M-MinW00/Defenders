using System;
using System.Collections.Generic;

public static class FormationPowerCalculator
{
    private const float DisplayScale = 10f;
    private const float BaselineRange = 3f;
    private const float RangeWeight = 0.03f;
    private const float MaximumRangeBonus = 0.2f;
    private const float EnergyWeight = 0.01f;
    private const float MaximumEnergyBonus = 0.15f;

    public static int Calculate(UserRosterData roster, IUnitCatalog catalog)
    {
        if (roster?.SelectedUnitIds == null || roster.OwnedUnits == null || catalog == null)
            return 0;

        Dictionary<string, UserUnitData> ownedById = new(StringComparer.Ordinal);
        foreach (UserUnitData unit in roster.OwnedUnits)
        {
            if (unit != null && !string.IsNullOrWhiteSpace(unit.UnitId))
                ownedById[unit.UnitId] = unit;
        }

        double total = 0d;
        HashSet<string> counted = new(StringComparer.Ordinal);
        foreach (string unitId in roster.SelectedUnitIds)
        {
            if (string.IsNullOrWhiteSpace(unitId) || !counted.Add(unitId) ||
                !ownedById.TryGetValue(unitId, out UserUnitData userUnit))
            {
                continue;
            }

            UnitDataSO definition = catalog.Get(unitId);
            if (definition == null)
                continue;

            total += CalculateUnit(definition, userUnit);
            if (total >= int.MaxValue)
                return int.MaxValue;
        }

        return Math.Clamp((int)Math.Round(total), 0, int.MaxValue);
    }

    public static int CalculateUnit(UnitDataSO definition, UserUnitData userUnit)
    {
        if (definition == null || userUnit == null)
            return 0;

        UnitStats stats = UnitStatCalculator.Calculate(definition, userUnit);
        float hp = Math.Max(1f, stats.MaxHp);
        float attack = Math.Max(0f, stats.Attack);
        float attacksPerSecond = Math.Max(0.01f, stats.AttackPerSec);
        float criticalChance = Math.Clamp(stats.CritChance, 0f, 1f);
        float criticalDamage = Math.Max(1f, stats.CritDamage);
        float expectedCriticalMultiplier = 1f + criticalChance * (criticalDamage - 1f);
        float effectiveDps = Math.Max(0.1f, attack * attacksPerSecond * expectedCriticalMultiplier);

        float rangeBonus = Math.Clamp(
            (stats.DetectRange - BaselineRange) * RangeWeight,
            0f,
            MaximumRangeBonus);
        float energyBonus = Math.Clamp(
            stats.EnergyRecovery * EnergyWeight,
            0f,
            MaximumEnergyBonus);

        double power = Math.Sqrt(hp * effectiveDps) * DisplayScale *
                       (1f + rangeBonus) * (1f + energyBonus);
        return power >= int.MaxValue ? int.MaxValue : Math.Max(0, (int)Math.Round(power));
    }
}
