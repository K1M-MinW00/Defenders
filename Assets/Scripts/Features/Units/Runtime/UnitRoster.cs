using System;
using System.Collections.Generic;
using UnityEngine;

public class UnitRoster : MonoBehaviour
{
    private readonly List<UnitController> units = new();
    private readonly Dictionary<UnitController, float> lastKnownHp = new();

    public IReadOnlyList<UnitController> Units => units;
    public int RegisteredCount
    {
        get
        {
            int count = 0;
            for (int i = 0; i < units.Count; i++)
            {
                if (units[i] != null)
                    count++;
            }

            return count;
        }
    }

    public event Action OnRosterChanged;
    public event Action OnAliveCountChanged;
    public event Action<UnitController, DamageResult> OnUnitDamaged;

    [SerializeField] private float combatAlertCooldown = 0.2f;
    private float lastCombatAlertTime = -999f;

    public void Register(UnitController unit)
    {
        if (unit == null || units.Contains(unit))
            return;

        units.Add(unit);

        unit.Health.OnDead += HandleUnitDead;
        unit.Health.OnHpChanged += HandleUnitHpChanged;
        unit.Health.OnDamaged += HandleUnitDamaged;

        lastKnownHp[unit] = unit.Health.CurrentHp;

        OnRosterChanged?.Invoke();
    }

    public void Unregister(UnitController unit)
    {
        if (unit == null)
            return;

        if (!units.Remove(unit))
            return;

        unit.Health.OnDead -= HandleUnitDead;
        unit.Health.OnHpChanged -= HandleUnitHpChanged;
        unit.Health.OnDamaged -= HandleUnitDamaged;

        lastKnownHp.Remove(unit);

        OnRosterChanged?.Invoke();
    }

    private void HandleUnitDead(UnitController runtime)
    {
        OnAliveCountChanged?.Invoke();
    }

    private void HandleUnitDamaged(UnitController unit, DamageResult result)
    {
        OnUnitDamaged?.Invoke(unit, result);
    }

    private void HandleUnitHpChanged(UnitController unit, float currentHp, float maxHp)
    {
        if (unit == null)
            return;

        if (!lastKnownHp.TryGetValue(unit, out float prevHp))
        {
            lastKnownHp[unit] = currentHp;
            return;
        }

        bool tookDamage = currentHp < prevHp;
        lastKnownHp[unit] = currentHp;

        if (!tookDamage)
            return;

        BroadcastCombatAlert(unit);
    }

    private void BroadcastCombatAlert(UnitController damagedUnit)
    {
        if (Time.time < lastCombatAlertTime + combatAlertCooldown)
            return;

        lastCombatAlertTime = Time.time;

        for (int i = 0; i < units.Count; i++)
        {
            UnitController unit = units[i];

            if (unit == null || unit.IsDead)
                continue;

            unit.ReceiveCombatAlert();
        }
    }

    public UnitController FindClosestAlive(Vector3 from)
    {
        return CombatTargetSelector.FindClosest(units, from);
    }

    public UnitController FindAny(string unitId, int star, UnitController exclude = null)
    {
        for (int i = 0; i < units.Count; i++)
        {
            UnitController unit = units[i];

            if (unit == null || unit == exclude)
                continue;

            if (unit.UnitId == unitId && unit.Star == star)
                return unit;
        }

        return null;
    }

    public int CountAliveUnits()
    {
        int aliveCount = 0;

        foreach (UnitController unit in units)
        {
            if (unit == null || unit.IsDead)
                continue;

            aliveCount++;
        }

        return aliveCount;
    }

    public UnitController GetLowestHpAliveUnit()
    {
        return CombatTargetSelector.FindLowestHealth(units);
    }

    private void OnDestroy()
    {
        for (int i = units.Count - 1; i >= 0; i--)
        {
            UnitController unit = units[i];
            if (unit == null || unit.Health == null)
                continue;

            unit.Health.OnDead -= HandleUnitDead;
            unit.Health.OnHpChanged -= HandleUnitHpChanged;
            unit.Health.OnDamaged -= HandleUnitDamaged;
        }

        units.Clear();
        lastKnownHp.Clear();
    }
}
