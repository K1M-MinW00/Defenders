using System;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class UnitController : MonoBehaviour, IDamageable
{
    [Header("Data")]
    [SerializeField] private UnitDataSO unitData;

    [Header("Runtime")]
    [SerializeField] private StageUnitRuntime runtime;

    [Header("References")]
    [SerializeField] private ModelView view;
    [SerializeField] private RangeSensor rangeSensor;
    [SerializeField] private UnitRangeIndicator rangeIndicator;

   //  [SerializeField] private UnitSkillController skillController;
    [SerializeField] private MonoBehaviour attackBehaviorSource;

    private IAttackBehavior attackBehavior;
    private MonsterSpawner monsterSpawner;
    private NavMeshAgent agent;

    public NavMeshAgent Agent => agent;

    [Header("Combat")]
    [SerializeField] private float targetRefreshInterval = 0.2f;
    public float TargetRefreshInterval => targetRefreshInterval;

    [Header("FSM")]
    private UnitFSM fsm;
    public IdleState idleState;
    public MoveState moveState;
    public AttackState attackState;
    public SkillState skillState;
    public DeadState deadState;

    public MonsterController Target { get; private set; }

    public UnitDataSO UnitData => unitData;
    public StageUnitRuntime Runtime => runtime;

    public UnitCode UnitCode => runtime.UnitCode;
    public int Level => runtime.Level;
    public int Promotion => runtime.Promotion;
    public int LimitBreak => runtime.LimitBreak;
    public int Star => runtime.Star;

    public float Attack => runtime.FinalStats.Attack;
    public float MaxHp => runtime.FinalStats.MaxHp;
    public float CurrentHp => runtime.CurrentHp;
    public float AttackPerSec => runtime.FinalStats.AttackPerSec;
    public float DetectRange => runtime.FinalStats.DetectRange;
    public float CritChance => runtime.FinalStats.CritChance;
    public float EnergyRecovery => runtime.FinalStats.EnergyRecovery;
    public float CurrentEnergy => runtime.CurrentEnergy;
    public float MaxEnergy => runtime.MaxEnergy;

    public bool IsDead => runtime == null || runtime.IsDead;
    public bool IsAlive => runtime != null && !runtime.IsDead;
    public bool IsEnergyFull => runtime != null && runtime.CurrentEnergy >= runtime.MaxEnergy;

    public bool CanUsePassive => runtime != null && runtime.CanUsePassive;
    public bool CanUseActive => runtime != null && runtime.CanUseActive;
    public bool CanRecoverEnergy => runtime != null && runtime.CanRecoverEnergy;

    public bool ActiveTier2Unlocked => runtime != null && runtime.ActiveTier2Unlocked;
    public bool PassiveTier3Unlocked => runtime != null && runtime.PassiveTier3Unlocked;
    public bool ActiveTier4Unlocked => runtime != null && runtime.ActiveTier4Unlocked;

    public event Action<UnitController> OnInitialized;
    public event Action<UnitController> OnStatsChanged;
    public event Action<UnitController, float, float> OnHpChanged;
    public event Action<UnitController> OnEnergyChanged;
    public event Action<UnitController> OnDead;
    public event Action<UnitController, int> OnStarChanged;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        if (view == null)
            view = GetComponentInChildren<ModelView>();

        if (rangeSensor == null)
            rangeSensor = GetComponentInChildren<RangeSensor>();

        if (attackBehaviorSource != null)
            attackBehavior = attackBehaviorSource as IAttackBehavior;

        if (attackBehavior == null)
            attackBehavior = GetComponent<IAttackBehavior>();

        if (agent != null)
        {
            agent.updateRotation = false;
            agent.updateUpAxis = false;
        }

        fsm = new UnitFSM();
        idleState = new IdleState(this, fsm);
        moveState = new MoveState(this, fsm);
        attackState = new AttackState(this, fsm);
        skillState = new SkillState(this, fsm);
        deadState = new DeadState(this, fsm);
    }

    private void Update()
    {
        if (runtime == null || IsDead)
            return;

        TickEnergy();
        fsm?.Update();
    }

    public void BindCombatContext(MonsterSpawner spawner)
    {
        monsterSpawner = spawner;
    }

    public void Initialize(StageUnitInitData initData)
    {
        if (initData == null || initData.UnitData == null || initData.UserData == null)
        {
            Debug.LogError($"{name} - Initialize failed. initData is invalid.");
            return;
        }

        unitData = initData.UnitData;
        runtime = new StageUnitRuntime(initData);

        RecalculateStats(resetHp: true);
        RestoreForPrepare();

        // TODO : SKILL CONTROLLER
        // skillController?.Initialize(this);

        OnInitialized?.Invoke(this);
        OnStatsChanged?.Invoke(this);

        fsm.ChangeState(idleState);
    }

    private void TickEnergy()
    {
        if (!CanRecoverEnergy)
            return;

        if (IsEnergyFull)
            return;

        AddEnergy(EnergyRecovery * Time.deltaTime);
    }

    public void RecalculateStats(bool resetHp = false)
    {
        if (unitData == null || runtime == null)
            return;

        runtime.RefreshStageFlags();
        runtime.FinalStats = UnitStatCalculator.Calculate(unitData, runtime);

        if (resetHp)
            runtime.CurrentHp = runtime.FinalStats.MaxHp;
        else
            runtime.CurrentHp = Mathf.Min(runtime.CurrentHp, runtime.FinalStats.MaxHp);

        ApplyStats();

        OnStatsChanged?.Invoke(this);
        OnHpChanged?.Invoke(this, runtime.CurrentHp, runtime.FinalStats.MaxHp);
        OnEnergyChanged?.Invoke(this);
    }

    public void RestoreForPrepare()
    {
        if (runtime == null)
            return;

        runtime.IsDead = false;
        runtime.CurrentHp = runtime.FinalStats.MaxHp;
        runtime.CurrentEnergy = 0f;

        ClearTarget();

        if (rangeSensor != null)
            rangeSensor.enabled = true;

        ApplyStats();

        OnStatsChanged?.Invoke(this);
        OnHpChanged?.Invoke(this, runtime.CurrentHp, runtime.FinalStats.MaxHp);
        OnEnergyChanged?.Invoke(this);
    }

    public void ApplyStarUp()
    {
        if (runtime == null)
            return;

        if (runtime.Star >= 4)
            return;

        runtime.Star++;
        runtime.RefreshStageFlags();

        RecalculateStats(resetHp: true);

        OnStarChanged?.Invoke(this, runtime.Star);
    }

    private void ApplyStats()
    {
        if (runtime == null)
            return;

        if (agent != null)
        {
            if (!agent.enabled)
                agent.enabled = true;

            agent.isStopped = false;
        }

        if (rangeSensor != null)
            rangeSensor.SetRadius(runtime.FinalStats.DetectRange);
    }

    public bool CanEnterSkillState()
    {
        if (IsDead)
            return false;

        if (!CanUseActive)
            return false;

        if (!IsEnergyFull)
            return false;

        return true;
               //skillController != null;
    }

    public void AddEnergy(float amount)
    {
        if (runtime == null || IsDead || !CanRecoverEnergy || amount <= 0f)
            return;

        runtime.CurrentEnergy = Mathf.Clamp(runtime.CurrentEnergy + amount, 0f, runtime.MaxEnergy);
        OnEnergyChanged?.Invoke(this);
    }

    public void ConsumeAllEnergy()
    {
        if (runtime == null)
            return;

        runtime.CurrentEnergy = 0f;
        OnEnergyChanged?.Invoke(this);
    }

    public void TakeDamage(float damage)
    {
        if (runtime == null || IsDead || damage <= 0f)
            return;

        runtime.CurrentHp = Mathf.Max(0f, runtime.CurrentHp - damage);
        OnHpChanged?.Invoke(this, runtime.CurrentHp, runtime.FinalStats.MaxHp);

        if (runtime.CurrentHp <= 0f)
            Die();
    }

    public void Heal(float amount)
    {
        if (runtime == null || IsDead || amount <= 0f)
            return;

        float nextHp = Mathf.Min(runtime.FinalStats.MaxHp, runtime.CurrentHp + amount);

        if (Mathf.Approximately(nextHp, runtime.CurrentHp))
            return;

        runtime.CurrentHp = nextHp;
        OnHpChanged?.Invoke(this, runtime.CurrentHp, runtime.FinalStats.MaxHp);
    }

    public void Die()
    {
        if (runtime == null || runtime.IsDead)
            return;

        runtime.IsDead = true;

        ClearTarget();
        StopMovement();
        attackBehavior?.CancelAttack();

        if (rangeSensor != null)
            rangeSensor.enabled = false;

        view?.PlayDie();
        fsm.ChangeState(deadState);

        OnDead?.Invoke(this);
    }

    public void CancelAttack()
    {
        attackBehavior?.CancelAttack();
    }

    public void StopMovement()
    {
        if (agent == null || !agent.enabled)
            return;

        agent.isStopped = true;
        agent.ResetPath();
        agent.enabled = false;
    }

    public void ResumeMovement()
    {
        if (agent == null)
            return;

        if (!agent.enabled)
            agent.enabled = true;

        agent.isStopped = false;
    }

    public void MoveTo(Vector3 destination)
    {
        if (agent == null)
            return;

        if (!agent.enabled)
            agent.enabled = true;

        agent.isStopped = false;
        agent.SetDestination(destination);
    }

    public void PlayIdle() => view?.PlayIdle();
    public void PlayMove() => view?.PlayMove();
    public void PlayAttack() => view?.PlayAttack();
    public void PlaySkill() => view?.PlaySkill();

    public void FaceTarget()
    {
        if (!HasValidTarget())
            return;

        view?.FaceTo(transform.position, Target.transform.position);
    }

    public void SetTarget(MonsterController target) => Target = target;
    public void ClearTarget() => Target = null;

    public bool HasValidTarget()
    {
        return Target != null && !Target.Health.IsDead;
    }

    public bool TryFindTargetInSensor()
    {
        MonsterController closest = GetClosestEnemyInRange();
        SetTarget(closest);
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

        MonsterController monster = monsterSpawner.FindClosestAlive(transform.position);
        SetTarget(monster);

        return HasValidTarget();
    }

    public bool IsTargetInAttackRange()
    {
        if (!HasValidTarget())
            return false;

        float distSqr = (Target.transform.position - transform.position).sqrMagnitude;
        float range = runtime.FinalStats.DetectRange;
        return distSqr <= range * range;
    }

    public void MoveToCurrentTarget()
    {
        if (!HasValidTarget())
            return;

        FaceTarget();
        MoveTo(Target.transform.position);
    }

    public void TryAttackCurrentTarget()
    {
        if (!HasValidTarget())
            return;

        FaceTarget();
        attackBehavior?.TryAttack(Target);
    }

    public void RefreshTargetIfCloserInRange()
    {
        MonsterController closest = GetClosestEnemyInRange();
        if (closest == null || closest == Target)
            return;

        Target = closest;
    }

    public void EngageRequest()
    {
        if (!IsAlive)
            return;

        if (HasValidTarget())
            return;

        bool found = FindGlobalAliveMonster();

        if (found)
            fsm.ChangeState(moveState);
        else
        {
            ClearTarget();
            fsm.ChangeState(idleState);
        }
    }

    public Vector2 GetFacingDirection()
    {
        return view != null ? view.GetFacingDirection() : Vector2.right;
    }

    public void ShowRange()
    {
        if (rangeIndicator == null)
            return;

        rangeIndicator.Show(DetectRange);
    }

    public void HideRange()
    {
        rangeIndicator?.Hide();
    }
}