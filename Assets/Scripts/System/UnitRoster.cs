using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UnitRoster : MonoBehaviour
{
    private readonly List<UnitController> units = new();
    private readonly Dictionary<UnitController, float> lastKnownHp = new();

    public IReadOnlyList<UnitController> Units => units;

    public event Action OnRosterChanged;
    public event Action OnAliveCountChanged;

    [SerializeField] private float combatAlertCooldown = 0.2f;
    private float lastCombatAlertTime = -999f;

    public void Register(UnitController unit)
    {
        if (unit == null || units.Contains(unit))
            return;

        units.Add(unit);

        unit.Health.OnDead += HandleUnitDead;
        unit.Health.OnHpChanged += HandleUnitHpChanged;

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

        lastKnownHp.Remove(unit);

        OnRosterChanged?.Invoke();
    }

    private void HandleUnitDead(UnitController runtime)
    {
        OnAliveCountChanged?.Invoke();
    }

    private void HandleUnitHpChanged(UnitController unit, float currentHp, float maxHp)
    {
        if (unit == null || unit.IsDead)
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
        UnitController best = null;
        float bestD = float.PositiveInfinity;

        for (int i = units.Count - 1; i >= 0; --i)
        {
            UnitController u = units[i];

            if (u == null || u.IsDead)
                continue;

            float d = (u.transform.position - from).sqrMagnitude;

            if (d < bestD)
            {
                bestD = d;
                best = u;
            }
        }

        return best;
    }

    public UnitController FindAny(UnitCode unitCode, int star, UnitController exclude = null)
    {
        for (int i = 0; i < units.Count; i++)
        {
            UnitController unit = units[i];

            if (unit == null || unit == exclude)
                continue;

            if (unit.UnitCode == unitCode && unit.Star == star)
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
        return units
            .Where(u => u != null && !u.IsDead)
            .OrderBy(u => u.Health.CurrentHp)
            .FirstOrDefault();
    }

    public List<UnitController> GetLowestHpAliveUnits(int count)
    {
        return units
            .Where(u => u != null && !u.IsDead)
            .OrderBy(u => u.Health.CurrentHp)
            .Take(count)
            .ToList();
    }
}