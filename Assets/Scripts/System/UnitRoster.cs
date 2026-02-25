using System;
using System.Collections.Generic;
using UnityEngine;

public class UnitRoster : MonoBehaviour
{
    private readonly List<UnitInstance> units = new();

    public IReadOnlyList<UnitInstance> Units => units;

    public event Action<UnitInstance> OnUnitAdded;
    public event Action<UnitInstance> OnUnitRemoved;

    public void Register(UnitInstance unit)
    {
        if (unit == null) return;
        if (units.Contains(unit)) return;

        units.Add(unit);
        OnUnitAdded?.Invoke(unit);
    }

    public void Unregister(UnitInstance unit)
    {
        if (unit == null) return;

        if (units.Remove(unit))
        {
            OnUnitRemoved?.Invoke(unit);
        }
    }

    public int CountBy(string unitId, int star)
    {
        int count = 0;
        for (int i = 0; i < units.Count; i++)
        {
            var u = units[i];
            if (u == null) continue;
            if (u.Data == null) continue;

            if (u.Data.unitId == unitId && u.Star == star)
                count++;
        }
        return count;
    }

    public UnitInstance FindAny(string unitId, int star, UnitInstance exclude = null)
    {
        for (int i = 0; i < units.Count; i++)
        {
            var u = units[i];
            if (u == null || u == exclude) continue;
            if (u.Data == null) continue;

            if (u.Data.unitId == unitId && u.Star == star)
                return u;
        }
        return null;
    }

    public int CountAliveUnits()
    {
        int alive = 0;

        foreach (var u in units)
        {
            if (u != null && u.IsAlive)
                alive++;
        }

        return alive;
    }
    public void CleanupNulls()
    {
        units.RemoveAll(u => u == null);
    }
}
