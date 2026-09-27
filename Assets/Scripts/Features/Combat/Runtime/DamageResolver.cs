using System;
using UnityEngine;

public static class DamageResolver
{
    public static DamageResolution Resolve(
        DamageRequest request,
        float currentHp,
        bool targetIsDead,
        Func<float, float> modifier = null,
        Func<float> randomValueProvider = null)
    {
        if (targetIsDead || currentHp <= 0f)
            return DamageResolution.Rejected(request.Amount, DamageRejectReason.TargetAlreadyDead);

        if (!IsValidAmount(request.Amount))
            return DamageResolution.Rejected(request.Amount, DamageRejectReason.InvalidAmount);

        float offensiveAmount = ResolveCriticalDamage(
            request,
            randomValueProvider,
            out bool isCritical);

        float modifiedAmount = modifier != null ? modifier(offensiveAmount) : offensiveAmount;
        if (!IsValidAmount(modifiedAmount))
        {
            DamageRejectReason reason = modifiedAmount <= 0f
                ? DamageRejectReason.FullyPrevented
                : DamageRejectReason.InvalidAmount;

            return DamageResolution.Rejected(request.Amount, reason);
        }

        float appliedAmount = modifiedAmount < currentHp ? modifiedAmount : currentHp;
        return DamageResolution.Accepted(
            request.Amount,
            modifiedAmount,
            appliedAmount,
            appliedAmount >= currentHp,
            isCritical);
    }

    private static float ResolveCriticalDamage(
        DamageRequest request,
        Func<float> randomValueProvider,
        out bool isCritical)
    {
        isCritical = false;

        if (request.Origin != DamageOrigin.BasicAttack && request.Origin != DamageOrigin.Skill)
            return request.Amount;

        if (request.Source is not ICombatDamageSource source)
            return request.Amount;

        float chance = Mathf.Clamp01(source.CriticalChance);
        float multiplier = source.CriticalDamageMultiplier;
        if (chance <= 0f || multiplier <= 1f || float.IsNaN(multiplier) || float.IsInfinity(multiplier))
            return request.Amount;

        float roll = randomValueProvider != null ? randomValueProvider() : UnityEngine.Random.value;
        if (roll >= chance)
            return request.Amount;

        isCritical = true;
        return request.Amount * multiplier;
    }

    private static bool IsValidAmount(float amount)
    {
        return amount > 0f && !float.IsNaN(amount) && !float.IsInfinity(amount);
    }
}
