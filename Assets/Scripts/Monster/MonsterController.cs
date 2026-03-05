using System;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(MonsterHealth))]
public class MonsterController : MonoBehaviour, IPoolable
{
    [SerializeField] private MonsterStats stats;

    [Header("AI")]
    [SerializeField] private float navSampleRadius = 1.0f;

    [Header("References")]
    [SerializeField] private UnitRoster unitRoster;
    [SerializeField] private MonoBehaviour attackBehaviour;

    public NavMeshAgent Agent { get; private set; }
    public UnitInstance TargetUnit { get; private set; }
    public IMonsterAttack Attack { get; private set; }
    public MonsterHealth Health { get; private set; }

    public float AttackRange => (stats != null) ? stats.atkRange : 0f;
    public float AttackCooldown => (stats != null && stats.atkCoolTime > 0f) ? (1f / stats.atkCoolTime) : 999f;
    public float AtkDamage => (stats != null) ? stats.atkDamage : 0f;

    private string poolKey;
    public string PoolKey => poolKey;


    public void SetPoolKey(string key) => poolKey = key;

    private MonsterFSM fsm;
    public MonsterMoveState moveState;
    public MonsterAttackState attackState;
    public MonsterIdleState idleState;

    public event Action<MonsterController> OnDead;

    private void Awake()
    {
        Agent = GetComponent<NavMeshAgent>();
        Agent.updateRotation = false;
        Agent.updateUpAxis = false;

        Attack = attackBehaviour as IMonsterAttack;

        if (Attack == null)
            Attack = GetComponent<IMonsterAttack>();

        Health = GetComponent<MonsterHealth>();
        Health.OnDead += HandleDead;

        fsm = new MonsterFSM();
        moveState = new MonsterMoveState(this, fsm);
        attackState = new MonsterAttackState(this, fsm);
        idleState = new MonsterIdleState(this, fsm);


        if (unitRoster == null)
            unitRoster = StageManager.Instance.UnitRoster;
    }

    private void Start()
    {
    }

    private void Update()
    {
        if (Health.IsDead)
            return;

        fsm.Update();
    }

    public void OnSpawn()
    {
        ApplyStats();

        TargetUnit = null;

        Agent.isStopped = false;
        Agent.ResetPath();

        Health.Initialize(stats);
        Health.ResetHealth();

        fsm.ChangeState(idleState);
    }

    public void OnDespawn()
    {
        TargetUnit = null;
        Agent.ResetPath();
        Agent.isStopped = true;
    }

    private void ApplyStats()
    {
        if (stats == null)
            return;

        Agent.speed = stats.moveSpeed;

        // TODO : 공격 관련 Attack Behaviour 에서 stats 참조하여 적용
    }


    private void HandleDead(MonsterHealth health)
    {
        StopMovement();
        OnDead?.Invoke(this);
    }


    // --- Targeting / Movement Helpers ---
    public bool IsTargetValid(UnitInstance u) => u != null && u.IsAlive;

    public void SetTarget(UnitInstance newTarget) => TargetUnit = newTarget;

    public UnitInstance FindClosestAliveUnit()
    {
        if (unitRoster == null)
        {
            Debug.LogWarning("UnitRoster Null");
            return null;
        }

        return unitRoster.FindClosestAlive(transform.position);
    }

    public void MoveTo(Vector3 targetPos)
    {
        if (Agent.isStopped)
            Agent.isStopped = false;

        if (NavMesh.SamplePosition(targetPos, out NavMeshHit hit, navSampleRadius, NavMesh.AllAreas))
            Agent.SetDestination(hit.position);
    }

    public void StopMovement()
    {
        Agent.isStopped = true;
        Agent.velocity = Vector3.zero;
        Agent.ResetPath();
    }

    // TODO : 기즈모 따로 분리
#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying)
            return;

        if (Agent == null)
            return;

        DrawNavMeshPath();
        DrawTargetGizmo();
    }

    private void DrawNavMeshPath()
    {
        if (!Agent.hasPath)
            return;

        NavMeshPath path = Agent.path;
        if (path == null || path.corners.Length < 2)
            return;

        Gizmos.color = Color.cyan;

        for (int i = 0; i < path.corners.Length - 1; i++)
        {
            Gizmos.DrawLine(path.corners[i], path.corners[i + 1]);
        }

        // 코너 포인트 표시
        Gizmos.color = Color.blue;
        foreach (var corner in path.corners)
        {
            Gizmos.DrawSphere(corner, 0.1f);
        }
    }

    private void DrawTargetGizmo()
    {
        if (TargetUnit == null)
            return;

        // 타겟 라인
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, TargetUnit.transform.position);

        // 공격 사거리
        Gizmos.color = new Color(1f, 0f, 0f, 0.2f);
        Gizmos.DrawWireSphere(transform.position, AttackRange);
    }

#endif
}
