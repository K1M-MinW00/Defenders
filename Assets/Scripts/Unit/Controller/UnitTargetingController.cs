using UnityEngine;

public class UnitTargetingController : MonoBehaviour
{
    [SerializeField] private RangeSensor rangeSensor;

    private UnitController owner;
    private MonsterSpawner monsterSpawner;
    private MonsterController currentTarget;

    public MonsterController CurrentTarget => currentTarget;

    public void Initialize(UnitController owner)
    {
        this.owner = owner;

        if (rangeSensor == null)
            rangeSensor = GetComponentInChildren<RangeSensor>();
    }

    public void BindSpawner(MonsterSpawner spawner)
    {
        monsterSpawner = spawner;
    }

    public void ClearTarget()
    {
        currentTarget = null;
    }

    public bool HasValidTarget()
    {
        bool valid = currentTarget != null && !currentTarget.Health.IsDead;
       
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

    public MonsterController GetClosestEnemyInRange()
    {
        if (rangeSensor == null)
            return null;

        return rangeSensor.GetClosestAlive(transform.position);
    }

    public bool FindGlobalAliveMonster()
    {
        if (monsterSpawner == null)
            return false;

        currentTarget = monsterSpawner.FindClosestAlive(transform.position);
        return HasValidTarget();
    }

    public void RefreshTargetIfCloserInRange()
    {
        MonsterController closest = GetClosestEnemyInRange();
        if (closest == null || closest == currentTarget)
            return;

        currentTarget = closest;
    }

    public bool IsTargetInRange()
    {
        if (!HasValidTarget())
            return false;

        float distSqr = (currentTarget.transform.position - transform.position).sqrMagnitude;
        float range = owner.Runtime.FinalStats.DetectRange;
        return distSqr <= range * range;
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