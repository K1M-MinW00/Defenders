using System;
using UnityEngine;

public class MonsterHealth : MonoBehaviour, IDamageable
{
    private MonsterStats stats;

    public float MaxHp {  get; private set; }
    public float CurrentHp { get; private set; }
    public bool IsDead { get; private set; }

    public event Action<MonsterHealth> OnDead;
    public event Action<MonsterHealth, float> OnHpChanged;

    public void Initialize(MonsterStats s)
    {
        stats = s;
        ResetHealth();
    }

    public void ResetHealth()
    {
        IsDead = false;

        MaxHp = stats.maxHp;
        CurrentHp = MaxHp;
    }

    public void TakeDamage(float damage)
    {
        if (IsDead || damage <= 0f)
            return;

        damage = Mathf.Min(damage, CurrentHp);
        CurrentHp -= damage;

        int finalDamage = (int)damage;

        OnHpChanged?.Invoke(this, damage);
        if (CurrentHp <= 0f)
            Die();
    }

    public void Kill() => Die();

    public void Die()
    {
        if (IsDead)
            return;

        IsDead  = true;
        OnDead?.Invoke(this);
    }
}