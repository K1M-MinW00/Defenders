using UnityEngine;

public class UnitTargetingController : MonoBehaviour
{
    [SerializeField] private RangeSensor rangeSensor;

    private UnitController owner;
    private ICombatTarget currentTarget;

    public ICombatTarget CurrentTarget => currentTarget;

    public void Initialize(UnitController owner)
    {
        this.owner = owner;

        if (rangeSensor == null)
            rangeSensor = GetComponentInChildren<RangeSensor>();
    }

    public void ClearTarget()
    {
        currentTarget = null;
    }

    public bool HasValidTarget()
    {
        bool valid = CombatTargetSelector.IsValid(currentTarget);
       
        if (!valid)
            ClearTarget();

        return valid;
    }

    public bool TryFindTargetInSensor()
    {
        var closest = GetClosestEnemyInRange();
        currentTarget = closest;
        return HasValidTarget();
    }

    public ICombatTarget GetClosestEnemyInRange()
    {
        if (rangeSensor == null)
            return null;

        return rangeSensor.GetClosestAlive(transform.position);
    }

    public void RefreshTargetIfCloserInRange()
    {
        ICombatTarget closest = GetClosestEnemyInRange();
        if (closest == null || closest == currentTarget)
            return;

        currentTarget = closest;
    }

    public bool IsTargetInRange()
    {
        if (!HasValidTarget())
            return false;

        float range = owner.Runtime.FinalStats.DetectRange;
        return CombatTargetSelector.IsWithinRange(currentTarget, transform.position, range);
    }

    public void EnableSensor(bool enable)
    {
        if (rangeSensor != null)
            rangeSensor.enabled = enable;
    }

    public void ApplyRange(float detectRange)
    {
        if (rangeSensor != null)
            rangeSensor.SetRadius(detectRange);
    }
}
