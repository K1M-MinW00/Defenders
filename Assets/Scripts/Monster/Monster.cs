using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(NavMeshAgent))]

public class Monster : MonoBehaviour, IDamageable
{
    [Header("Combat")]
    public float atkRange = 1.5f;
    public float atkCoolTime = 1.0f;

    private NavMeshAgent agent;
    private Transform target;
    private float lastAtkTime;

    public List<PlayerCharacter> characters;

    private bool isDead;
    public bool IsDead => isDead;

    [SerializeField] private float hp = 50;

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

    private void Update()
    {

    }
    /// <summary>
    /// 주기적으로 공격 가능 여부 체크
    /// </summary>
    private IEnumerator CombatCheckRoutine()
    {
        WaitForSeconds wait = new WaitForSeconds(2f);

        while (true)
        {
            if (target == null)
            {
                FindClosestPlayer();
            }

            if (target != null)
            {
                float distance = Vector2.Distance(transform.position, target.position);

                if (distance <= atkRange)
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
            float dist = Vector3.SqrMagnitude(player.transform.position - currentPos);
            if (dist < closestDist)
            {
                closestDist = dist;
                closest = player.transform;
            }
        }

        target = closest;
        Debug.Log("몬스터 타겟 설정");

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

        Debug.Log("몬스터 정지");
        agent.isStopped = true;
        agent.velocity = Vector3.zero;
    }

    private void TryAttack()
    {
        if (lastAtkTime - Time.time >= atkCoolTime)
        {
            lastAtkTime = Time.time;
            Debug.Log("플레이어 공격");
        }
    }

    public void TakeDamage(float damage)
    {
        hp -= damage;
        Debug.Log($"Enemy Hit! HP: {hp}");

        if (hp <= 0)
        {
            Destroy(gameObject);
        }
    }
}