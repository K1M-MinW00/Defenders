using System.Collections.Generic;
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
    public IAttackBehavior attackBehavior;

    [Header("Sensors")]
    [SerializeField] private RangeSensor rangeSensor;

    // States
    private PlayerFSM fsm;
    public IdleState idleState;
    public MoveState moveState;
    public AttackState attackState;

    private float nextTargetRefreshTime;

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

        if (rangeSensor != null)
        {
            rangeSensor.SetRadius(atkRange);

            rangeSensor.OnExited += OnEnemyExitedRange;
            rangeSensor.OnEntered += OnEnemyEnteredRange;
        }
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

    public void SetAttackRange(float newRange)
    {
        atkRange = newRange;

        if (rangeSensor != null)
            rangeSensor.SetRadius(atkRange);
    }

    public bool HasValidTarget()
    {
        if (target == null)
            return false;

        var m = target.GetComponent<Monster>();

        if (m == null || m.IsDead)
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

    public bool SearchTarget(bool forceRefresh = false)
    {
        if (!forceRefresh && HasValidTarget() && IsTargetInRange(target))
            return true;

        if (!forceRefresh && Time.time < nextTargetRefreshTime)
            return HasValidTarget();

        nextTargetRefreshTime = Time.time + targetRefreshInterval;

        if (rangeSensor == null)
        {
            target = null;
            return false;
        }

        rangeSensor.CleanupDeadOrNull();
        target = FindClosestEnemyInRange();

        return target != null;
    }

    private Transform FindClosestEnemyInRange()
    {
        if (rangeSensor == null)
            return null;

        float closestDistSqr = float.PositiveInfinity;

        Transform best = null;

        foreach (var enemy in rangeSensor.InRange)
        {
            if (enemy == null || enemy.IsDead)
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

    public void ClearTarget()
    {
        target = null;
    }

    private void OnEnemyExitedRange(Monster monster)
    {
        if (target != null && monster != null && monster.transform == target)
            target = null;
    }

    private void OnEnemyEnteredRange(Monster monster)
    {
        if (target == null && monster != null && !monster.IsDead)
            target = monster.transform;
    }
}
