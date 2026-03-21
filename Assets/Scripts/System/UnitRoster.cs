using System;
using System.Collections.Generic;
using UnityEngine;

public class UnitRoster : MonoBehaviour
{
    private readonly List<UnitRuntime> units = new();

    public IReadOnlyList<UnitRuntime> Units => units;

    public event Action<UnitRuntime> OnUnitAdded;
    public event Action<UnitRuntime> OnUnitRemoved;

    public void Register(UnitRuntime unit)
    {
        if (unit == null) return;
        if (units.Contains(unit)) return;

        units.Add(unit);
        OnUnitAdded?.Invoke(unit);
    }

    public void Unregister(UnitRuntime unit)
    {
        if (unit == null) return;

        if (units.Remove(unit))
        {
            OnUnitRemoved?.Invoke(unit);
        }
    }

    public UnitRuntime FindClosestAlive(Vector3 from)
    {
        UnitRuntime best = null;
        float bestD = float.PositiveInfinity;

        for (int i = units.Count - 1; i >= 0; --i)
        {
            UnitRuntime u = units[i];

            if (u == null || !u.IsAlive)
                continue;

            float d = (u.transform.position - from).sqrMagnitude;

            if(d < bestD)
            {
                bestD = d;
                best = u;
            }
        }

        return best;
    }

    public UnitRuntime FindAny(UnitCode unitCode, int star, UnitRuntime exclude = null)
    {
        for (int i = 0; i < units.Count; i++)
        {
            var u = units[i];
            if (u == null || u == exclude) continue;
            if (u.Data == null) continue;

            if (u.Data.UnitCode == unitCode && u.Star == star)
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
