using System.Collections.Generic;
using UnityEngine;

public static class BossAbilityTargetSelector
{
    public static BossAbilityTarget Select(
        IReadOnlyList<UnitController> units,
        Vector3 bossPosition,
        BossTargetSelection selection,
        float areaRadius)
    {
        return selection switch
        {
            BossTargetSelection.DensestUnitCluster => SelectDensest(units, areaRadius),
            BossTargetSelection.RandomAliveUnit => SelectRandom(units),
            _ => SelectNearest(units, bossPosition),
        };
    }

    private static BossAbilityTarget SelectNearest(IReadOnlyList<UnitController> units, Vector3 origin)
    {
        UnitController selected = null;
        float bestDistance = float.MaxValue;

        if (units != null)
        {
            foreach (UnitController unit in units)
            {
                if (!IsAlive(unit))
                    continue;

                float distance = (unit.transform.position - origin).sqrMagnitude;
                if (distance >= bestDistance)
                    continue;

                selected = unit;
                bestDistance = distance;
            }
        }

        return selected != null
            ? new BossAbilityTarget(selected, selected.transform.position)
            : default;
    }

    private static BossAbilityTarget SelectRandom(IReadOnlyList<UnitController> units)
    {
        if (units == null)
            return default;

        int aliveCount = 0;
        foreach (UnitController unit in units)
            if (IsAlive(unit))
                aliveCount++;

        if (aliveCount == 0)
            return default;

        int selectedIndex = Random.Range(0, aliveCount);
        foreach (UnitController unit in units)
        {
            if (!IsAlive(unit))
                continue;

            if (selectedIndex-- == 0)
                return new BossAbilityTarget(unit, unit.transform.position);
        }

        return default;
    }

    private static BossAbilityTarget SelectDensest(IReadOnlyList<UnitController> units, float radius)
    {
        UnitController selected = null;
        int bestCount = 0;
        float radiusSquared = Mathf.Max(0f, radius) * Mathf.Max(0f, radius);

        if (units != null)
        {
            foreach (UnitController candidate in units)
            {
                if (!IsAlive(candidate))
                    continue;

                int count = 0;
                foreach (UnitController other in units)
                {
                    if (IsAlive(other) &&
                        (other.transform.position - candidate.transform.position).sqrMagnitude <= radiusSquared)
                        count++;
                }

                if (count <= bestCount)
                    continue;

                selected = candidate;
                bestCount = count;
            }
        }

        return selected != null
            ? new BossAbilityTarget(selected, selected.transform.position)
            : default;
    }

    private static bool IsAlive(UnitController unit)
    {
        return unit != null && !unit.IsDead && unit.gameObject.activeInHierarchy;
    }
}
