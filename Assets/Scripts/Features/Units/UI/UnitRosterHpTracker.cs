using System;
using System.Collections.Generic;
using UnityEngine;

public class UnitRosterHpTracker : MonoBehaviour
{
    [SerializeField] private UnitRoster unitRoster;

    public event Action<float, float> OnTotalHpChanged;

    private readonly Dictionary<UnitController, HpSnapshot> hpSnapshots = new();
    private float totalCurrentHp;
    private float totalMaxHp;
    private bool isBound;

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

    private void OnEnable()
    {
        if(unitRoster == null)
        {
            Debug.LogError($"UnitRosterHpTracker : Unit Roster is null.");
            return;
        }

        if (!isBound)
        {
            unitRoster.OnRosterChanged += Rebuild;
            isBound = true;
        }

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

        isBound = false;
        UnsubscribeAll();
        hpSnapshots.Clear();
        totalCurrentHp = 0f;
        totalMaxHp = 0f;
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

        if (unitRoster == null)
            return;

        foreach (UnitController unit in unitRoster.Units)
        {
            if (unit == null)
                continue;

            unit.Health.OnHpChanged += HandleUnitHpChanged;
            unit.Health.OnDead += HandleUnitDead;

            float currentHp = unit.Health.CurrentHp;
            float maxHp = unit.Health.MaxHp;

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

        HandleUnitHpChanged(unit, unit.Health.CurrentHp, unit.Health.MaxHp);
    }

    private void UnsubscribeAll()
    {
        foreach (UnitController unit in hpSnapshots.Keys)
        {
            if (unit == null)
                continue;

            unit.Health.OnHpChanged -= HandleUnitHpChanged;
            unit.Health.OnDead -= HandleUnitDead;
        }
    }
}
