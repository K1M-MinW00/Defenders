using UnityEngine;

public static class StatusEffectResistanceResolver
{
    private const float ImmunityThreshold = 0.999f;

    public static bool IsImmune(StatusEffectType type, BossStatusResistanceSettings resistance) =>
        Mathf.Clamp01(resistance?.Get(type) ?? 0f) >= ImmunityThreshold;

    public static bool TryResolve(
        StatusEffectApplication application,
        BossStatusResistanceSettings resistance,
        out StatusEffectApplication resolved)
    {
        float ratio = Mathf.Clamp01(resistance?.Get(application.Type) ?? 0f);
        if (!application.IsValid || IsImmune(application.Type, resistance))
        {
            resolved = default;
            return false;
        }

        float duration = application.Duration;
        float magnitude = application.Magnitude;
        if (application.Type == StatusEffectType.Poison || application.Type == StatusEffectType.Burn)
            magnitude *= 1f - ratio;
        else
            duration *= 1f - ratio;

        resolved = new StatusEffectApplication(
            application.Type,
            duration,
            magnitude,
            application.TickInterval,
            application.Source);
        return resolved.IsValid && duration > 0f;
    }
}
