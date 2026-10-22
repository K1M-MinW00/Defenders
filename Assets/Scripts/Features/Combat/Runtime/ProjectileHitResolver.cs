using System.Collections.Generic;
using UnityEngine;

public static class ProjectileHitResolver
{
    public static bool TryResolve(
        Collider2D collider,
        LayerMask targetLayer,
        ISet<ICombatHealth> damagedTargets,
        out ICombatHealth combatHealth)
    {
        return CombatHitResolver.TryResolve(
            collider,
            targetLayer,
            damagedTargets,
            out combatHealth);
    }

    public static bool TryResolve(
        Collider2D collider,
        LayerMask targetLayer,
        ISet<ICombatHealth> damagedTargets,
        out ICombatTarget target,
        out ICombatHealth combatHealth)
    {
        target = null;
        combatHealth = null;

        if (collider == null || ((1 << collider.gameObject.layer) & targetLayer.value) == 0)
            return false;
        if (!collider.TryGetComponent(out target))
            return false;

        return CombatHitResolver.TryResolve(target, damagedTargets, out combatHealth);
    }

    public static bool TryResolve(
        ICombatTarget target,
        ISet<ICombatHealth> damagedTargets,
        out ICombatHealth combatHealth)
    {
        return CombatHitResolver.TryResolve(target, damagedTargets, out combatHealth);
    }
}
