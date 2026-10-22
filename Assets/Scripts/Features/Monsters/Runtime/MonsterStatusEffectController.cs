using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class MonsterStatusEffectController : MonoBehaviour
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
    private MonsterController owner;

    public IReadOnlyList<Snapshot> ActiveEffects => snapshots;
    public bool IsStunned => HasEffect(StatusEffectType.Stun);
    public float DefenseReductionPenalty => GetStrongestMagnitude(StatusEffectType.DefenseDown);
    public event Action EffectsChanged;
    public event Action<StatusEffectType> EffectImmune;

    public void Initialize(MonsterController monster)
    {
        ClearAll();
        owner = monster;
        RefreshSnapshots();
    }

    private void Update() => Tick(Time.deltaTime);

    public void Tick(float deltaTime)
    {
        if (owner == null || activeEffects.Count == 0)
            return;

        bool wasStunned = IsStunned;
        bool changed = false;
        float elapsed = Mathf.Max(0f, deltaTime);
        for (int index = activeEffects.Count - 1; index >= 0; index--)
        {
            RuntimeEffect effect = activeEffects[index];
            TickPeriodicEffect(effect, Mathf.Min(elapsed, effect.RemainingTime));
            effect.RemainingTime -= elapsed;
            if (effect.RemainingTime <= 0f)
            {
                activeEffects.RemoveAt(index);
                changed = true;
            }
        }

        ApplySlow();
        if (wasStunned && !IsStunned)
            owner.ResumeBehaviorAfterControlEffect();
        RefreshSnapshots();
        if (changed)
            EffectsChanged?.Invoke();
    }

    public bool Apply(StatusEffectApplication application)
    {
        if (owner == null || owner.IsDead || !application.IsValid)
            return false;

        BossStatusResistanceSettings resistance = owner.Data?.BossDefinition?.statusResistance;
        if (!StatusEffectResistanceResolver.TryResolve(application, resistance, out StatusEffectApplication resolved))
        {
            if (application.IsValid && StatusEffectResistanceResolver.IsImmune(application.Type, resistance))
                EffectImmune?.Invoke(application.Type);
            return false;
        }

        bool wasStunned = IsStunned;
        RuntimeEffect existing = activeEffects.Find(effect => effect.Type == resolved.Type);
        if (existing == null)
        {
            activeEffects.Add(new RuntimeEffect
            {
                Type = resolved.Type,
                RemainingTime = resolved.Duration,
                Duration = resolved.Duration,
                Magnitude = resolved.Magnitude,
                TickInterval = resolved.TickInterval,
                TimeUntilNextTick = resolved.TickInterval,
                Source = resolved.Source,
            });
        }
        else
        {
            existing.RemainingTime = Mathf.Max(existing.RemainingTime, resolved.Duration);
            existing.Duration = Mathf.Max(existing.Duration, resolved.Duration);
            if (resolved.Magnitude >= existing.Magnitude)
            {
                existing.Magnitude = resolved.Magnitude;
                existing.Source = resolved.Source;
            }
        }

        ApplySlow();
        if (!wasStunned && IsStunned)
            owner.PrepareForControlEffect();
        RefreshSnapshots();
        EffectsChanged?.Invoke();
        return true;
    }

    public bool TryGet(StatusEffectType type, out float remainingTime, out float magnitude)
    {
        RuntimeEffect effect = activeEffects.Find(candidate => candidate.Type == type);
        if (effect == null)
        {
            remainingTime = 0f;
            magnitude = 0f;
            return false;
        }

        remainingTime = Mathf.Max(0f, effect.RemainingTime);
        magnitude = effect.Magnitude;
        return true;
    }

    public void ClearAll()
    {
        bool wasStunned = IsStunned;
        activeEffects.Clear();
        owner?.SetStatusMoveSpeedMultiplier(1f);
        if (wasStunned)
            owner?.ResumeBehaviorAfterControlEffect();
        RefreshSnapshots();
        EffectsChanged?.Invoke();
    }

    public void ResetRuntime()
    {
        ClearAll();
        owner = null;
    }

    private void ApplySlow()
    {
        owner?.SetStatusMoveSpeedMultiplier(1f - Mathf.Clamp01(GetStrongestMagnitude(StatusEffectType.Slow)));
    }

    private bool HasEffect(StatusEffectType type) =>
        activeEffects.Exists(effect => effect.Type == type && effect.RemainingTime > 0f);

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

    private void TickPeriodicEffect(RuntimeEffect effect, float elapsed)
    {
        if ((effect.Type != StatusEffectType.Poison && effect.Type != StatusEffectType.Burn) ||
            effect.TickInterval <= 0f || elapsed <= 0f)
            return;

        effect.TimeUntilNextTick -= elapsed;
        while (effect.TimeUntilNextTick <= 0f)
        {
            owner.CombatHealth.ApplyDamage(new DamageRequest(effect.Magnitude, effect.Source, DamageOrigin.Effect));
            effect.TimeUntilNextTick += effect.TickInterval;
            if (owner.IsDead)
                break;
        }
    }

    private void RefreshSnapshots()
    {
        snapshots.Clear();
        foreach (RuntimeEffect effect in activeEffects)
        {
            snapshots.Add(new Snapshot(
                effect.Type,
                Mathf.Max(0f, effect.RemainingTime),
                Mathf.Max(0.01f, effect.Duration),
                effect.Magnitude));
        }
    }
}
