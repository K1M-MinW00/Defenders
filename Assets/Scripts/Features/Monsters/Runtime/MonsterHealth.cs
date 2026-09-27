using System;
using UnityEngine;

public class MonsterHealth : MonoBehaviour, IDamageable
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
        if (IsDead || damage <= 0f)
            return;

        damage = state.TakeDamage(damage);
        GameAudioManager.Instance?.PlayCharacterSfx(owner?.Data?.hitSound, GameAudioCue.MonsterHit, GameAudioPriority.Low, 0.05f);

        OnHpChanged?.Invoke(this, damage);
        if (CurrentHp <= 0f)
            NotifyDead();
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
