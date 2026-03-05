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

    public UnitInstance FindClosestAlive(Vector3 from)
    {
        UnitInstance best = null;
        float bestD = float.PositiveInfinity;

        for (int i = units.Count - 1; i >= 0; --i)
        {
            UnitInstance u = units[i];

            if (u == null || !u.IsAlive)
                continue;

            float d = (u.transform.position - from).sqrMagnitude;

            if(d < bestD)
            {
                bestD = d;
                best = u;
            }
        }


        if(best == null) 
            return null;

        return best;
    }

    public UnitInstance FindAny(string unitId, int star, UnitInstance exclude = null)
    {
        for (int i = 0; i < units.Count; i++)
        {
            var u = units[i];
            if (u == null || u == exclude) continue;
            if (u.Data == null) continue;

            if (u.Data.UnitId == unitId && u.Star == star)
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
