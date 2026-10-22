using System;
using UnityEngine;

public class MonsterHealth : MonoBehaviour, ICombatHealth
{
    private MonsterStats stats;
    private MonsterController owner;
    private readonly CombatHealthState state = new();
    private readonly System.Collections.Generic.List<IIncomingDamageModifier> damageModifiers = new();

    public float MaxHp => state.Max;
    public float CurrentHp => state.Current;
    public bool IsDead => state.IsDead;

    public event Action<MonsterHealth> OnDead;
    public event Action<MonsterHealth, float> OnHpChanged;
    public event Action<MonsterHealth, DamageResult> OnDamaged;

    private void Awake()
    {
        owner = GetComponent<MonsterController>();
    }

    public void Initialize(MonsterStats s)
    {
        stats = s;
        CacheDamageModifiers();
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
        DamageResolution resolution = DamageResolver.Resolve(
            request,
            CurrentHp,
            IsDead,
            damage => ApplyIncomingDamageModifiers(request, damage));
        if (!resolution.CanApply)
            return DamageResult.Rejected(request.Amount, resolution.RejectReason);

        float appliedDamage = state.TakeDamage(resolution.AppliedAmount);

        GameAudioManager.Instance?.PlayCharacterSfx(
            owner?.Data?.hitSound,
            GameAudioCue.MonsterHit,
            GameAudioPriority.Low,
            0.05f,
            owner);

        OnHpChanged?.Invoke(this, appliedDamage);

        DamageResult result = DamageResult.Applied(
            request.Amount,
            appliedDamage,
            resolution.IsLethal,
            resolution.IsCritical);

        CombatDebugTelemetry.ReportDamageApplied(this, request, result);
        OnDamaged?.Invoke(this, result);

        if (!result.WasLethal && request.Source is UnitController unitSource)
            unitSource.SkillController?.TryApplyDamageStatusEffect(owner, request.Origin);

        if (CurrentHp <= 0f)
            NotifyDead();

        return result;
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

    public void ClearTransientListeners()
    {
        OnHpChanged = null;
        OnDamaged = null;
        damageModifiers.Clear();
    }

    private void CacheDamageModifiers()
    {
        damageModifiers.Clear();
        MonoBehaviour[] components = GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour component in components)
        {
            if (component is IIncomingDamageModifier modifier)
                damageModifiers.Add(modifier);
        }

        damageModifiers.Sort((left, right) => left.Priority.CompareTo(right.Priority));
    }

    private float ApplyIncomingDamageModifiers(DamageRequest request, float damage)
    {
        float defensePenalty = owner?.StatusEffects?.DefenseReductionPenalty ?? 0f;
        float result = damage * (1f + Mathf.Clamp(defensePenalty, 0f, 90f) / 100f);
        foreach (IIncomingDamageModifier modifier in damageModifiers)
        {
            result = modifier.ModifyIncomingDamage(request, result);
            if (result <= 0f)
                return 0f;
        }

        return result;
    }
}
