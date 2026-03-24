using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(UnitRuntime))]
[RequireComponent(typeof(NavMeshAgent))]
public class PlayerCharacter : MonoBehaviour
{
    [Header("References")]
    private ModelView view;
    private RangeSensor rangeSensor;
    private MonsterSpawner monsterSpawner;

    private IAttackBehavior attackBehavior;
    private NavMeshAgent agent;
    private UnitRuntime runtime;

    [Header("Combat")]
    private float targetRefreshInterval = .2f;
    private float energyPerSec = 1f;

    // States
    private PlayerFSM fsm;
    public IdleState idleState;
    public MoveState moveState;
    public AttackState attackState;
    public DeadState deadState;

    public MonsterController Target { get; private set; }
    public UnitRuntime Runtime => runtime;
    public float TargetRefreshInterval => targetRefreshInterval;

    public float MoveSpeed => runtime.FinalStats.speed;
    public float Atk => runtime.FinalStats.atk;
    public float AttackPerSec => runtime.FinalStats.attackPerSec;

    public bool IsDead => runtime == null || runtime.IsDead;


    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        runtime = GetComponent<UnitRuntime>();

        if (view == null)
            view = GetComponentInChildren<ModelView>();

        if (rangeSensor == null)
            rangeSensor = GetComponentInChildren<RangeSensor>(true);

        if (attackBehavior == null)
            attackBehavior = GetComponent<IAttackBehavior>();


        agent.updateRotation = false;
        agent.updateUpAxis = false;

        fsm = new PlayerFSM();
        idleState = new IdleState(this, fsm);
        moveState = new MoveState(this, fsm);
        attackState = new AttackState(this, fsm);
        deadState = new DeadState(this, fsm);


        runtime.OnStatsChanged += HandleStatsChanged;
        runtime.OnDead += HandleDead;
    }

    public void BindCombatContext(MonsterSpawner monsterSpawner)
    {
        this.monsterSpawner = monsterSpawner;
    }

    private void Start()
    {
        ApplyStats();
        fsm.ChangeState(idleState);
    }

    private void Update()
    {
        if (IsDead)
            return;

        TickEnergy();
        fsm.Update();
    }

    private void OnDestroy()
    {
        if (runtime != null)
        {
            runtime.OnStatsChanged -= HandleStatsChanged;
            runtime.OnDead -= HandleDead;
        }
    }

    private void TickEnergy()
    {
        if (runtime.IsEnergyFull)
            return;

        runtime.AddMp(energyPerSec * Time.deltaTime);
    }


    private void HandleStatsChanged(UnitRuntime _)
    {
        ApplyStats();
    }
    private void HandleDead(UnitRuntime _)
    {
        ClearTarget();
        StopMovement();
        attackBehavior?.CancelAttack();

        if (agent != null && agent.enabled == true)
        {
            agent.ResetPath();
            agent.isStopped = true;
            agent.enabled = false;
        }

        if (rangeSensor != null)
            rangeSensor.enabled = false;

        view?.PlayDie();
        fsm.ChangeState(deadState);
    }

    public void CancelAttack()
    {
        attackBehavior?.CancelAttack();
    }

    private void ApplyStats()
    {
        if (runtime == null || runtime.Data == null)
        {
            Debug.LogWarning("Unit Null");
            return;
        }

        if (agent != null)
        {
            agent.enabled = true;
            agent.isStopped = false;
            agent.speed = MoveSpeed;
        }

        if (rangeSensor != null)
            rangeSensor.SetRadius(runtime.FinalStats.detectRange);

        // 공격 속도, 공격력 등 IAttackBehavior 과 연동 필요

        view?.PlayIdle();
        fsm.ChangeState(idleState);
    }

    public void StopMovement()
    {
        if (agent == null || agent.enabled == false)
            return;

        agent.isStopped = true;
        agent.ResetPath();
        agent.enabled = false;
    }

    public void ResumeMovement()
    {
        agent.enabled = true;
        agent.isStopped = false;
    }

    public void MoveTo(Vector3 dest)
    {
        if (agent == null || !agent.enabled)
            return;

        ResumeMovement();
        agent.SetDestination(dest);
    }

    public void PlayIdle() => view?.PlayIdle();
    public void PlayMove() => view?.PlayMove();
    public void PlayAttack() => view?.PlayAttack();

    public void FaceTarget()
    {
        if (!HasValidTarget())
            return;

        view?.FaceTo(transform.position, Target.transform.position);
    }

    public void SetTarget(MonsterController newTarget) => Target = newTarget;
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
        MonsterController monster = monsterSpawner.FindClosestAlive(transform.position);
        SetTarget(monster);

        return HasValidTarget();
    }

    public bool IsTargetInAttackRange()
    {
        if (!HasValidTarget())
            return false;

        float distSqr = (Target.transform.position - transform.position).sqrMagnitude;
        float range = runtime.FinalStats.attackRange;

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
        if (runtime == null || !runtime.IsAlive)
            return;

        if (HasValidTarget())
            return;

        bool found = FindGlobalAliveMonster();

        if (found)
        {
            fsm.ChangeState(moveState);
        }
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
}
