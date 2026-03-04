using System;
using UnityEngine;

public class MonsterHealth : MonoBehaviour, IDamageable
{
    private MonsterStats stats;

    public float CurrentHp { get; private set; }
    public bool IsDead { get; private set; }

    public event Action<MonsterHealth> OnDead;

    public void Initialize(MonsterStats s)
    {
        stats = s;
    }

    public void ResetHealth()
    {
        IsDead = false;
        CurrentHp = (stats != null) ? stats.maxHp : 1f;
    }

    public void TakeDamage(float damage)
    {
        if (IsDead || damage <= 0f)
            return;
        
        CurrentHp -= damage;

        DamageUIService.Instance?.Show(transform.position, damage);

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