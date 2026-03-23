using System;
using UnityEngine;

public class UnitRuntime : MonoBehaviour, IDamageable
{
    public UnitDataSO Data { get; private set; }
    public int Star { get; private set; } = 1;

    public UnitStats FinalStats { get; private set; }
    public float MaxHp {  get; private set; }
    public float CurrentHp { get; private set; }
    public float CurrentMp { get; private set; }

    public bool IsDead { get; private set; }
    public bool IsAlive => !IsDead;
    public bool IsEnergyFull => CurrentMp >= FinalStats.maxMp;

    public event Action<UnitRuntime> OnInitialized;
    public event Action<UnitRuntime> OnStatsChanged;
    public event Action<UnitRuntime, float, float> OnHpChanged;
    public event Action<UnitRuntime> OnMpChanged;
    public event Action<UnitRuntime> OnDead;

    public void Initialize(UnitDataSO data, int star = 1)
    {
        Data = data;
        Star = star;

        RecalculateStats();
        RestoreForPrepare();

        IsDead = false;

        OnInitialized?.Invoke(this);
        OnStatsChanged?.Invoke(this);
    }

    public void ApplyStarUp()
    {
        Star++;
        RecalculateStats();

        MaxHp = FinalStats.maxHp;
        CurrentHp = Mathf.Min(CurrentHp, FinalStats.maxHp);
        CurrentMp = Mathf.Min(CurrentMp, FinalStats.maxMp);

        OnStatsChanged?.Invoke(this);
        OnHpChanged?.Invoke(this, CurrentHp, MaxHp);
        OnMpChanged?.Invoke(this);
    }

    public void TakeDamage(float damage)
    {
        if (IsDead || damage <= 0f)
            return;


        CurrentHp = Mathf.Max(0f, CurrentHp - damage);
        OnHpChanged?.Invoke(this, CurrentHp, MaxHp);

        if (CurrentHp <= 0f)
            Die();
    }

    public void Heal(float amount)
    {
        if (IsDead || amount <= 0f)
            return;

        float nextHp = Mathf.Min(FinalStats.maxHp, CurrentHp + amount);

        if (Mathf.Approximately(nextHp, CurrentHp))
            return;

        CurrentHp = nextHp;
        OnHpChanged?.Invoke(this, CurrentHp, MaxHp);
    }

    public void AddMp(float amount)
    {
        if (IsDead || amount <= 0f)
            return;

        float nextMp = Mathf.Min(FinalStats.maxMp, CurrentMp + amount);
        if (Mathf.Approximately(nextMp, CurrentMp))
            return;

        CurrentMp = nextMp;
        OnMpChanged?.Invoke(this);
    }

    public void ResetMp()
    {
        CurrentMp = 0f;
        OnMpChanged?.Invoke(this);
    }

    public void RestoreForPrepare()
    {
        IsDead = false;

        MaxHp = FinalStats.maxHp;
        CurrentHp = FinalStats.maxHp;
        CurrentMp = 0f;

        OnStatsChanged?.Invoke(this);
        OnHpChanged?.Invoke(this, CurrentHp, MaxHp);
        OnMpChanged?.Invoke(this);
    }

    private void Die()
    {
        if (IsDead)
            return;

        IsDead = true;
        OnDead?.Invoke(this);
    }

    private void RecalculateStats()
    {
        if (Data == null)
            return;

        FinalStats = Data.BaseStats;
        // TODO : 합성 스탯 업그레이드

    }
}
