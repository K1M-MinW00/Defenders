using System;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class UnitController : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private UnitDataSO unitData;
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
    #region Property
    public UnitDataSO UnitData => unitData;
    public StageUnitRuntime Runtime => runtime;
    public UnitHealth Health => health;
    public UnitEnergy Energy => energy;
    public UnitRoster UnitRoster => unitRoster;
    public UnitTargetingController Targeting => targeting;
    public UnitMovementController Movement => movement;
    public UnitAnimationController Animation => anim;
    public UnitCombatController Combat => combat;
    public UnitFSMController FSMController => fsmController;
    public UnitSkillController SkillController => skillController;
    public UnitStatService StatService => statService;
    public UnitBuffController BuffController => buffController;
    public MonsterController Target => targeting.CurrentTarget;
    public UnitCode UnitCode => runtime.UnitCode;
    public int Star => runtime.Star;
     
    public float Attack => runtime.FinalStats.Attack;
    public float AttackPerSec => runtime.FinalStats.AttackPerSec;
    public float DetectRange => runtime.FinalStats.DetectRange;

    public bool IsDead => Health.IsDead;
  
    #endregion Property

    public event Action<UnitController> OnInitialized;
    public event Action<UnitController> OnStatsChanged;

    private void Awake()
    {
        CacheComponents();
    }

    private void Update()
    {
        if (runtime == null || IsDead)
            return;

        energy.Tick(Time.deltaTime);
        fsmController.Tick();
    }

    public void BindCombatContext(MonsterSpawner spawner, UnitRoster roster)
    {
        targeting.BindSpawner(spawner);
        unitRoster = roster;
    }

    public void Initialize(StageUnitInitData initData)
    {
        unitData = initData.UnitData;
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
        RestoreForPrepare();

        OnInitialized?.Invoke(this);

        fsmController.ChangeToIdle();
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
        if(skillController == null) skillController = GetComponent<UnitSkillController>();
        if (buffController == null) buffController = GetComponent<UnitBuffController>();
    }

    public void RestoreForPrepare()
    {
        health.RestoreFull();
        energy.ConsumeAll();

        movement.Stop();
        movement.EnableMovement(true);

        targeting.ClearTarget();
        targeting.EnableSensor(true);
        
        combat.CancelAttack();
        skillController.CancelSkill();

        targeting.ApplyRange(runtime.FinalStats.DetectRange);

        fsmController.ChangeToIdle();
    }

    public void ReceiveCombatAlert()
    {
        if (IsDead)
            return;

        if (!fsmController.IsIdleState)
            return;

        bool found = Targeting.FindGlobalAliveMonster();
        if (!found)
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

    public void SetCombatPhase(bool active)
    {
        energy.SetCombatPhase(active);
        skillController.SetCombatPhase(active);
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

        FaceTarget();
        Movement.MoveTo(Target.transform.position);
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