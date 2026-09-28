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
        combatHealth = null;

        if (collider == null || ((1 << collider.gameObject.layer) & targetLayer.value) == 0)
            return false;

        if (!collider.TryGetComponent(out ICombatTarget target))
            return false;

        return TryResolve(target, damagedTargets, out combatHealth);
    }

    public static bool TryResolve(
        ICombatTarget target,
        ISet<ICombatHealth> damagedTargets,
        out ICombatHealth combatHealth)
    {
        combatHealth = null;

        if (!CombatTargetSelector.IsValid(target))
            return false;

        combatHealth = target.CombatHealth;
        return damagedTargets == null || damagedTargets.Add(combatHealth);
    }
}
