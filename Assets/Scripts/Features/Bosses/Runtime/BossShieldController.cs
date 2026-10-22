using System;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class BossShieldController : MonoBehaviour, IIncomingDamageModifier
{
    private MonsterController owner;
    private MonsterHealth ownerHealth;
    private BossDefinitionSO definition;
    private bool hasTriggered;

    public int Priority => -100;
    public float CurrentShield { get; private set; }
    public float MaxShield { get; private set; }
    public bool IsActive => CurrentShield > 0f;

    public event Action<float, float> ShieldChanged;
    public event Action ShieldBroken;

    public void Initialize(MonsterController monster, BossDefinitionSO bossDefinition)
    {
        ResetRuntime();
        owner = monster;
        ownerHealth = owner != null ? owner.GetComponent<MonsterHealth>() : null;
        definition = bossDefinition;
        if (ownerHealth != null)
            ownerHealth.OnHpChanged += HandleHpChanged;
    }

    public float ModifyIncomingDamage(DamageRequest request, float damage)
    {
        if (!IsActive || damage <= 0f)
            return damage;

        float absorbed = Mathf.Min(CurrentShield, damage);
        CurrentShield -= absorbed;
        ShieldChanged?.Invoke(CurrentShield, MaxShield);

        if (CurrentShield <= 0f)
        {
            CurrentShield = 0f;
            ShieldBroken?.Invoke();
        }

        return damage - absorbed;
    }

    public void ResetRuntime()
    {
        if (ownerHealth != null)
            ownerHealth.OnHpChanged -= HandleHpChanged;

        owner = null;
        ownerHealth = null;
        definition = null;
        hasTriggered = false;
        CurrentShield = 0f;
        MaxShield = 0f;
        ShieldChanged = null;
        ShieldBroken = null;
    }

    private void HandleHpChanged(MonsterHealth health, float _)
    {
        if (hasTriggered || definition == null || health == null || health.MaxHp <= 0f)
            return;

        if (health.CurrentHp / health.MaxHp > definition.shieldTriggerHpRatio)
            return;

        hasTriggered = true;
        MaxShield = health.MaxHp * definition.shieldMaxHpRatio;
        CurrentShield = MaxShield;
        ShieldChanged?.Invoke(CurrentShield, MaxShield);
        PlayActivationSound();
    }

    private void PlayActivationSound()
    {
        AudioClipSettings audio = definition?.audio?.shieldActivated;
        if (audio == null || !audio.IsValid)
            return;

        GameAudioManager.Instance?.PlaySfx(
            audio,
            GameAudioPriority.High,
            GameAudioCue.BossShieldActivated,
            0.1f);
    }
}
