using UnityEngine;

public static class SkillOnHitStatusEffectResolver
{
    public static bool TryBuildApplication(
        SkillOnHitStatusEffectSettings settings,
        ICombatTarget source,
        float sourceAttack,
        float chanceRoll,
        out StatusEffectApplication application)
    {
        application = default;
        if (settings == null || !settings.enabled || settings.duration <= 0f)
            return false;

        float chance = Mathf.Clamp01(settings.chance);
        if (chance <= 0f || (chance < 1f && Mathf.Clamp01(chanceRoll) >= chance))
            return false;

        float magnitude = settings.magnitudeMode == SkillStatusMagnitudeMode.SourceAttackRatio
            ? Mathf.Max(0f, sourceAttack) * Mathf.Max(0f, settings.magnitude)
            : Mathf.Max(0f, settings.magnitude);

        application = new StatusEffectApplication(
            settings.type,
            settings.duration,
            magnitude,
            settings.tickInterval,
            source);
        return application.IsValid;
    }
}
