using System;
using UnityEngine;
using UnityEngine.AI;

[System.Serializable]
public enum UnitRuntimeState
{
    Despawned,
    AwaitingInitialization,
    Preparing,
    Combat,
    WaveEnded,
    Removing
}

public enum UnitRemovalReason
{
    None,
    Sold,
    Rerolled,
    Fused
}

[RequireComponent(typeof(NavMeshAgent))]
public class UnitController : MonoBehaviour, IPoolable, ICombatTarget, ICombatDamageSource
{
    [Header("Data")]
    [SerializeField] private UnitDataSO unitData;
    [SerializeField] private UserUnitData userData; 
    private StageUnitRuntime runtime;

    [Header("References")]
    [SerializeField] private UnitFSMController fsmController;
    [SerializeField] private UnitStatService statService;
    [SerializeField] private UnitHealth health;
    [SerializeField] private UnitEnergy energy;
    [SerializeField] private UnitTargetingController targeting;
    [SerializeField] private UnitMovementController movement;
    [SerializeField] private UnitAnimationController anim;
    [SerializeField] private UnitCombatController combat;
    [SerializeField] private UnitRangeIndicator rangeIndicator;
    [SerializeField] private UnitSkillController skillController;
    [SerializeField] private UnitBuffController buffController;
    private UnitRoster unitRoster;
    private StagePoolManager poolManager;
    private Poolable poolable;

    [Header("Runtime State")]
    [SerializeField] private UnitRuntimeState runtimeState = UnitRuntimeState.Despawned;
    [SerializeField] private UnitRemovalReason removalReason;
    [SerializeField] private bool isCombatAlerted;
    #region Property
    public UnitDataSO UnitData => unitData;
    public UserUnitData UserUnit => userData;
    public StageUnitRuntime Runtime => runtime;
    public UnitHealth Health => health;
    public UnitEnergy Energy => energy;
    public UnitRoster UnitRoster => unitRoster;
    public StagePoolManager PoolManager => poolManager;
    public UnitTargetingController Targeting => targeting;
    public UnitMovementController Movement => movement;
    public UnitAnimationController Animation => anim;
    public UnitCombatController Combat => combat;
    public UnitFSMController FSMController => fsmController;
    public UnitSkillController SkillController => skillController;
    public UnitStatService StatService => statService;
    public UnitBuffController BuffController => buffController;
    public ICombatTarget Target => targeting.CurrentTarget;
    public string UnitId => runtime.UnitId;
    public int Star => runtime.Star;
     
    public float Attack => runtime.FinalStats.Attack;
    public float AttackPerSec => runtime.FinalStats.AttackPerSec;
    public float DetectRange => runtime.FinalStats.DetectRange;
    public float CriticalChance => runtime?.FinalStats.CritChance ?? 0f;
    public float CriticalDamageMultiplier => runtime?.FinalStats.CritDamage ?? 1f;

    public bool IsDead => Health.IsDead;
    public bool IsCombatPhase { get; private set; }
    public bool IsCombatAlerted => isCombatAlerted;
    public UnitRuntimeState RuntimeState => runtimeState;
    public UnitRemovalReason RemovalReason => removalReason;
    public Transform TargetTransform => transform;
    public ICombatHealth CombatHealth => health;
  
    #endregion Property

    public event Action<UnitController> OnInitialized;
    public event Action<UnitController> OnStatsChanged;

    private void Awake()
    {
        CacheComponents();
    }

    private void Update()
    {
        if (runtime == null || RuntimeState == UnitRuntimeState.Removing || IsDead)
            return;

        energy.Tick(Time.deltaTime);

        if (IsCombatPhase)
            fsmController.Tick();
    }

    public void BindCombatContext(
        ICombatTargetProvider targetProvider,
        UnitRoster roster,
        StagePoolManager poolManager)
    {
        targeting.BindTargetProvider(targetProvider);
        unitRoster = roster;
        this.poolManager = poolManager;
    }

    public bool Initialize(StageUnitInitData initData)
    {
        CacheComponents();

        if (initData == null || initData.UnitData == null || initData.UserData == null)
        {
            Debug.LogError($"[{nameof(UnitController)}] Initialize failed: unit data is incomplete.", this);
            return false;
        }

        unitData = initData.UnitData;
        userData = initData.UserData;

        runtime = new StageUnitRuntime(initData);

        statService.Initialize(this);
        health.Initialize(this);
        energy.Initialize(this);
        targeting.Initialize(this);
        movement.Initialize(this);
        anim.Initialize(this);
        combat.Initialize(this);
        fsmController.Initialize(this);
        skillController.Initialize(this);
        buffController.Initialize(this);

        statService.BuildInitialStats(initData);
        EnterPreparation();

        OnInitialized?.Invoke(this);
        return true;
    }

    private void CacheComponents()
    {
        if (fsmController == null) fsmController = GetComponent<UnitFSMController>();
        if (statService == null) statService = GetComponent<UnitStatService>();
        if (health == null) health = GetComponent<UnitHealth>();
        if (energy == null) energy = GetComponent<UnitEnergy>();
        if (targeting == null) targeting = GetComponent<UnitTargetingController>();
        if (movement == null) movement = GetComponent<UnitMovementController>();
        if (anim == null) anim = GetComponent<UnitAnimationController>();
        if (combat == null) combat = GetComponent<UnitCombatController>();
        if (rangeIndicator == null) rangeIndicator = GetComponent<UnitRangeIndicator>();
        if (skillController == null) skillController = GetComponent<UnitSkillController>();
        if (buffController == null) buffController = GetComponent<UnitBuffController>();
        if (poolable == null) poolable = GetComponent<Poolable>();
    }

    public void EnterPreparation()
    {
        if (runtime == null || RuntimeState == UnitRuntimeState.Removing)
            return;

        SetCombatActive(false);
        isCombatAlerted = false;
        health.RestoreFull();
        energy.ConsumeAll();

        movement.Stop();
        movement.EnableMovement(true);

        targeting.ClearTarget();
        targeting.EnableSensor(true);
        
        combat.CancelAttack();
        skillController.CancelSkill();

        targeting.ApplyRange(runtime.FinalStats.DetectRange);

        anim.SetFacing(true);
        fsmController.ChangeToIdle();
        runtimeState = UnitRuntimeState.Preparing;
    }

    public void ReceiveCombatAlert()
    {
        if (IsDead || !IsCombatPhase)
            return;

        isCombatAlerted = true;

        if (!fsmController.IsIdleState || !Targeting.TryFindGlobalClosestTarget())
            return;

        if (Targeting.IsTargetInRange())
            FSMController.ChangeToAttack();
        else
            FSMController.ChangeToMove();
    }

    public void ApplyStarUp()
    {
        if (runtime == null)
            return;

        if (runtime.Star >= 4)
            return;

        runtime.UpgradeStar();

        statService.Recalculate(StatRefreshPolicy.FullHeal);
        OnStatsChanged?.Invoke(this);
    }

    public bool BeginCombat()
    {
        if (RuntimeState != UnitRuntimeState.Preparing || IsDead)
            return false;

        runtimeState = UnitRuntimeState.Combat;
        SetCombatActive(true);
        return true;
    }

    public void CompleteWave()
    {
        bool wasCombatPhase = RuntimeState == UnitRuntimeState.Combat;
        SetCombatActive(false);
        isCombatAlerted = false;

        if (wasCombatPhase)
            buffController.CompleteWave();

        combat.CancelAttack();
        movement.Stop();
        targeting.ClearTarget();
        targeting.EnableSensor(false);

        if (!IsDead)
            fsmController.ChangeToIdle();

        runtimeState = UnitRuntimeState.WaveEnded;
    }

    public bool TryBeginRemoval(UnitRemovalReason reason)
    {
        if (reason == UnitRemovalReason.None ||
            RuntimeState == UnitRuntimeState.Removing ||
            RuntimeState == UnitRuntimeState.Despawned)
            return false;

        SetCombatActive(false);
        isCombatAlerted = false;
        combat.CancelAttack();
        skillController.CancelSkill();
        movement.Stop();
        targeting.ClearTarget();
        targeting.EnableSensor(false);

        removalReason = reason;
        runtimeState = UnitRuntimeState.Removing;
        return true;
    }

    public void ReturnToPool()
    {
        if (RuntimeState != UnitRuntimeState.Removing)
            return;

        if (poolManager != null && poolable != null)
        {
            poolManager.Despawn(poolable);
            return;
        }

        Destroy(gameObject);
    }

    public void OnSpawn()
    {
        if (poolable == null)
            poolable = GetComponent<Poolable>();

        runtimeState = UnitRuntimeState.AwaitingInitialization;
        removalReason = UnitRemovalReason.None;
        isCombatAlerted = false;
    }

    public void OnDespawn()
    {
        SetCombatActive(false);
        isCombatAlerted = false;
        combat.CancelAttack();
        skillController.Shutdown();
        movement.Stop();
        targeting.ClearTarget();
        targeting.EnableSensor(false);
        buffController.ResetRuntime();
        fsmController.ResetRuntime();
        rangeIndicator.Hide();
        health.ClearRuntimeListeners();
        energy.ClearRuntimeListeners();

        runtime = null;
        unitData = null;
        userData = null;
        unitRoster = null;
        poolManager = null;
        OnInitialized = null;
        OnStatsChanged = null;
        runtimeState = UnitRuntimeState.Despawned;
    }

    private void SetCombatActive(bool active)
    {
        IsCombatPhase = active && !IsDead;
        energy.SetCombatPhase(IsCombatPhase);
        skillController.SetCombatPhase(IsCombatPhase);
    }

    public void FaceTarget()
    {
        if (!targeting.HasValidTarget())
            return;

        anim.FaceTarget(Target);
    }

    public void MoveToCurrentTarget()
    {
        if (!targeting.HasValidTarget())
            return;

        anim.FaceDirection(movement.MoveDirection);
        Movement.MoveTo(Target.TargetTransform.position);
    }
   
    public void ShowRange()
    {
        rangeIndicator.Show(DetectRange);
    }

    public void HideRange()
    {
        rangeIndicator.Hide();
    }
}
