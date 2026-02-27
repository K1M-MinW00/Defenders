using System;
using System.Collections;
using UnityEditor.Tilemaps;
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
    [SerializeField] private float retargetHysteresisRatio = .8f;

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

        if (unitRoster == null)
            unitRoster = StageManager.Instance.UnitRoster;

        Attack = attackBehaviour as IMonsterAttack;
        if (Attack == null)
            Attack = GetComponent<IMonsterAttack>();

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

                if (cand <= curd * (retargetHysteresisRatio * retargetHysteresisRatio))
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

        float best = float.PositiveInfinity;
        UnitInstance bestUnit = null;
        Vector3 p = transform.position;

        foreach(var u in unitRoster.Units)
        {
            if (u == null || !u.IsAlive)
                continue;

            float d = (u.transform.position - p).sqrMagnitude;
            if(d < best)
            {
                best = d;
                bestUnit = u;
            }
        }

        return bestUnit;
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
}
