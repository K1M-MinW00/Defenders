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
    private MonsterFSMController fsmController;
    private MonsterMovementController movementController;
    private MonsterCombatController combatController;
    private MonsterPresentationController presentationController;
    private BossController bossController;
    private MonsterStatusEffectController statusEffectController;
    private MonsterStatusEffectBarView statusEffectBar;
    private float attackDamageMultiplier = 1f;
    private float attackSpeedMultiplier = 1f;
    private float stageMoveSpeedMultiplier = 1f;
    private float statusMoveSpeedMultiplier = 1f;

    public MonsterDataSO Data { get; private set; }
    public MonsterStats FinalStats { get; private set; }
    public NavMeshAgent Agent => movementController != null ? movementController.Agent : null;
    public MonsterTargetingController Targeting { get; private set; }
    public MonsterHealth Health { get; private set; }
    public StagePoolManager PoolManager => poolManager;
    public UnitRoster UnitRoster { get; private set; }
    public MonsterSpawner Spawner { get; private set; }
    public MonsterStatusEffectController StatusEffects => statusEffectController;
    public float AttackRange => FinalStats.atkRange;
    public float AttackCooldown => 1f / Mathf.Max(0.01f, FinalStats.atkPerSec * attackSpeedMultiplier);
    public float AtkDamage => FinalStats.atkDamage * attackDamageMultiplier;
    public float IdleTargetAcquireInterval => Data?.IdleTargetAcquireInterval ?? 0.5f;
    public float MoveTargetRefreshInterval => Data?.MoveTargetRefreshInterval ?? 0.25f;
    public Transform TargetTransform => transform;
    public ICombatHealth CombatHealth => Health;
    public bool IsDead => Health != null && Health.IsDead;
    public bool IsControlLocked =>
        (movementController != null && movementController.IsControlLocked) ||
        (statusEffectController != null && statusEffectController.IsStunned);
    public bool IsRuntimeInitialized { get; private set; }
    public bool IsUsingAbility => fsmController != null && fsmController.IsUsingAbility;
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
        Targeting = GetComponent<MonsterTargetingController>();
        fsmController = GetComponent<MonsterFSMController>();
        movementController = GetComponent<MonsterMovementController>();
        combatController = GetComponent<MonsterCombatController>();
        presentationController = GetComponent<MonsterPresentationController>();
        bossController = GetComponent<BossController>();
        statusEffectController = GetComponent<MonsterStatusEffectController>() ??
                                 gameObject.AddComponent<MonsterStatusEffectController>();
        statusEffectBar = GetComponentInChildren<MonsterStatusEffectBarView>(true) ??
                          MonsterStatusEffectBarView.Create(transform);
        Health.OnDead += HandleDead;
        movementController.Initialize(this);
        fsmController.Initialize(this);
        combatController.Initialize(this);
        presentationController.Initialize();
    }

    public bool TryInitialize(
        UnitRoster unitRoster,
        MonsterDataSO data,
        StagePoolManager poolManager,
        MonsterStats runtimeStats = null,
        MonsterSpawner spawner = null)
    {
        if (data == null)
        {
            Debug.LogError($"[{nameof(MonsterController)}] TryInitialize failed: data is null.", this);
            return false;
        }

        if (unitRoster == null || poolManager == null)
        {
            Debug.LogError(
                $"[{nameof(MonsterController)}] TryInitialize failed: combat context is incomplete.",
                this);
            return false;
        }

        Data = data;
        FinalStats = runtimeStats != null
            ? MonsterStatCalculator.Calculate(runtimeStats)
            : MonsterStatCalculator.Calculate(data);
        this.poolManager = poolManager;
        UnitRoster = unitRoster;
        Spawner = spawner;
        Targeting.Initialize(unitRoster);
        presentationController.ApplyVisualScale(data.VisualScale);

        ConfigureBossRuntime(data);
        statusEffectController.Initialize(this);
        statusEffectBar.ConfigureHeight(data.Classification);
        statusEffectBar.Bind(statusEffectController);

        Health.OnDead -= HandleDead;
        Health.OnDead += HandleDead;

        ApplyStats();
        IsRuntimeInitialized = true;
        bossController?.Initialize(this);
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
        stageMoveSpeedMultiplier = Mathf.Max(0f, multiplier);
        RefreshMoveSpeed();
    }

    public void SetStatusMoveSpeedMultiplier(float multiplier)
    {
        statusMoveSpeedMultiplier = Mathf.Clamp01(multiplier);
        RefreshMoveSpeed();
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
        Targeting.ClearTarget();
        StopMovement();
        PlayIdle();
    }

    private void ResetRuntimeState()
    {
        IsRuntimeInitialized = false;
        bossController?.ResetRuntime();
        statusEffectBar?.Bind(null);
        statusEffectController?.ResetRuntime();
        fsmController.ResetRuntime();
        combatController.ResetRuntime();
        Targeting.ResetRuntime();
        movementController.ResetRuntime();

        Health.ClearTransientListeners();
        OnDead = null;
        Data = null;
        FinalStats = null;
        poolManager = null;
        UnitRoster = null;
        Spawner = null;
        attackDamageMultiplier = 1f;
        attackSpeedMultiplier = 1f;
        stageMoveSpeedMultiplier = 1f;
        statusMoveSpeedMultiplier = 1f;
        presentationController?.ResetVisualScale();
    }

    private void ConfigureBossRuntime(MonsterDataSO data)
    {
        if (data.Classification != MonsterClassification.Boss)
        {
            bossController?.Configure(null);
            return;
        }

        BossShieldController shield = GetComponent<BossShieldController>();
        if (shield == null)
            shield = gameObject.AddComponent<BossShieldController>();

        bossController = GetComponent<BossController>();
        if (bossController == null)
            bossController = gameObject.AddComponent<BossController>();

        if (GetComponent<BossAreaAttackAbility>() == null)
            gameObject.AddComponent<BossAreaAttackAbility>();

        if (GetComponent<BossSummonAbility>() == null)
            gameObject.AddComponent<BossSummonAbility>();

        bossController.Configure(data.BossDefinition);
    }


    // --- Movement / Combat State Helpers ---
    public void MoveTo(Vector3 dest)
    {
        movementController.MoveTo(dest);
    }

    public void MoveToTarget()
    {
        if (!Targeting.HasValidTarget())
            return;

        Vector3 velocity = Agent != null ? Agent.velocity : Vector3.zero;
        presentationController.FaceMovement(velocity);
        MoveTo(Targeting.CurrentTarget.transform.position);
    }

    public void ResumeMovement()
    {
        movementController.TryResume();
    }
    public void StopMovement()
    {
        movementController.Stop();
    }

    public bool TryAttackCurrentTarget()
    {
        if (!Targeting.HasValidTarget())
            return false;

        FaceTarget();
        return combatController.TryAttack(Targeting.CurrentTarget);
    }

    public void CancelAttack()
    {
        combatController.CancelAttack();
    }

    public void FaceTarget()
    {
        if (!Targeting.HasValidTarget())
            return;

        presentationController.FaceTarget(transform.position, Targeting.CurrentTarget.transform);
    }

    public void FacePosition(Vector3 position)
    {
        presentationController.FacePosition(transform.position, position);
    }

    public void PlayIdle() => presentationController.PlayIdle();
    public void PlayMove() => presentationController.PlayMove();
    public void PlayAttack() => presentationController.PlayAttack(
        (FinalStats?.atkPerSec ?? 1f) * attackSpeedMultiplier);

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

    public bool TryUseAbility(IMonsterAbility ability)
    {
        return fsmController != null && fsmController.TryUseAbility(ability);
    }

    public void CompleteCurrentAbility()
    {
        fsmController?.CompleteAbility();
    }

    public void SetCombatMultipliers(float damageMultiplier, float speedMultiplier)
    {
        attackDamageMultiplier = Mathf.Max(0f, damageMultiplier);
        attackSpeedMultiplier = Mathf.Max(0.01f, speedMultiplier);
    }

    public bool ApplyStatusEffect(StatusEffectApplication application) =>
        statusEffectController != null && statusEffectController.Apply(application);

    private void RefreshMoveSpeed()
    {
        if (movementController != null && FinalStats != null)
            movementController.ApplyMoveSpeed(
                FinalStats.moveSpeed * stageMoveSpeedMultiplier * statusMoveSpeedMultiplier);
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

        if (!Targeting.HasValidTarget() && !Targeting.TryAcquireClosest(transform.position))
        {
            ChangeToIdle();
            return;
        }

        if (Targeting.IsCurrentTargetInRange(transform.position, AttackRange))
            ChangeToAttack();
        else
            ChangeToMove();
    }
}
