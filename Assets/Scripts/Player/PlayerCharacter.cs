using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class PlayerCharacter : MonoBehaviour
{
    [Header("Combat")]
    public float targetRefreshInterval = .2f;
    public MonsterController Target { get; private set; }


    [HideInInspector] public NavMeshAgent agent;
    [HideInInspector] public IAttackBehavior attackBehavior;
    private RangeSensor rangeSensor;
    // States
    private PlayerFSM fsm;
    public IdleState idleState;
    public MoveState moveState;
    public AttackState attackState;

    public UnitInstance unit;
    public float AttackPerSec => unit.Stats.attackPerSec;

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

        unit = GetComponent<UnitInstance>();
        unit.OnStarChanged += HandleUnitChanged;
    }

    private void Start()
    {
        fsm.ChangeState(idleState);
    }

    private void Update()
    {
        if (!unit.IsAlive)
            return;

        fsm.Update();
    }

    private void OnDestroy()
    {
        if (unit != null)
        {
            unit.OnStarChanged -= HandleUnitChanged;
        }
    }

    private void HandleUnitChanged(UnitInstance _)
    {
        ApplyUnitStats();
    }

    private void ApplyUnitStats()
    {
        if (unit == null || unit.Data == null)
        {
            Debug.LogWarning("Unit Null");
            return;
        }

        SetDetectionRange(unit.Stats.attackRange);

        // 공격 속도, 공격력 등 IAttackBehavior 과 연동 필요
    }

    public void SetDetectionRange(float newRange)
    {
        rangeSensor.SetRadius(newRange);
    }

    public void SetTarget(MonsterController newTarget) => Target = newTarget;
    public void ClearTarget() => Target = null;

    public bool HasValidTarget()
    {
        return Target != null && !Target.Health.IsDead;
    }

    // 사거리 안 후보군에서 가장 가까운 몬스터를 target 으로 설정 (Idle, Attack 에서만 사용)
    public bool DetectTargetInRange()
    {
        Target = GetClosestEnemyInRange();
        return Target != null;
    }

    public MonsterController GetClosestEnemyInRange()
    {
        return rangeSensor.GetClosestAlive(transform.position);
    }

    public void EngageRequest()
    {
        if (unit == null || !unit.IsAlive)
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

    public bool FindGlobalAliveMonster()
    {
        MonsterController m = StageManager.Instance.MonsterSpawner.FindClosestAlive(transform.position);
        SetTarget(m);

        return HasValidTarget();
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
        Gizmos.DrawWireSphere(transform.position, unit.Stats.attackRange);
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
