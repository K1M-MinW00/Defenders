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
        if (IsDead || damage <= 0f)
            return;

        float finalDamage = damage;

        owner.SkillController.NotifyBeforeTakeDamage(ref finalDamage);

        finalDamage = Mathf.Max(0f,finalDamage);
        float appliedDamage = state.TakeDamage(finalDamage);

        if (appliedDamage > 0f)
            GameAudioManager.Instance?.PlayCharacterSfx(owner.UnitData?.hitSound, GameAudioCue.UnitHit, GameAudioPriority.Low, 0.05f);
        
        OnHpChanged?.Invoke(owner, CurrentHp, MaxHp);

        owner.SkillController.NotifyAfterTakeDamage(appliedDamage);

        if (IsDead)
            Die();
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
