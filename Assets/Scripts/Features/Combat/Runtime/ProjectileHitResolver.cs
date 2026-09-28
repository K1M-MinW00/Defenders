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
        ICombatTarget target,
        ISet<ICombatHealth> damagedTargets,
        out ICombatHealth combatHealth)
    {
        return CombatHitResolver.TryResolve(target, damagedTargets, out combatHealth);
    }
}
