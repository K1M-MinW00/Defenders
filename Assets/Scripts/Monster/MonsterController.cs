using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class MonsterController : MonoBehaviour, IDamageable, IPoolable
{
    private enum State { Idle, Move, Attack}

    [Header("Stats")]
    [SerializeField] private float maxHp = 50f;
    [SerializeField] private float moveSpeed = 3.5f;
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float attackPerSec = 1.0f;
    [SerializeField] private float atkDamage = 5f;

    [Header("AI")]
    [SerializeField] private float retargetInterval = .25f;
    [SerializeField] private float navSampleRadius = 1.0f;

    [Header("References")]
    [SerializeField] private UnitRoster unitRoster;
    [SerializeField] private MonoBehaviour attackBehaviour;

    public float CurrentHp { get;private set; }
    public bool IsDead { get; private set; }
    public float AttackRange => attackRange;
    public float AttackCooldown => attackPerSec <= 0f ? 999f : (1f / attackPerSec);
    public float AtkDamage => atkDamage;

    private string poolKey;
    public string PoolKey => poolKey;

    public NavMeshAgent Agent { get; private set; }
    public IMonsterAttack Attack { get;private set; }
    public UnitInstance TargetUnit { get; private set; }

    private float lastAttackTime;
    private State state;

    private Coroutine aiLoop;

    public event Action<MonsterController> OnDead;

    private void Awake()
    {
        Agent = GetComponent<NavMeshAgent>();
        Agent.updateRotation = false;
        Agent.updateUpAxis = false;


        Attack = attackBehaviour as IMonsterAttack;
        if (Attack == null)
            Attack = GetComponent<IMonsterAttack>();

    }

    private void Start()
    {
        if (unitRoster == null)
            unitRoster = StageManager.Instance.UnitRoster;
    }

    public void SetPoolKey(string key) => poolKey = key;

    public void OnSpawn()
    {
        IsDead = false;
        CurrentHp = maxHp;

        TargetUnit = null;
        lastAttackTime = -999f;
        state = State.Idle;

        Agent.speed = moveSpeed;
        Agent.isStopped = false;
        Agent.ResetPath();

        if(aiLoop != null)
            StopCoroutine(aiLoop);

        aiLoop = StartCoroutine(AILoop());
    }

    public void OnDespawn()
    {
        if(aiLoop != null)
        {
            StopCoroutine(aiLoop);
            aiLoop = null;
        }

        TargetUnit = null;
        Agent.ResetPath() ;
        Agent.isStopped = true;
    }

    private IEnumerator AILoop()
    {
        var wait = new WaitForSeconds(retargetInterval);

        while(!IsDead)
        {
            Tick();
            yield return wait;
        }
    }

    private void Tick()
    {
        if(!IsTargetValid(TargetUnit))
        {
            TargetUnit = FindClosestAliveUnit();
            state = TargetUnit != null ? State.Move : State.Idle;
        }
        else
        {
            UnitInstance candidate = FindClosestAliveUnit();
            if(candidate != null && candidate != TargetUnit)
            {
                float curd = (TargetUnit.transform.position - transform.position).sqrMagnitude;
                float cand = (candidate.transform.position - transform.position).sqrMagnitude;

                if (cand <= curd )
                    TargetUnit = candidate;
            }
        }

        if(TargetUnit == null)
        {
            StopMovement();
            state = State.Idle;
            return;
        }

        bool inRange = (TargetUnit.transform.position - transform.position).sqrMagnitude <= attackRange * attackRange;

        if (inRange)
        {
            StopMovement();
            state = State.Attack;

            TryAttack();
        }
        else
        {
            state = State.Move;
            MoveTo(TargetUnit.transform.position);
        }
    }

    private void TryAttack()
    {
        if (Time.time - lastAttackTime < AttackCooldown)
            return;

        lastAttackTime = Time.time;

        Attack.Execute(this);
    }

    private bool IsTargetValid(UnitInstance u) => u != null && u.IsAlive;

    private UnitInstance FindClosestAliveUnit()
    {
        if(unitRoster == null)
            return null;

        return unitRoster.FindClosestAlive(transform.position);
    }

    private void MoveTo(Vector3 targetPos)
    {
        if (Agent.isStopped)
            Agent.isStopped = false;

        if (NavMesh.SamplePosition(targetPos, out NavMeshHit hit, navSampleRadius, NavMesh.AllAreas))
            Agent.SetDestination(hit.position);
    }

    private void StopMovement()
    {
        if(!Agent.isStopped)
        {
            Agent.isStopped = true;
            Agent.velocity = Vector3.zero;
        }
    }

    public void TakeDamage(float damage)
    {
        if (IsDead)
            return;

        if (damage <= 0f)
            return;

        CurrentHp -= damage;
        DamageUIService.Instance?.Show(transform.position, damage);

        if (CurrentHp <= 0f)
            Die();
    }

    public void Kill() => Die();
    private void Die()
    {
        if (IsDead)
            return;

        IsDead = true;
        StopMovement();
        OnDead?.Invoke(this);
    }

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
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
#endif
}
