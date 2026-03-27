using System;
using System.Collections.Generic;
using System.Dynamic;
using UnityEngine;

public class UnitRosterHpTracker : MonoBehaviour
{
    private UnitRoster unitRoster;

    public event Action<float, float> OnTotalHpChanged;

    private readonly Dictionary<UnitController, HpSnapshot> hpSnapshots = new();
    private float totalCurrentHp;
    private float totalMaxHp;

    public float CurrentHp => totalCurrentHp;
    public float MaxHp => totalMaxHp;

    private struct HpSnapshot
    {
        public float CurrentHp;
        public float MaxHp;

        public HpSnapshot(float currentHp, float maxHp)
        {
            CurrentHp = currentHp;
            MaxHp = maxHp;
        }
    }

    public void Init(UnitRoster unitRoster)
    {
        Unbind();

        this.unitRoster = unitRoster;
        unitRoster.OnRosterChanged += Rebuild;

        Rebuild();
    }

    private void OnDisable()
    {
        Unbind();
    }

    private void Unbind()
    {
        if (unitRoster != null)
            unitRoster.OnRosterChanged -= Rebuild;

        UnsubscribeAll();
        unitRoster = null;
    }

    public float GetHpRatio()
    {
        if (totalMaxHp <= 0f)
            return 1f;

        return totalCurrentHp / totalMaxHp;
    }

    public void Rebuild()
    {
        UnsubscribeAll();

        hpSnapshots.Clear();
        totalCurrentHp = 0f;
        totalMaxHp = 0f;

        foreach (UnitController unit in unitRoster.Units)
        {
            if (unit == null)
                continue;

            unit.OnHpChanged += HandleUnitHpChanged;
            unit.OnDead += HandleUnitDead;

            float currentHp = unit.CurrentHp;
            float maxHp = unit.MaxHp;

            hpSnapshots[unit] = new HpSnapshot(currentHp, maxHp);
            totalCurrentHp += currentHp;
            totalMaxHp += maxHp;
        }

        OnTotalHpChanged?.Invoke(totalCurrentHp, totalMaxHp);
    }

    private void HandleUnitHpChanged(UnitController unit, float currentHp, float maxHp)
    {
        if (unit == null)
            return;

        if (!hpSnapshots.TryGetValue(unit, out HpSnapshot oldSnapshot))
            return;

        totalCurrentHp -= oldSnapshot.CurrentHp;
        totalMaxHp -= oldSnapshot.MaxHp;

        hpSnapshots[unit] = new HpSnapshot(currentHp, maxHp);

        totalCurrentHp += currentHp;
        totalMaxHp += maxHp;

        totalCurrentHp = Mathf.Max(0f, totalCurrentHp);
        totalMaxHp = Mathf.Max(0f, totalMaxHp);

        OnTotalHpChanged?.Invoke(totalCurrentHp, totalMaxHp);
    }

    private void HandleUnitDead(UnitController unit)
    {
        if (unit == null)
            return;

        HandleUnitHpChanged(unit, unit.CurrentHp, unit.MaxHp);
    }

    private void UnsubscribeAll()
    {
        foreach (UnitController unit in hpSnapshots.Keys)
        {
            if (unit == null)
                continue;

            unit.OnHpChanged -= HandleUnitHpChanged;
            unit.OnDead -= HandleUnitDead;
        }
    }
}