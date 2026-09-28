using System.Collections.Generic;
using UnityEngine;

public static class CombatTargetSelector
{
    public static bool IsValid(ICombatTarget target)
    {
        if (target == null)
            return false;
        if (target is Object unityObject && unityObject == null)
            return false;

        Transform targetTransform = target.TargetTransform;
        if (targetTransform == null || !targetTransform.gameObject.activeInHierarchy)
            return false;

        return target.CombatHealth != null && !target.IsDead;
    }

    public static bool IsWithinRange(ICombatTarget target, Vector3 origin, float range)
    {
        if (!IsValid(target) || range < 0f)
            return false;

        return (target.TargetTransform.position - origin).sqrMagnitude <= range * range;
    }

    public static T FindClosest<T>(IEnumerable<T> candidates, Vector3 origin)
        where T : class, ICombatTarget
    {
        if (candidates == null)
            return null;

        T closest = null;
        float closestDistance = float.PositiveInfinity;

        foreach (T candidate in candidates)
        {
            if (!IsValid(candidate))
                continue;

            float distance = (candidate.TargetTransform.position - origin).sqrMagnitude;
            if (distance >= closestDistance)
                continue;

            closest = candidate;
            closestDistance = distance;
        }

        return closest;
    }

    public static T FindLowestHealth<T>(IEnumerable<T> candidates)
        where T : class, ICombatTarget
    {
        if (candidates == null)
            return null;

        T lowest = null;
        float lowestHealth = float.PositiveInfinity;

        foreach (T candidate in candidates)
        {
            if (!IsValid(candidate))
                continue;

            float currentHealth = candidate.CombatHealth.CurrentHp;
            if (currentHealth >= lowestHealth)
                continue;

            lowest = candidate;
            lowestHealth = currentHealth;
        }

        return lowest;
    }
}
