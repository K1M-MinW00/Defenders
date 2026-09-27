using UnityEngine;

[DisallowMultipleComponent]
public sealed class MonsterTargetingController : MonoBehaviour
{
    private UnitRoster unitRoster;

    public UnitController CurrentTarget { get; private set; }

    public void Initialize(UnitRoster roster)
    {
        unitRoster = roster;
        ClearTarget();
    }

    public void ClearTarget()
    {
        CurrentTarget = null;
    }

    public void SetTarget(UnitController target)
    {
        CurrentTarget = target;
    }

    public bool HasValidTarget()
    {
        bool valid = CombatTargetSelector.IsValid(CurrentTarget);
        if (!valid)
            ClearTarget();

        return valid;
    }

    public bool TryAcquireClosest(Vector3 origin)
    {
        SetTarget(unitRoster != null ? unitRoster.FindClosestAlive(origin) : null);
        return HasValidTarget();
    }

    public bool IsCurrentTargetInRange(Vector3 origin, float range)
    {
        return CombatTargetSelector.IsWithinRange(CurrentTarget, origin, range);
    }
}
