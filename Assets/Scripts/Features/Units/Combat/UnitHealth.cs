using System;
using UnityEngine;

public class UnitHealth : MonoBehaviour, IDamageable
{
    private UnitController owner;
    private float currentHp;
    private float maxHp;
    private bool isDead;

    public float CurrentHp => currentHp;
    public float MaxHp => maxHp;
    public bool IsDead => isDead;

    public event Action<UnitController, float, float> OnHpChanged;
    public event Action<UnitController> OnDead;

    public void Initialize(UnitController owner)
    {
        this.owner = owner;

        maxHp = owner.Runtime.FinalStats.MaxHp;
        RestoreFull();
    }

    public void ApplyStatRefresh(float newMaxHp)
    {
        if (maxHp == newMaxHp)
            return;

        newMaxHp = Mathf.Max(maxHp,newMaxHp);

        maxHp = newMaxHp;
        RestoreFull() ;
    }

    public void RestoreFull()
    {
        isDead = false;
        currentHp = maxHp;
        OnHpChanged?.Invoke(owner,currentHp, maxHp);
    }

    public void TakeDamage(float damage)
    {
        if (isDead || damage <= 0f)
            return;

        float finalDamage = damage;

        owner.SkillController.NotifyBeforeTakeDamage(ref finalDamage);

        finalDamage = Mathf.Max(0f,finalDamage);
        currentHp = Mathf.Max(0f, currentHp - finalDamage);
        
        OnHpChanged?.Invoke(owner, CurrentHp, MaxHp);

        owner.SkillController.NotifyAfterTakeDamage(finalDamage);

        if (currentHp <= 0f)
            Die();
    }

    public void Heal(float amount)
    {
        if (isDead || amount <= 0f)
            return;

        float nextHp = Mathf.Min(maxHp, currentHp + amount);

        if (Mathf.Approximately(nextHp, currentHp))
            return;

        currentHp = nextHp;
        OnHpChanged?.Invoke(owner, currentHp, maxHp);
    }

    private void Die()
    {
        if (isDead)
            return;

        isDead = true;

        owner.Movement.EnableMovement(false);
        owner.Targeting.ClearTarget();
        owner.Targeting.EnableSensor(false);
        owner.FSMController.ChangeToDead();

        OnDead?.Invoke(owner);
    }
}