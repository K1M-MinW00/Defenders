using System.Collections.Generic;
using UnityEngine;

public static class SkillAreaDamageUtility
{
    public static int ApplyCircle(
        Vector2 center,
        float radius,
        ContactFilter2D filter,
        Collider2D[] hitBuffer,
        LayerMask targetLayer,
        HashSet<ICombatHealth> damagedTargets,
        float damage,
        ICombatTarget source)
    {
        if (!CanQuery(hitBuffer, damagedTargets, damage))
            return 0;

        int hitCount = Physics2D.OverlapCircle(center, radius, filter, hitBuffer);
        return ApplyHits(
            hitBuffer,
            hitCount,
            targetLayer,
            damagedTargets,
            damage,
            source);
    }

    public static int ApplyBox(
        Vector2 center,
        Vector2 size,
        float angle,
        ContactFilter2D filter,
        Collider2D[] hitBuffer,
        LayerMask targetLayer,
        HashSet<ICombatHealth> damagedTargets,
        float damage,
        ICombatTarget source)
    {
        if (!CanQuery(hitBuffer, damagedTargets, damage))
            return 0;

        int hitCount = Physics2D.OverlapBox(center, size, angle, filter, hitBuffer);
        return ApplyHits(
            hitBuffer,
            hitCount,
            targetLayer,
            damagedTargets,
            damage,
            source);
    }

    private static int ApplyHits(
        Collider2D[] hitBuffer,
        int hitCount,
        LayerMask targetLayer,
        HashSet<ICombatHealth> damagedTargets,
        float damage,
        ICombatTarget source)
    {
        damagedTargets.Clear();
        int appliedCount = 0;

        for (int i = 0; i < hitCount; i++)
        {
            if (!CombatHitResolver.TryResolve(
                    hitBuffer[i],
                    targetLayer,
                    damagedTargets,
                    out ICombatHealth combatHealth))
                continue;

            DamageResult result = combatHealth.ApplyDamage(
                new DamageRequest(damage, source, DamageOrigin.Skill));
            if (result.WasApplied)
                appliedCount++;
        }

        return appliedCount;
    }

    private static bool CanQuery(
        Collider2D[] hitBuffer,
        HashSet<ICombatHealth> damagedTargets,
        float damage)
    {
        return hitBuffer != null &&
               hitBuffer.Length > 0 &&
               damagedTargets != null &&
               damage > 0f;
    }
}
