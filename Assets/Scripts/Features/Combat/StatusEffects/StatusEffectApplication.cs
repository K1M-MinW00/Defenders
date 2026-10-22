using UnityEngine;

public readonly struct StatusEffectApplication
{
    public StatusEffectType Type { get; }
    public float Duration { get; }
    public float Magnitude { get; }
    public float TickInterval { get; }
    public ICombatTarget Source { get; }

    public bool IsValid => Duration > 0f && Magnitude >= 0f;

    public StatusEffectApplication(
        StatusEffectType type,
        float duration,
        float magnitude,
        float tickInterval = 0f,
        ICombatTarget source = null)
    {
        Type = type;
        Duration = Mathf.Max(0f, duration);
        Magnitude = Mathf.Max(0f, magnitude);
        TickInterval = Mathf.Max(0f, tickInterval);
        Source = source;
    }

    public static StatusEffectApplication Slow(float duration, float slowRatio)
    {
        return new StatusEffectApplication(StatusEffectType.Slow, duration, Mathf.Clamp01(slowRatio));
    }

    public static StatusEffectApplication Stun(float duration)
    {
        return new StatusEffectApplication(StatusEffectType.Stun, duration, 1f);
    }

    public static StatusEffectApplication Poison(
        float duration,
        float damagePerTick,
        ICombatTarget source = null,
        float tickInterval = 1f)
    {
        return new StatusEffectApplication(
            StatusEffectType.Poison,
            duration,
            damagePerTick,
            Mathf.Max(0.05f, tickInterval),
            source);
    }

    public static StatusEffectApplication Burn(
        float duration,
        float damagePerTick,
        ICombatTarget source = null,
        float tickInterval = 0.5f)
    {
        return new StatusEffectApplication(
            StatusEffectType.Burn,
            duration,
            damagePerTick,
            Mathf.Max(0.05f, tickInterval),
            source);
    }

    public static StatusEffectApplication DefenseDown(float duration, float percentagePoints)
    {
        return new StatusEffectApplication(
            StatusEffectType.DefenseDown,
            duration,
            Mathf.Clamp(percentagePoints, 0f, 90f));
    }
}
