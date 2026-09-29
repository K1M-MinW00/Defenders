using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(MonsterHealth))]
[RequireComponent(typeof(MonsterTargetingController))]
public class MonsterController : MonoBehaviour, IPoolable, ICombatTarget, IKnockbackReceiver
{
    [Header("References")]
    private ModelView view;
    private NavMeshAgent agent;
    private ICombatAttack attackBehavior;
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
    public float IdleTargetAcquireInterval => Data?.IdleTargetAcquireInterval ?? 0.5f;
    public float MoveTargetRefreshInterval => Data?.MoveTargetRefreshInterval ?? 0.25f;
    public Transform TargetTransform => transform;
    public ICombatHealth CombatHealth => Health;
    public bool IsDead => Health != null && Health.IsDead;
    public bool IsControlLocked { get; private set; }

    public float GetInitialUpdateDelay(float interval)
    {
        return StaggeredUpdateSchedule.GetInitialDelay(GetInstanceID(), interval);
    }

    private Poolable poolable;

    private StateMachine fsm;
    private MonsterMoveState moveState;
    private MonsterAttackState attackState;
    private MonsterIdleState idleState;

    public event Action<MonsterController> OnDead;
    private Coroutine knockbackRoutine;


    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        if (view == null)
            view = GetComponentInChildren<ModelView>();

        if (attackBehavior == null)
            attackBehavior = GetComponent<ICombatAttack>();

        poolable = GetComponent<Poolable>();

        Health = GetComponent<MonsterHealth>();
        targeting = GetComponent<MonsterTargetingController>();
        if (targeting == null)
            targeting = gameObject.AddComponent<MonsterTargetingController>();
        Health.OnDead += HandleDead;

        agent.updateRotation = false;
        agent.updateUpAxis = false;

        fsm = new StateMachine();
        moveState = new MonsterMoveState(this);
        attackState = new MonsterAttackState(this);
        idleState = new MonsterIdleState(this);
    }

    public void Initialize(UnitRoster unitRoster, MonsterDataSO data, StagePoolManager poolManager)
    {
        if (data == null)
        {
            Debug.LogError($"[{nameof(MonsterController)}] Initialize failed: data is null.", this);
            return;
        }

        Data = data;
        FinalStats = MonsterStatCalculator.Calculate(data);
        this.poolManager = poolManager;
        targeting.Initialize(unitRoster);

        ApplyStats();
        ChangeToIdle();
    }

    private void Update()
    {
        if (Health.IsDead || IsControlLocked)
            return;

        fsm.Tick();
    }

    public void OnSpawn()
    {
        ResetRuntimeState(clearContext: false);
    }

    public void OnDespawn()
    {
        ResetRuntimeState(clearContext: true);
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

        poolable?.ReturnToPool();
    }

    private void ResetRuntimeState(bool clearContext)
    {
        StopKnockback();
        fsm.Reset();
        attackBehavior?.CancelAttack();
        targeting.ClearTarget();
        StopMovement();

        if (!clearContext)
            return;

        Health.ClearRuntimeListeners();
        OnDead = null;
        Data = null;
        FinalStats = null;
        poolManager = null;
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
        if (agent == null || !agent.enabled || !agent.isOnNavMesh)
            return;

        agent.enabled = true;
        agent.isStopped = false;
    }
    public void StopMovement()
    {
        if (agent == null || !agent.enabled || !agent.isOnNavMesh)
            return;

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

    public bool TryAttackCurrentTarget()
    {
        if (!HasValidTarget())
            return false;

        FaceTarget();
        return attackBehavior?.TryAttack(Target) ?? false;
    }

    public void CancelAttack()
    {
        attackBehavior?.CancelAttack();
    }

    public void FaceTarget()
    {
        if (!HasValidTarget())
            return;

        view?.FaceTo(transform.position, Target.transform.position);
    }

    public void PlayIdle() => view?.PlayIdle();
    public void PlayMove() => view?.PlayMove();
    public void PlayAttack() => view?.PlayAttack(FinalStats?.atkPerSec ?? 1f);

    public void ChangeToIdle() => fsm.ChangeState(idleState);
    public void ChangeToMove() => fsm.ChangeState(moveState);
    public void ChangeToAttack() => fsm.ChangeState(attackState);

    public void ApplyKnockback(Vector2 direction, float distance, float duration)
    {
        if (IsDead || direction.sqrMagnitude <= 0.0001f || distance <= 0f)
            return;

        StopKnockback();

        IsControlLocked = true;
        fsm.Reset();
        CancelAttack();
        StopMovement();

        knockbackRoutine = StartCoroutine(KnockbackRoutine(
            direction,
            distance,
            Mathf.Max(0.01f, duration)));
    }

    private void StopKnockback()
    {
        if (knockbackRoutine == null)
            return;

        StopCoroutine(knockbackRoutine);
        knockbackRoutine = null;
        IsControlLocked = false;
    }

    private IEnumerator KnockbackRoutine(Vector2 direction, float distance, float duration)
    {
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

        if (agent != null && agent.enabled && agent.isOnNavMesh)
        {
            agent.Warp(transform.position);
            agent.ResetPath();
            agent.isStopped = false;
        }

        knockbackRoutine = null;
        IsControlLocked = false;
        ResumeBehaviorAfterControlEffect();
    }

    private void ResumeBehaviorAfterControlEffect()
    {
        if (IsDead || !gameObject.activeInHierarchy)
            return;

        if (!HasValidTarget() && !TryFindClosestAliveUnit())
        {
            ChangeToIdle();
            return;
        }

        if (IsTargetInAttackRange())
            ChangeToAttack();
        else
            ChangeToMove();
    }
}
