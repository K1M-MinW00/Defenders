using System;

public static class DamageResolver
{
    public static DamageResolution Resolve(
        DamageRequest request,
        float currentHp,
        bool targetIsDead,
        Func<float, float> modifier = null)
    {
        if (targetIsDead || currentHp <= 0f)
            return DamageResolution.Rejected(request.Amount, DamageRejectReason.TargetAlreadyDead);

        if (!IsValidAmount(request.Amount))
            return DamageResolution.Rejected(request.Amount, DamageRejectReason.InvalidAmount);

        float modifiedAmount = modifier != null ? modifier(request.Amount) : request.Amount;
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
            appliedAmount >= currentHp);
    }

    private static bool IsValidAmount(float amount)
    {
        return amount > 0f && !float.IsNaN(amount) && !float.IsInfinity(amount);
    }
}
