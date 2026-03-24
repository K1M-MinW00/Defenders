using System;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(MonsterHealth))]
public class MonsterController : MonoBehaviour, IPoolable
{
    [Header("References")]
    private ModelView view;
    private UnitRoster unitRoster;
    private IMonsterAttack attackBehavior;

    public MonsterDataSO Data { get; private set; }
    public MonsterStats FinalStats { get; private set; }

    public NavMeshAgent Agent { get; private set; }
    public UnitRuntime Target { get; private set; }
    public MonsterHealth Health { get; private set; }

    public float AttackRange => FinalStats.atkRange;
    public float AttackCooldown => 1f / FinalStats.atkPerSec;
    public float AtkDamage => FinalStats.atkDamage;
    public string PoolKey => poolKey;
    private string poolKey;

    public void SetPoolKey(string key) => poolKey = key;

    private MonsterFSM fsm;
    public MonsterMoveState moveState;
    public MonsterAttackState attackState;
    public MonsterIdleState idleState;

    public event Action<MonsterController> OnDead;

    private void Awake()
    {
        Agent = GetComponent<NavMeshAgent>();

        if (view == null)
            view = GetComponentInChildren<ModelView>();

        if (attackBehavior == null)
            attackBehavior = GetComponent<IMonsterAttack>();

        Health = GetComponent<MonsterHealth>();
        Health.OnDead += HandleDead;

        Agent.updateRotation = false;
        Agent.updateUpAxis = false;

        fsm = new MonsterFSM();
        moveState = new MonsterMoveState(this, fsm);
        attackState = new MonsterAttackState(this, fsm);
        idleState = new MonsterIdleState(this, fsm);
    }

    public void Initialize(UnitRoster unitRoster, MonsterDataSO data)
    {
        Data = data;
        FinalStats = data.Stats;
        this.unitRoster = unitRoster;

        ApplyStats();
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
        fsm.ChangeState(idleState);
    }

    public void OnDespawn()
    {
        Target = null;
        StopMovement();
    }

    private void ApplyStats()
    {
        if (FinalStats == null)
            return;

        Health.Initialize(FinalStats);
        Agent.speed = FinalStats.moveSpeed;
    }


    private void HandleDead(MonsterHealth health)
    {
        ClearTarget();
        StopMovement();
        OnDead?.Invoke(this);
    }


    // --- Targeting / Movement Helpers ---
    public void ClearTarget() => Target = null;
    public void SetTarget(UnitRuntime newTarget) => Target = newTarget;
    public bool HasValidTarget()
    { 
        return Target != null && Target.IsAlive;
    }

    public bool TryFindClosestAliveUnit()
    {
        UnitRuntime closest = unitRoster.FindClosestAlive(transform.position);
        SetTarget(closest);
        return HasValidTarget();
    }

    public void MoveTo(Vector3 dest)
    {
        if (Agent == null || !Agent.enabled)
            return;

        ResumeMovement();
        Agent.SetDestination(dest);
    }

    public void MoveToTarget()
    {
        if (!HasValidTarget())
            return;
        FaceTarget();
        MoveTo(Target.transform.position);
    }

    public void ResumeMovement()
    {
        Agent.enabled = true;
        Agent.isStopped = false;
    }
    public void StopMovement()
    {
        Agent.isStopped = true;
        Agent.velocity = Vector3.zero;
        Agent.ResetPath();
    }

    public bool IsTargetInAttackRange()
    {
        if (!HasValidTarget())
            return false;

        float distSqr = (Target.transform.position - transform.position).sqrMagnitude;
        float range = AttackRange;

        return distSqr <= range * range;
    }

    public void TryAttackCurrentTarget()
    {
        if (!HasValidTarget())
            return;

        FaceTarget();
        attackBehavior?.TryAttack(Target);
    }

    public void FaceTarget()
    {
        if (!HasValidTarget())
            return;

        view?.FaceTo(transform.position, Target.transform.position);
    }

    public void PlayIdle() => view?.PlayIdle();
    public void PlayMove() => view?.PlayMove();
    public void PlayAttack() => view?.PlayAttack();
}
