using System;
using System.Collections.Generic;

public enum CombatStatisticType
{
    DamageDealt,
    HealingDone,
    DamageTaken
}

public sealed class UnitCombatStatistics
{
    public UnitController Unit { get; }
    public string SortKey { get; }
    public float DamageDealt { get; private set; }
    public float HealingDone { get; private set; }
    public float DamageTaken { get; private set; }

    public UnitCombatStatistics(UnitController unit)
    {
        Unit = unit;
        // UnitController.UnitId depends on runtime data that may not exist yet, or may
        // already have been released when a pooled unit leaves combat.
        SortKey = unit != null
            ? unit.UnitData?.unitId ?? unit.name ?? string.Empty
            : string.Empty;
    }

    public void Add(CombatStatisticType type, float amount)
    {
        switch (type)
        {
            case CombatStatisticType.DamageDealt: DamageDealt += amount; break;
            case CombatStatisticType.HealingDone: HealingDone += amount; break;
            case CombatStatisticType.DamageTaken: DamageTaken += amount; break;
        }
    }

    public float GetValue(CombatStatisticType type) => type switch
    {
        CombatStatisticType.DamageDealt => DamageDealt,
        CombatStatisticType.HealingDone => HealingDone,
        CombatStatisticType.DamageTaken => DamageTaken,
        _ => 0f
    };
}

public sealed class CombatStatisticsTracker : IDisposable
{
    private readonly Dictionary<UnitController, UnitCombatStatistics> statistics = new();
    private readonly List<UnitCombatStatistics> ranking = new();
    private UnitRoster roster;
    private bool isCombatActive;

    public event Action Changed;

    public void Initialize(UnitRoster unitRoster)
    {
        Dispose();
        roster = unitRoster;
        CombatStatisticsEvents.DamageDealt += HandleDamageDealt;
        CombatStatisticsEvents.HealingDone += HandleHealingDone;
        CombatStatisticsEvents.DamageTaken += HandleDamageTaken;
    }

    public void BeginCombat()
    {
        statistics.Clear();
        isCombatActive = true;
        if (roster != null)
        {
            foreach (UnitController unit in roster.Units)
            {
                if (unit != null)
                    statistics[unit] = new UnitCombatStatistics(unit);
            }
        }
        Changed?.Invoke();
    }

    public void EndCombat() => isCombatActive = false;

    public IReadOnlyList<UnitCombatStatistics> GetTop(CombatStatisticType type, int count)
    {
        ranking.Clear();
        foreach (UnitCombatStatistics entry in statistics.Values)
        {
            if (entry.Unit != null)
                ranking.Add(entry);
        }

        ranking.Sort((left, right) =>
        {
            int valueOrder = right.GetValue(type).CompareTo(left.GetValue(type));
            if (valueOrder != 0)
                return valueOrder;
            return string.Compare(left.SortKey, right.SortKey, StringComparison.Ordinal);
        });

        if (ranking.Count > count)
            ranking.RemoveRange(count, ranking.Count - count);
        return ranking;
    }

    public float GetMaximum(CombatStatisticType type)
    {
        float maximum = 0f;
        foreach (UnitCombatStatistics entry in statistics.Values)
            maximum = Math.Max(maximum, entry.GetValue(type));
        return maximum;
    }

    public void Dispose()
    {
        CombatStatisticsEvents.DamageDealt -= HandleDamageDealt;
        CombatStatisticsEvents.HealingDone -= HandleHealingDone;
        CombatStatisticsEvents.DamageTaken -= HandleDamageTaken;
        statistics.Clear();
        ranking.Clear();
        roster = null;
        isCombatActive = false;
    }

    private void HandleDamageDealt(UnitController unit, float amount) => Add(unit, CombatStatisticType.DamageDealt, amount);
    private void HandleHealingDone(UnitController unit, float amount) => Add(unit, CombatStatisticType.HealingDone, amount);
    private void HandleDamageTaken(UnitController unit, float amount) => Add(unit, CombatStatisticType.DamageTaken, amount);

    private void Add(UnitController unit, CombatStatisticType type, float amount)
    {
        if (!isCombatActive || unit == null || amount <= 0f)
            return;
        if (!statistics.TryGetValue(unit, out UnitCombatStatistics entry))
        {
            entry = new UnitCombatStatistics(unit);
            statistics.Add(unit, entry);
        }
        entry.Add(type, amount);
        Changed?.Invoke();
    }
}
