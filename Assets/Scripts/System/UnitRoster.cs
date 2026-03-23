using System;
using System.Collections.Generic;
using UnityEngine;

public class UnitRoster : MonoBehaviour
{
    private readonly List<UnitRuntime> units = new();
    public IReadOnlyList<UnitRuntime> Units => units;

    private readonly Dictionary<UnitRuntime, HpSnapshot> hpSnapshots = new();

    public event Action OnRosterChanged; // PopulationManager 인원수 관리
    public event Action<float, float> OnTotalHpChanged;
    public event Action OnAliveCountChanged;

    private float totalCurrentHp;
    private float totalMaxHp;

    private struct HpSnapshot
    {
        public float CurrentHp;
        public float MaxHp;

        public HpSnapshot(float currentHp, float maxHp)
        {
            CurrentHp= currentHp;
            MaxHp= maxHp;
        }
    }

    public void Register(UnitRuntime unit)
    {
        if (unit == null || units.Contains(unit)) 
            return;

        units.Add(unit);

        float currentHp = unit.CurrentHp;
        float maxHp = unit.MaxHp;
        hpSnapshots[unit] = new HpSnapshot(currentHp, maxHp);

        totalCurrentHp += unit.CurrentHp;
        totalMaxHp += unit.MaxHp;
        
        unit.OnHpChanged += HandleUnitHpChanged;
        unit.OnDead += HandleUnitDead;
        OnRosterChanged?.Invoke();
        OnTotalHpChanged?.Invoke(totalCurrentHp, totalMaxHp);
    }

    public void Unregister(UnitRuntime unit)
    {
        if (unit == null) return;

        if (!units.Remove(unit))
            return;

        unit.OnHpChanged -= HandleUnitHpChanged;
        
        if(hpSnapshots.TryGetValue(unit, out HpSnapshot snapshot))
        {
            totalCurrentHp -= snapshot.CurrentHp;
            totalMaxHp -= unit.MaxHp;
            hpSnapshots.Remove(unit);
        }

        OnRosterChanged?.Invoke();
        OnTotalHpChanged?.Invoke(totalCurrentHp,totalMaxHp);
    }

    private void HandleUnitHpChanged(UnitRuntime unit, float curHp,float maxHp)
    {
        if(unit == null) 
            return;

        if (!hpSnapshots.TryGetValue(unit, out HpSnapshot oldSnapshot))
            return;

        totalCurrentHp -= oldSnapshot.CurrentHp;
        totalMaxHp -= oldSnapshot.MaxHp;

        hpSnapshots[unit] = new(curHp,maxHp);

        totalCurrentHp += curHp;
        totalMaxHp += maxHp;

        OnTotalHpChanged?.Invoke(totalCurrentHp, totalMaxHp);
    }

    private void HandleUnitDead(UnitRuntime runtime)
    {
        OnAliveCountChanged?.Invoke();
    }

    public float GetHpRatio()
    {
        if (totalMaxHp <= 0f)
            return 1f;

        return totalCurrentHp / totalMaxHp;
    }
    public void ForceRefreshHp()
    {
        totalCurrentHp = 0f;
        totalMaxHp = 0f;

        foreach (UnitRuntime unit in units)
        {
            if (unit == null)
                continue;

            float currentHp = unit.CurrentHp;
            float maxHp = unit.MaxHp;

            hpSnapshots[unit] = new HpSnapshot(currentHp, maxHp);
            totalCurrentHp += currentHp;
            totalMaxHp += maxHp;
        }

        OnTotalHpChanged?.Invoke(totalCurrentHp, totalMaxHp);
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
