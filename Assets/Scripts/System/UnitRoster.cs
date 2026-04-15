using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UnitRoster : MonoBehaviour
{
    private readonly List<UnitController> units = new();
    public IReadOnlyList<UnitController> Units => units;

    public event Action OnRosterChanged;
    public event Action OnAliveCountChanged;

    public void Register(UnitController unit)
    {
        if (unit == null || units.Contains(unit))
            return;

        units.Add(unit);

        unit.Health.OnDead += HandleUnitDead;
        OnRosterChanged?.Invoke();
    }

    public void Unregister(UnitController unit)
    {
        if (unit == null) return;

        if (!units.Remove(unit))
            return;

        unit.Health.OnDead -= HandleUnitDead;
        OnRosterChanged?.Invoke();
    }

    private void HandleUnitDead(UnitController runtime)
    {
        OnAliveCountChanged?.Invoke();
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
            .OrderBy(u => u.CurrentHp)
            .FirstOrDefault();
    }

    public List<UnitController> GetLowestHpAliveUnits(int count)
    {
        return units
            .Where(u => u != null && !u.IsDead)
            .OrderBy(u => u.CurrentHp)
            .Take(count)
            .ToList();
    }
}
