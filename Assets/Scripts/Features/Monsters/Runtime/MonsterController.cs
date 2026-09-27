using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(MonsterHealth))]
[RequireComponent(typeof(MonsterTargetingController))]
public class MonsterController : MonoBehaviour, IPoolable, ICombatTarget
{
    [Header("References")]
    private ModelView view;
    private NavMeshAgent agent;
    private IMonsterAttack attackBehavior;
    private StagePoolManager poolManager;
    private MonsterTargetingController targeting;

    public MonsterDataSO Data { get; private set; }
    public MonsterStats FinalStats { get; private set; }
    public NavMeshAgent Agent => agent;
    public UnitController Target => targeting != null ? targeting.CurrentTarget : null;
    public MonsterHealth Health { get; private set; }
    public StagePoolManager PoolManager => poolManager;
    public float AttackRange => FinalStats.atkRange;
    public float AttackCooldown => 1f / Mathf.Max(0.01f, FinalStats.atkPerSec);
    public float AtkDamage => FinalStats.atkDamage;
    public Transform TargetTransform => transform;
    public ICombatHealth CombatHealth => Health;
    public bool IsDead => Health != null && Health.IsDead;

    private Poolable poolable;

    private MonsterFSM fsm;
    public MonsterMoveState moveState;
    public MonsterAttackState attackState;
    public MonsterIdleState idleState;

    public event Action<MonsterController> OnDead;
    private Coroutine knockbackRoutine;


    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        if (view == null)
            view = GetComponentInChildren<ModelView>();

        if (attackBehavior == null)
            attackBehavior = GetComponent<IMonsterAttack>();

        poolable = GetComponent<Poolable>();

        Health = GetComponent<MonsterHealth>();
        targeting = GetComponent<MonsterTargetingController>();
        if (targeting == null)
            targeting = gameObject.AddComponent<MonsterTargetingController>();
        Health.OnDead += HandleDead;

        agent.updateRotation = false;
        agent.updateUpAxis = false;

        fsm = new MonsterFSM();
        moveState = new MonsterMoveState(this, fsm);
        attackState = new MonsterAttackState(this, fsm);
        idleState = new MonsterIdleState(this, fsm);
    }

    public void Initialize(UnitRoster unitRoster, MonsterDataSO data, StagePoolManager poolManager)
    {
        Data = data;
        FinalStats = MonsterStatCalculator.Calculate(data);
        this.poolManager = poolManager;
        targeting.Initialize(unitRoster);

        ApplyStats();
    }

    private void Update()
    {
        if (Health.IsDead)
            return;

        fsm.Update();
    }

    public void OnSpawn()
    {
        ApplyStats();
        fsm.ChangeState(idleState);
    }

    public void OnDespawn()
    {
        targeting.ClearTarget();
        StopMovement();
    }

    private void ApplyStats()
    {
        if (FinalStats == null)
            return;

        Health.Initialize(FinalStats);
        agent.speed = FinalStats.moveSpeed;
    }


    private void HandleDead(MonsterHealth health)
    {
        OnDead?.Invoke(this);

        poolable.ReturnToPool();
    }


    // --- Targeting / Movement Helpers ---
    public void ClearTarget() => targeting.ClearTarget();
    public void SetTarget(UnitController newTarget) => targeting.SetTarget(newTarget);
    public bool HasValidTarget() => targeting.HasValidTarget();

    public bool TryFindClosestAliveUnit()
    {
        return targeting.TryAcquireClosest(transform.position);
    }

    public void MoveTo(Vector3 dest)
    {
        if (agent == null || !agent.enabled)
            return;

        ResumeMovement();
        agent.SetDestination(dest);
    }

    public void MoveToTarget()
    {
        if (!HasValidTarget())
            return;

        Vector3 velocity = agent.velocity;
        view.FaceDirection(velocity);
        MoveTo(Target.transform.position);
    }

    public void ResumeMovement()
    {
        agent.enabled = true;
        agent.isStopped = false;
    }
    public void StopMovement()
    {
        agent.isStopped = true;
        agent.velocity = Vector3.zero;
        agent.ResetPath();
    }

    public bool IsTargetInAttackRange()
    {
        if (!HasValidTarget())
            return false;

        return targeting.IsCurrentTargetInRange(transform.position, AttackRange);
    }

    public void TryAttackCurrentTarget()
    {
        if (!HasValidTarget())
            return;

        FaceTarget();
        attackBehavior?.TryAttack(Target);
    }

    public void FaceTarget()
    {
        if (!HasValidTarget())
            return;

        view?.FaceTo(transform.position, Target.transform.position);
    }

    public void PlayIdle() => view?.PlayIdle();
    public void PlayMove() => view?.PlayMove();
    public void PlayAttack() => view?.PlayAttack();

    public void ApplyKnockback(Vector2 direction, float distance, float duration)
    {
        if (knockbackRoutine != null)
            StopCoroutine(knockbackRoutine);

        knockbackRoutine = StartCoroutine(KnockbackRoutine(direction, distance, duration));
    }

    private IEnumerator KnockbackRoutine(Vector2 direction, float distance, float duration)
    {
        if (agent != null)
            agent.isStopped = true;

        Vector3 start = transform.position;
        Vector3 end = start + (Vector3)(direction.normalized * distance);

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            transform.position = Vector3.Lerp(start, end, t);
            yield return null;
        }

        transform.position = end;

        if (agent != null)
        {
            agent.Warp(transform.position);
            agent.ResetPath();
            agent.isStopped = false;
        }

        knockbackRoutine = null;
    }
}
