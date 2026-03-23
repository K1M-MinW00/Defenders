using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(UnitRuntime))]
[RequireComponent(typeof(NavMeshAgent))]
public class PlayerCharacter : MonoBehaviour
{
    [Header("References")]
    private UnitView view;
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
            view = GetComponentInChildren<UnitView>();

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
        if (agent == null || agent.enabled == true)
            return;

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


    public void FaceTo(Vector3 targetPos)
    {
        view?.FaceTo(transform.position, targetPos);
    }

    public Vector2 GetFacingDirection()
    {
        return view != null ? view.GetFacingDirection() : Vector2.right;
    }

#if UNITY_EDITOR
    [Header("Debug Gizmos")]
    [SerializeField] private bool drawGizmos = true;
    [SerializeField] private bool drawAttackRange = true;
    [SerializeField] private bool drawTargetLine = true;
    [SerializeField] private bool drawNavMeshPath = true;
    [SerializeField] private bool drawStateLabel = true;
    [SerializeField] private float cornerSphereRadius = 0.08f;

    private void OnDrawGizmosSelected()
    {
        if (!drawGizmos)
            return;

        if (!Application.isPlaying)
            return;

        if (agent == null)
            agent = GetComponent<NavMeshAgent>();

        if (drawAttackRange)
            DrawAttackRange();

        if (drawTargetLine)
            DrawTargetLine();

        if (drawNavMeshPath)
            DrawAgentPath();

        if (drawStateLabel)
            DrawStateLabel();
    }

    private void DrawAttackRange()
    {
        Gizmos.color = new Color(0f, 1f, 0f, 0.25f);
        Gizmos.DrawWireSphere(transform.position, runtime.FinalStats.attackRange);
    }

    private void DrawTargetLine()
    {
        if (Target == null)
            return;

        bool valid = HasValidTarget();
        Gizmos.color = valid ? Color.red : Color.gray;

        Gizmos.DrawLine(transform.position, Target.transform.position);

        Gizmos.color = valid ? Color.yellow : Color.gray;
        Gizmos.DrawWireSphere(Target.transform.position, 0.25f);
    }

    private void DrawAgentPath()
    {
        if (agent == null)
            return;

        if (!agent.hasPath)
            return;

        var path = agent.path;
        if (path == null || path.corners == null || path.corners.Length < 2)
            return;

        switch (agent.pathStatus)
        {
            case NavMeshPathStatus.PathComplete:
                Gizmos.color = Color.cyan;
                break;
            case NavMeshPathStatus.PathPartial:
                Gizmos.color = Color.yellow;
                break;
            case NavMeshPathStatus.PathInvalid:
                Gizmos.color = Color.red;
                break;
        }

        for (int i = 0; i < path.corners.Length - 1; i++)
            Gizmos.DrawLine(path.corners[i], path.corners[i + 1]);

        Gizmos.color = Color.blue;
        for (int i = 0; i < path.corners.Length; i++)
            Gizmos.DrawSphere(path.corners[i], cornerSphereRadius);
    }

    private void DrawStateLabel()
    {
        string stateName = "Unknown";
        try
        {
            var curState = fsm?.CurrentState;
            if (curState != null)
                stateName = curState.GetType().Name;
        }
        catch { /* ignore */ }

        Handles.color = Color.white;
        Handles.Label(transform.position + Vector3.up * 0.8f, $"State: {stateName}\nTarget: {(Target ? Target.name : "None")}");
    }

#endif

}
