using System;
using UnityEngine;

public class MonsterHealth : MonoBehaviour, ICombatHealth
{
    private MonsterStats stats;
    private MonsterController owner;
    private readonly CombatHealthState state = new();

    public float MaxHp => state.Max;
    public float CurrentHp => state.Current;
    public bool IsDead => state.IsDead;

    public event Action<MonsterHealth> OnDead;
    public event Action<MonsterHealth, float> OnHpChanged;

    private void Awake()
    {
        owner = GetComponent<MonsterController>();
    }

    public void Initialize(MonsterStats s)
    {
        stats = s;
        ResetHealth();
    }

    public void ResetHealth()
    {
        state.Initialize(stats.maxHp);
    }

    public void TakeDamage(float damage)
    {
        ApplyDamage(new DamageRequest(damage));
    }

    public DamageResult ApplyDamage(DamageRequest request)
    {
        if (IsDead)
            return DamageResult.Rejected(request.Amount, DamageRejectReason.TargetAlreadyDead);

        if (request.Amount <= 0f)
            return DamageResult.Rejected(request.Amount, DamageRejectReason.InvalidAmount);

        float appliedDamage = state.TakeDamage(request.Amount);
        if (appliedDamage <= 0f)
            return DamageResult.Rejected(request.Amount, DamageRejectReason.InvalidAmount);

        GameAudioManager.Instance?.PlayCharacterSfx(owner?.Data?.hitSound, GameAudioCue.MonsterHit, GameAudioPriority.Low, 0.05f);

        OnHpChanged?.Invoke(this, appliedDamage);
        if (CurrentHp <= 0f)
            NotifyDead();

        return DamageResult.Applied(request.Amount, appliedDamage, IsDead);
    }

    public void Kill() => Die();

    public void Die()
    {
        if (!state.Kill())
            return;

        NotifyDead();
    }

    private void NotifyDead()
    {
        OnDead?.Invoke(this);
    }
}
