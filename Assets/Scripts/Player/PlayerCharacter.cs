using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class PlayerCharacter : MonoBehaviour
{
    [Header("Combat")]
    public float atkRange = 3f;
    public float targetRefreshInterval = .2f;
    public Transform target { get; private set; }


    [HideInInspector] public NavMeshAgent agent;
    [HideInInspector] public IAttackBehavior attackBehavior;
    private RangeSensor rangeSensor;

    // States
    private PlayerFSM fsm;
    public IdleState idleState;
    public MoveState moveState;
    public AttackState attackState;

    private float nextTargetRefreshTime;

    private UnitInstance unit;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;

        attackBehavior = GetComponent<IAttackBehavior>();

        fsm = new PlayerFSM();
        idleState = new IdleState(this, fsm);
        moveState = new MoveState(this, fsm);
        attackState = new AttackState(this, fsm);

        if (rangeSensor == null)
            rangeSensor = GetComponentInChildren<RangeSensor>(true);

        unit = GetComponent<UnitInstance>();
        unit.OnStarChanged += HandleUnitChanged;

        ApplyUnitStats();
    }

    private void Start()
    {
        nextTargetRefreshTime = Time.time;
        fsm.ChangeState(idleState);
    }

    private void Update()
    {
        fsm.Update();
    }

    private void OnDestroy()
    {
        if (unit != null)
        {
            unit.OnStarChanged -= HandleUnitChanged;
        }
    }

    private void HandleUnitChanged(UnitInstance _)
    {
        ApplyUnitStats();
    }

    private void ApplyUnitStats()
    {
        if (unit == null || unit.Data == null)
            return;

        SetAttackRange(unit.Stats.range);

        // 공격 속도, 공격력 등 IAttackBehavior 과 연동 필요
    }

    public void SetAttackRange(float newRange)
    {
        atkRange = newRange;
        rangeSensor.SetRadius(atkRange);
    }

    public void SetTarget(Transform newTarget) => target = newTarget;
    public void ClearTarget() => target = null;

    public bool HasValidTarget()
    {
        if (target == null)
            return false;

        var m = target.GetComponent<Monster>();

        if (m == null || !m.IsAlive)
            return false;

        return true;
    }

    public bool IsTargetInRange(Transform candidate)
    {
        if (candidate == null)
            return false;

        float distSqr = (candidate.position - transform.position).sqrMagnitude;

        return distSqr <= atkRange * atkRange;
    }

    // 사거리 안 후보군에서 가장 가까운 몬스터를 target 으로 설정 (Idle, Attack 에서만 사용)
    public bool AcquireTargetInRange(bool forceRefresh = false)
    {
        if (!forceRefresh && HasValidTarget() && IsTargetInRange(target)) // 타겟이 이미 유효하고 사거리 안이면 유지
            return true;

        if (!forceRefresh && Time.time < nextTargetRefreshTime)
            return HasValidTarget() && IsTargetInRange(target);

        nextTargetRefreshTime = Time.time + targetRefreshInterval;


        target = GetClosestEnemyInRange();
        rangeSensor.CleanupDeadOrNull();

        return target != null;
    }

    public Transform GetClosestEnemyInRange()
    {
        float closestDistSqr = float.PositiveInfinity;

        Transform best = null;

        rangeSensor.CleanupDeadOrNull();

        foreach (var enemy in rangeSensor.InRange)
        {
            if (enemy == null || !enemy.IsAlive)
                continue;

            float distSqr = (enemy.transform.position - transform.position).sqrMagnitude;
            if (distSqr < closestDistSqr)
            {
                closestDistSqr = distSqr;
                best = enemy.transform;
            }
        }

        return best;
    }

}
