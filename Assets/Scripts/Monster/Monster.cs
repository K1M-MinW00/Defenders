using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;
using System;

[RequireComponent(typeof(NavMeshAgent))]

public class Monster : MonoBehaviour, IDamageable, IPoolable
{
    private ObjectPool pool;
    private string poolKey;
    public string PoolKey => poolKey;


    [Header("Combat")]
    public float atkRange = 1.5f;
    public float atkCoolTime = 1.0f;
    
    private float maxHp = 50;
    [SerializeField] private float curHp = 50;

    private NavMeshAgent agent;
    private Transform target;
    private float lastAtkTime;

    public List<PlayerCharacter> characters;

    private bool isDead;
    public bool IsDead => isDead;

    public event Action<Monster> OnDead;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        // 2D NavMesh 설정
        agent.updateRotation = false;
        agent.updateUpAxis = false;
    }

    private void Start()
    {
        FindClosestPlayer();
        StartCoroutine(CombatCheckRoutine());
    }

    
    public void SetPool(ObjectPool pool, string key)
    {
        this.pool = pool;
        this.poolKey = key;
    }

    public void OnSpawn()
    {
        isDead = false;
        curHp = maxHp;

        target = null;
        lastAtkTime = 0f;

        agent.isStopped = false;
        agent.ResetPath();

        StartCoroutine (CombatCheckRoutine());
    }

    public void OnDespawn()
    {
        StopAllCoroutines();
    }

    /// <summary>
    /// 주기적으로 공격 가능 여부 체크
    /// </summary>
    private IEnumerator CombatCheckRoutine()
    {
        WaitForSeconds wait = new WaitForSeconds(2f);

        while (!isDead)
        {
            if (target == null)
                FindClosestPlayer();
            
            if (target != null)
            {
                float sqrDist = (transform.position - target.position).sqrMagnitude;

                if (sqrDist <= atkRange * atkRange)
                {
                    StopMovement();
                    TryAttack();
                }
                else
                {
                    MoveToTarget();
                }
            }

            yield return wait;
        }
    }

    private void FindClosestPlayer()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        float closestDist = float.MaxValue;
        Transform closest = null;

        Vector3 currentPos = transform.position;

        foreach (GameObject player in players)
        {
            float dist = (player.transform.position - currentPos).sqrMagnitude;
            
            if (dist < closestDist)
            {
                closestDist = dist;
                closest = player.transform;
            }
        }

        target = closest;
    }

    private void MoveToTarget()
    {
        if (target == null)
            return;

        if (agent.isStopped)
            agent.isStopped = false;

        if (NavMesh.SamplePosition(target.position, out NavMeshHit hit, 1.0f, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }


    private void StopMovement()
    {
        if (agent.isStopped)
            return;

        agent.isStopped = true;
        agent.velocity = Vector3.zero;
    }

    private void TryAttack()
    {
        if (Time.time - lastAtkTime < atkCoolTime)
            return;

        lastAtkTime = Time.time;

        // TODO : 공격 시도

    }

    public void TakeDamage(float damage)
    {
        if (isDead)
            return;

        curHp -= damage;
        Debug.Log($"Enemy Hit! HP: {curHp}");

        if (curHp <= 0)
        {
            Die();
        }
    }
    private void Die()
    {
        isDead = true;
        agent.isStopped = true;

        OnDead?.Invoke(this);
    }
}