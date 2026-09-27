using System;
using UnityEngine;

public class UnitHealth : MonoBehaviour, ICombatHealth
{
    private UnitController owner;
    private readonly CombatHealthState state = new();

    public float CurrentHp => state.Current;
    public float MaxHp => state.Max;
    public bool IsDead => state.IsDead;

    public event Action<UnitController, float, float> OnHpChanged;
    public event Action<UnitController, DamageResult> OnDamaged;
    public event Action<UnitController> OnDead;

    public void Initialize(UnitController owner)
    {
        this.owner = owner;

        state.Initialize(owner.Runtime.FinalStats.MaxHp);
        OnHpChanged?.Invoke(owner, CurrentHp, MaxHp);
    }

    public void ApplyStatRefresh(float newMaxHp, StatRefreshPolicy refreshPolicy)
    {
        bool fullHeal = refreshPolicy == StatRefreshPolicy.FullHeal;
        if (!fullHeal && Mathf.Approximately(MaxHp, newMaxHp))
            return;

        state.SetMaximum(newMaxHp, fullHeal);
        OnHpChanged?.Invoke(owner, CurrentHp, MaxHp);
    }

    public void RestoreFull()
    {
        state.RestoreFull();
        OnHpChanged?.Invoke(owner, CurrentHp, MaxHp);
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
            ApplyDefensiveModifiers);

        if (!resolution.CanApply)
            return DamageResult.Rejected(request.Amount, resolution.RejectReason);

        float appliedDamage = state.TakeDamage(resolution.AppliedAmount);

        GameAudioManager.Instance?.PlayCharacterSfx(owner.UnitData?.hitSound, GameAudioCue.UnitHit, GameAudioPriority.Low, 0.05f);
        
        OnHpChanged?.Invoke(owner, CurrentHp, MaxHp);

        owner.SkillController.NotifyAfterTakeDamage(appliedDamage);

        DamageResult result = DamageResult.Applied(
            request.Amount,
            appliedDamage,
            resolution.IsLethal,
            resolution.IsCritical);

        OnDamaged?.Invoke(owner, result);

        if (IsDead)
            Die();

        return result;
    }

    private float ApplyDefensiveModifiers(float damage)
    {
        owner.SkillController.NotifyBeforeTakeDamage(ref damage);
        return damage;
    }

    public void Heal(float amount)
    {
        if (state.Heal(amount) <= 0f)
            return;

        OnHpChanged?.Invoke(owner, CurrentHp, MaxHp);
    }

    private void Die()
    {
        if (!IsDead)
            return;

        owner.Movement.EnableMovement(false);
        owner.Targeting.ClearTarget();
        owner.Targeting.EnableSensor(false);
        owner.FSMController.ChangeToDead();

        OnDead?.Invoke(owner);
    }
}
