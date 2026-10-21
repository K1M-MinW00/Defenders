using System;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(MonsterHealth))]
[RequireComponent(typeof(MonsterTargetingController))]
[RequireComponent(typeof(MonsterFSMController))]
[RequireComponent(typeof(MonsterMovementController))]
[RequireComponent(typeof(MonsterCombatController))]
[RequireComponent(typeof(MonsterPresentationController))]
public class MonsterController : MonoBehaviour, IPoolable, ICombatTarget, IKnockbackReceiver
{
    private StagePoolManager poolManager;
    private MonsterTargetingController targeting;
    private MonsterFSMController fsmController;
    private MonsterMovementController movementController;
    private MonsterCombatController combatController;
    private MonsterPresentationController presentationController;

    public MonsterDataSO Data { get; private set; }
    public MonsterStats FinalStats { get; private set; }
    public NavMeshAgent Agent => movementController != null ? movementController.Agent : null;
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
    public bool IsControlLocked => movementController != null && movementController.IsControlLocked;
    public bool IsRuntimeInitialized { get; private set; }
    public bool CanRunBehavior => IsRuntimeInitialized &&
                                  Data != null &&
                                  !IsDead &&
                                  !IsControlLocked &&
                                  gameObject.activeInHierarchy;

    public float GetInitialUpdateDelay(float interval)
    {
        return StaggeredUpdateSchedule.GetInitialDelay(GetInstanceID(), interval);
    }

    private Poolable poolable;

    public event Action<MonsterController> OnDead;


    private void Awake()
    {
        poolable = GetComponent<Poolable>();

        Health = GetComponent<MonsterHealth>();
        targeting = GetComponent<MonsterTargetingController>();
        fsmController = GetComponent<MonsterFSMController>();
        movementController = GetComponent<MonsterMovementController>();
        combatController = GetComponent<MonsterCombatController>();
        presentationController = GetComponent<MonsterPresentationController>();
        Health.OnDead += HandleDead;
        movementController.Initialize(this);
        fsmController.Initialize(this);
        combatController.Initialize(this);
        presentationController.Initialize();
    }

    public bool Initialize(
        UnitRoster unitRoster,
        MonsterDataSO data,
        StagePoolManager poolManager,
        MonsterStats runtimeStats = null)
    {
        if (data == null)
        {
            Debug.LogError($"[{nameof(MonsterController)}] Initialize failed: data is null.", this);
            return false;
        }

        if (unitRoster == null || poolManager == null)
        {
            Debug.LogError(
                $"[{nameof(MonsterController)}] Initialize failed: combat context is incomplete.",
                this);
            return false;
        }

        Data = data;
        FinalStats = runtimeStats != null
            ? MonsterStatCalculator.Calculate(runtimeStats)
            : MonsterStatCalculator.Calculate(data);
        this.poolManager = poolManager;
        targeting.Initialize(unitRoster);

        Health.OnDead -= HandleDead;
        Health.OnDead += HandleDead;

        ApplyStats();
        IsRuntimeInitialized = true;
        ChangeToIdle();
        return true;
    }

    private void Update()
    {
        if (!IsRuntimeInitialized || Health.IsDead || IsControlLocked)
            return;

        fsmController.Tick();
    }

    public void OnSpawn()
    {
        ResetRuntimeState();
    }

    public void OnDespawn()
    {
        ResetRuntimeState();
    }

    private void ApplyStats()
    {
        if (FinalStats == null)
            return;

        Health.Initialize(FinalStats);
        movementController.ApplyMoveSpeed(FinalStats.moveSpeed);
    }

    public void ApplyStageMoveSpeedMultiplier(float multiplier)
    {
        if (movementController == null || FinalStats == null)
            return;

        movementController.ApplyMoveSpeed(FinalStats.moveSpeed * Mathf.Max(0f, multiplier));
    }


    private void HandleDead(MonsterHealth health)
    {
        OnDead?.Invoke(this);

        poolable?.ReturnToPool();
    }

    public void SuspendForResultPresentation()
    {
        if (!IsRuntimeInitialized || IsDead)
            return;

        movementController.Suspend();
        fsmController.ResetRuntime();
        CancelAttack();
        ClearTarget();
        StopMovement();
        PlayIdle();
    }

    private void ResetRuntimeState()
    {
        IsRuntimeInitialized = false;
        fsmController.ResetRuntime();
        combatController.ResetRuntime();
        targeting.ClearTarget();
        movementController.ResetRuntime();

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
        movementController.MoveTo(dest);
    }

    public void MoveToTarget()
    {
        if (!HasValidTarget())
            return;

        Vector3 velocity = Agent != null ? Agent.velocity : Vector3.zero;
        presentationController.FaceMovement(velocity);
        MoveTo(Target.transform.position);
    }

    public void ResumeMovement()
    {
        movementController.TryResume();
    }
    public void StopMovement()
    {
        movementController.Stop();
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
        return combatController.TryAttack(Target);
    }

    public void CancelAttack()
    {
        combatController.CancelAttack();
    }

    public void FaceTarget()
    {
        if (!HasValidTarget())
            return;

        presentationController.FaceTarget(transform.position, Target.transform);
    }

    public void PlayIdle() => presentationController.PlayIdle();
    public void PlayMove() => presentationController.PlayMove();
    public void PlayAttack() => presentationController.PlayAttack(FinalStats?.atkPerSec ?? 1f);

    public void ChangeToIdle()
    {
        if (CanRunBehavior)
            fsmController.ChangeToIdle();
    }

    public void ChangeToMove()
    {
        if (CanRunBehavior)
            fsmController.ChangeToMove();
    }

    public void ChangeToAttack()
    {
        if (CanRunBehavior)
            fsmController.ChangeToAttack();
    }

    public void ApplyKnockback(Vector2 direction, float distance, float duration)
    {
        movementController.ApplyKnockback(direction, distance, duration);
    }

    internal void PrepareForControlEffect()
    {
        fsmController.ResetRuntime();
        CancelAttack();
        StopMovement();
    }

    internal void ResumeBehaviorAfterControlEffect()
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
