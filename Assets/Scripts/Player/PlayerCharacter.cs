using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class PlayerCharacter : MonoBehaviour
{
    [Header("Combat")]
    public float atkRange = 3f;
    public float targetRefreshInterval = .2f;
    public Transform target { get; private set; }


    [HideInInspector] public NavMeshAgent agent;
    [HideInInspector] public IAttackBehavior attackBehavior;
    private RangeSensor rangeSensor;

    // States
    private PlayerFSM fsm;
    public IdleState idleState;
    public MoveState moveState;
    public AttackState attackState;

    private float nextTargetRefreshTime;

    private UnitInstance unit;

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

        ApplyUnitStats();
    }

    private void Start()
    {
        nextTargetRefreshTime = Time.time;
        fsm.ChangeState(idleState);
    }

    private void Update()
    {
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
            return;

        SetAttackRange(unit.Stats.range);

        // 공격 속도, 공격력 등 IAttackBehavior 과 연동 필요
    }

    public void SetAttackRange(float newRange)
    {
        atkRange = newRange;
        rangeSensor.SetRadius(atkRange);
    }

    public void SetTarget(Transform newTarget) => target = newTarget;
    public void ClearTarget() => target = null;

    public bool HasValidTarget()
    {
        if (target == null)
            return false;

        var m = target.GetComponent<MonsterController>();

        if (m == null || m.Health.IsDead)
            return false;

        return true;
    }

    public bool IsTargetInRange(Transform candidate)
    {
        if (candidate == null)
            return false;

        float distSqr = (candidate.position - transform.position).sqrMagnitude;

        return distSqr <= atkRange * atkRange;
    }

    // 사거리 안 후보군에서 가장 가까운 몬스터를 target 으로 설정 (Idle, Attack 에서만 사용)
    public bool AcquireTargetInRange(bool forceRefresh = false)
    {
        if (!forceRefresh && HasValidTarget() && IsTargetInRange(target)) // 타겟이 이미 유효하고 사거리 안이면 유지
            return true;

        if (!forceRefresh && Time.time < nextTargetRefreshTime)
            return HasValidTarget() && IsTargetInRange(target);

        nextTargetRefreshTime = Time.time + targetRefreshInterval;


        target = GetClosestEnemyInRange();
        rangeSensor.CleanupDeadOrNull();

        return target != null;
    }

    public Transform GetClosestEnemyInRange()
    {
        float closestDistSqr = float.PositiveInfinity;

        Transform best = null;

        rangeSensor.CleanupDeadOrNull();

        foreach (var enemy in rangeSensor.InRange)
        {
            if (enemy == null || enemy.Health.IsDead)
                continue;

            float distSqr = (enemy.transform.position - transform.position).sqrMagnitude;
            if (distSqr < closestDistSqr)
            {
                closestDistSqr = distSqr;
                best = enemy.transform;
            }
        }

        return best;
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

        // agent는 Awake에서 세팅되지만, 안전하게 보장
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
        // 공격 사거리(유닛 기준)
        Gizmos.color = new Color(0f, 1f, 0f, 0.25f);
        Gizmos.DrawWireSphere(transform.position, atkRange);
    }

    private void DrawTargetLine()
    {
        if (target == null)
            return;

        // 타겟이 유효하지 않으면 색 변경
        bool valid = HasValidTarget();
        Gizmos.color = valid ? Color.red : Color.gray;

        Gizmos.DrawLine(transform.position, target.position);

        // 타겟 위치 표시
        Gizmos.color = valid ? Color.yellow : Color.gray;
        Gizmos.DrawWireSphere(target.position, 0.25f);
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

        // pathStatus에 따라 색 구분
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

        // 코너 연결선
        for (int i = 0; i < path.corners.Length - 1; i++)
            Gizmos.DrawLine(path.corners[i], path.corners[i + 1]);

        // 코너 포인트
        Gizmos.color = Color.blue;
        for (int i = 0; i < path.corners.Length; i++)
            Gizmos.DrawSphere(path.corners[i], cornerSphereRadius);
    }

    private void DrawStateLabel()
    {
        // FSM 상태 이름을 SceneView에 표시
        // PlayerFSM에 CurrentState가 없다면 아래 라인은 안전하게 "Unknown" 처리됩니다.
        string stateName = "Unknown";
        try
        {
            // 아래는 흔한 패턴 예시: fsm.CurrentState.GetType().Name
            // 선생님 PlayerFSM 구현에 맞춰 한 줄만 조정하시면 됩니다.
            var curState = fsm?.CurrentState;
            if (curState != null)
                stateName = curState.GetType().Name;
        }
        catch { /* ignore */ }

        Handles.color = Color.white;
        Handles.Label(transform.position + Vector3.up * 0.8f, $"State: {stateName}\nTarget: {(target ? target.name : "None")}");
    }
#endif

}
