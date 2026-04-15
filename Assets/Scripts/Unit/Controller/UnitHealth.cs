using System;
using UnityEngine;

public class UnitHealth : MonoBehaviour, IDamageable
{
    private UnitController owner;

    public event Action<UnitController, float, float> OnHpChanged;
    public event Action<UnitController> OnDead;

    public float CurrentHp => owner.Runtime.CurrentHp;
    public float MaxHp => owner.Runtime.FinalStats.MaxHp;
    public bool IsDead => owner.Runtime == null || owner.Runtime.IsDead;

    public void Initialize(UnitController owner)
    {
        this.owner = owner;
    }

    public void RestoreFull()
    {
        if (owner.Runtime == null) return;

        owner.Runtime.CurrentHp = owner.Runtime.FinalStats.MaxHp;
        OnHpChanged?.Invoke(owner,CurrentHp, MaxHp);
    }

    public void TakeDamage(float damage)
    {
        if (owner.Runtime == null || IsDead || damage <= 0f)
            return;

        owner.Runtime.CurrentHp = Mathf.Max(0f, owner.Runtime.CurrentHp - damage);
        OnHpChanged?.Invoke(owner, CurrentHp, MaxHp);

        if (owner.Runtime.CurrentHp <= 0f)
            Die();
    }

    public void Heal(float amount)
    {
        if (owner.Runtime == null || IsDead || amount <= 0f)
            return;

        float nextHp = Mathf.Min(MaxHp, CurrentHp + amount);
        if (Mathf.Approximately(nextHp, CurrentHp))
            return;

        owner.Runtime.CurrentHp = nextHp;
        OnHpChanged?.Invoke(owner, CurrentHp, MaxHp);
    }

    private void Die()
    {
        if (owner.Runtime.IsDead)
            return;

        owner.Runtime.IsDead = true;

        owner.Movement.EnableMovement(false);
        owner.Targeting.ClearTarget();
        owner.Targeting.EnableSensor(false);
        owner.FSMController.ChangeToDead();

        OnDead?.Invoke(owner);
    }
}