using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class PlayerCharacter : MonoBehaviour
{
    [Header("Combat")]
    public float atkRange = 3f;
    public Transform target;

    [HideInInspector] public NavMeshAgent agent;
    public IAttackBehavior attackBehavior;
    public MonsterSpawner monsterSpawner;

    // States
    private PlayerFSM fsm;
    public IdleState idleState;
    public MoveState moveState;
    public AttackState attackState;

    private List<Monster> enemyLists;

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
    }

    private void Start()
    {
        if (monsterSpawner != null)
            enemyLists = monsterSpawner.MonsterLists;

        fsm.ChangeState(idleState);
    }

    private void Update()
    {
        fsm.Update();
    }

    public Transform FindClosestEnemy()
    {
        float closestDistSqr = Mathf.Infinity;
        Transform candidate = null;

        for(int i=0; i<enemyLists.Count; i++)
        {
            Monster enemy = enemyLists[i];
            
            if(enemy == null || enemy.IsDead) 
                continue;

            float distSqr = (enemy.transform.position - transform.position).sqrMagnitude;

            if(distSqr < closestDistSqr)
            {
                closestDistSqr = distSqr;
                candidate = enemy.transform;
            }
        }

        return candidate;
    }
    
    public bool SearchTarget()
    {
        Transform candidate = FindClosestEnemy();

        if (candidate == null)
            return false;

        if(IsTargetInRange(candidate))
        {
            target = candidate;
            return true;
        }

        return false;
    }

    public bool IsTargetInRange(Transform candidate)
    {
        float distSqr = (candidate.transform.position - transform.position).sqrMagnitude;

        if (distSqr <= atkRange * atkRange)
            return true;

        return false;
    }
}
