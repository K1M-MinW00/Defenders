using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class UnitStatusEffectController : MonoBehaviour
{
    public readonly struct Snapshot
    {
        public StatusEffectType Type { get; }
        public float RemainingTime { get; }
        public float Duration { get; }
        public float Magnitude { get; }

        public Snapshot(StatusEffectType type, float remainingTime, float duration, float magnitude)
        {
            Type = type;
            RemainingTime = remainingTime;
            Duration = duration;
            Magnitude = magnitude;
        }
    }

    private sealed class RuntimeEffect
    {
        public StatusEffectType Type;
        public float RemainingTime;
        public float Duration;
        public float Magnitude;
        public float TickInterval;
        public float TimeUntilNextTick;
        public ICombatTarget Source;
    }

    private readonly List<RuntimeEffect> activeEffects = new();
    private readonly List<Snapshot> snapshots = new();
    private UnitController owner;
    private UnitMovementController movement;

    public IReadOnlyList<Snapshot> ActiveEffects => snapshots;
    public bool IsStunned => HasEffect(StatusEffectType.Stun);
    public float DefenseReductionPenalty => GetStrongestMagnitude(StatusEffectType.DefenseDown);
    public event Action EffectsChanged;
    public event Action<StatusEffectType, float> PeriodicEffectTicked;

    public void Initialize(UnitController unit)
    {
        ClearAll();
        owner = unit;
        movement = unit != null ? unit.GetComponent<UnitMovementController>() : null;
        RefreshSnapshots();
    }

    private void Update()
    {
        Tick(Time.deltaTime);
    }

    public void Tick(float deltaTime)
    {
        if (owner == null || activeEffects.Count == 0)
            return;

        bool changed = false;
        for (int index = activeEffects.Count - 1; index >= 0; index--)
        {
            RuntimeEffect effect = activeEffects[index];
            float elapsed = Mathf.Max(0f, deltaTime);
            float activeElapsed = Mathf.Min(elapsed, effect.RemainingTime);
            TickPeriodicEffect(effect, activeElapsed);
            effect.RemainingTime -= elapsed;
            if (effect.RemainingTime > 0f)
                continue;

            activeEffects.RemoveAt(index);
            changed = true;
        }

        if (changed)
        {
            ApplyRuntimeModifiers();
            NotifyChanged();
        }
        else
        {
            RefreshSnapshots();
        }
    }

    public bool Apply(StatusEffectApplication application)
    {
        if (owner == null || (owner.Health != null && owner.IsDead) || !application.IsValid)
            return false;

        bool wasStunned = IsStunned;
        RuntimeEffect existing = activeEffects.Find(effect => effect.Type == application.Type);
        if (existing == null)
        {
            activeEffects.Add(new RuntimeEffect
            {
                Type = application.Type,
                RemainingTime = application.Duration,
                Duration = application.Duration,
                Magnitude = application.Magnitude,
                TickInterval = application.TickInterval,
                TimeUntilNextTick = application.TickInterval,
                Source = application.Source
            });
        }
        else
        {
            existing.RemainingTime = Mathf.Max(existing.RemainingTime, application.Duration);
            existing.Duration = Mathf.Max(existing.Duration, application.Duration);
            if (application.Magnitude >= existing.Magnitude)
            {
                existing.Magnitude = application.Magnitude;
                existing.Source = application.Source;
            }

            if (application.TickInterval > 0f && existing.TickInterval <= 0f)
            {
                existing.TickInterval = application.TickInterval;
                existing.TimeUntilNextTick = application.TickInterval;
            }
        }

        ApplyRuntimeModifiers();
        if (!wasStunned && application.Type == StatusEffectType.Stun)
            InterruptForStun();
        NotifyChanged();
        return true;
    }

    public bool TryGet(StatusEffectType type, out Snapshot snapshot)
    {
        foreach (RuntimeEffect effect in activeEffects)
        {
            if (effect.Type != type)
                continue;

            snapshot = new Snapshot(effect.Type, effect.RemainingTime, effect.Duration, effect.Magnitude);
            return true;
        }

        snapshot = default;
        return false;
    }

    public void ClearAll()
    {
        bool hadEffects = activeEffects.Count > 0;
        activeEffects.Clear();
        movement?.SetSpeedMultiplier(1f);
        RefreshSnapshots();

        if (hadEffects)
            EffectsChanged?.Invoke();
    }

    public void ResetRuntime()
    {
        ClearAll();
        owner = null;
        movement = null;
        EffectsChanged = null;
        PeriodicEffectTicked = null;
        snapshots.Clear();
    }

    private void ApplyRuntimeModifiers()
    {
        float strongestSlow = 0f;
        foreach (RuntimeEffect effect in activeEffects)
        {
            if (effect.Type == StatusEffectType.Slow)
                strongestSlow = Mathf.Max(strongestSlow, effect.Magnitude);
        }

        movement?.SetSpeedMultiplier(1f - Mathf.Clamp01(strongestSlow));
    }

    private bool HasEffect(StatusEffectType type)
    {
        return activeEffects.Exists(effect => effect.Type == type && effect.RemainingTime > 0f);
    }

    private float GetStrongestMagnitude(StatusEffectType type)
    {
        float strongest = 0f;
        foreach (RuntimeEffect effect in activeEffects)
        {
            if (effect.Type == type)
                strongest = Mathf.Max(strongest, effect.Magnitude);
        }

        return strongest;
    }

    private void InterruptForStun()
    {
        movement?.Stop();
        owner?.Combat?.CancelAttack();
        owner?.SkillController?.CancelSkill();
        owner?.Animation?.PlayIdle();
    }

    private void TickPeriodicEffect(RuntimeEffect effect, float elapsed)
    {
        if (!IsPeriodicDamage(effect.Type) || effect.TickInterval <= 0f || elapsed <= 0f)
            return;

        effect.TimeUntilNextTick -= elapsed;
        while (effect.TimeUntilNextTick <= 0f)
        {
            ApplyPeriodicDamage(effect);
            effect.TimeUntilNextTick += effect.TickInterval;

            if (owner == null || (owner.Health != null && owner.IsDead))
                break;
        }
    }

    private static bool IsPeriodicDamage(StatusEffectType type)
    {
        return type == StatusEffectType.Poison || type == StatusEffectType.Burn;
    }

    private void ApplyPeriodicDamage(RuntimeEffect effect)
    {
        if (effect.Magnitude <= 0f)
            return;

        PeriodicEffectTicked?.Invoke(effect.Type, effect.Magnitude);
        owner?.CombatHealth?.ApplyDamage(
            new DamageRequest(effect.Magnitude, effect.Source, DamageOrigin.Effect));
    }

    private void NotifyChanged()
    {
        RefreshSnapshots();
        EffectsChanged?.Invoke();
    }

    private void RefreshSnapshots()
    {
        snapshots.Clear();
        foreach (RuntimeEffect effect in activeEffects)
            snapshots.Add(new Snapshot(
                effect.Type,
                Mathf.Max(0f, effect.RemainingTime),
                Mathf.Max(0.01f, effect.Duration),
                effect.Magnitude));
    }
}
