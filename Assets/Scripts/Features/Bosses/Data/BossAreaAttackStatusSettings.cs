using System;
using UnityEngine;

[Serializable]
public sealed class BossAreaAttackStatusSettings
{
    [Header("Slow")]
    public bool enableSlow;
    [Min(0f)] public float slowDuration = 3f;
    [Range(0f, 1f)] public float slowRatio = 0.35f;

    [Header("Stun")]
    public bool enableStun;
    [Min(0f)] public float stunDuration = 1.25f;

    [Header("Poison")]
    public bool enablePoison;
    [Min(0f)] public float poisonDuration = 5f;
    [Min(0.05f)] public float poisonTickInterval = 1f;
    [Min(0f)] public float poisonDamageMultiplier = 0.15f;

    [Header("Burn")]
    public bool enableBurn;
    [Min(0f)] public float burnDuration = 3f;
    [Min(0.05f)] public float burnTickInterval = 0.5f;
    [Min(0f)] public float burnDamageMultiplier = 0.1f;

    [Header("Defense Down")]
    public bool enableDefenseDown;
    [Min(0f)] public float defenseDownDuration = 5f;
    [Range(0f, 90f)] public float defenseDownPercentagePoints = 20f;

    public int EnabledEffectCount =>
        (enableSlow ? 1 : 0) +
        (enableStun ? 1 : 0) +
        (enablePoison ? 1 : 0) +
        (enableBurn ? 1 : 0) +
        (enableDefenseDown ? 1 : 0);

    public bool TryValidate(out string error)
    {
        if (enableSlow && (slowDuration <= 0f || slowRatio <= 0f))
        {
            error = "Enabled slow requires positive duration and ratio.";
            return false;
        }

        if (enableStun && stunDuration <= 0f)
        {
            error = "Enabled stun requires a positive duration.";
            return false;
        }

        if (enablePoison &&
            (poisonDuration <= 0f || poisonTickInterval <= 0f || poisonDamageMultiplier <= 0f))
        {
            error = "Enabled poison requires positive duration, interval, and damage multiplier.";
            return false;
        }

        if (enableBurn &&
            (burnDuration <= 0f || burnTickInterval <= 0f || burnDamageMultiplier <= 0f))
        {
            error = "Enabled burn requires positive duration, interval, and damage multiplier.";
            return false;
        }

        if (enableDefenseDown &&
            (defenseDownDuration <= 0f || defenseDownPercentagePoints <= 0f))
        {
            error = "Enabled defense down requires positive duration and percentage points.";
            return false;
        }

        error = string.Empty;
        return true;
    }
}
