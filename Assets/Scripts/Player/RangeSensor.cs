using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public class RangeSensor : MonoBehaviour
{
    [SerializeField] private LayerMask enemyLayer;

    private readonly HashSet<MonsterController> inRange = new HashSet<MonsterController>();

    public IReadOnlyCollection<MonsterController> InRange => inRange;
    private CircleCollider2D col;

    private void Awake()
    {
        col = GetComponent<CircleCollider2D>();
        col.isTrigger = true;
    }

    public void SetRadius(float radius)
    {
        if (col == null) 
            col = GetComponent<CircleCollider2D>();
        
        col.radius = radius;

        Debug.Log($"SetDetctionRange {radius}");
    }

    public MonsterController GetClosestAlive(Vector3 from)
    {
        CleanupDeadOrNull();

        float closestDistSqr = float.PositiveInfinity;

        MonsterController best = null;

        foreach (var enemy in inRange)
        {
            if (enemy == null || enemy.Health.IsDead)
                continue;

            float distSqr = (enemy.transform.position - transform.position).sqrMagnitude;
            if (distSqr < closestDistSqr)
            {
                closestDistSqr = distSqr;
                best = enemy;
            }
        }

        return best;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & enemyLayer) == 0)
            return;

        if (!other.TryGetComponent(out MonsterController monster))
            return;

        if (monster.Health.IsDead)
            return;

        inRange.Add(monster);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & enemyLayer) == 0)
            return;

        if (!other.TryGetComponent(out MonsterController monster))
            return;

        inRange.Remove(monster);
    }

    // 몬스터가 Destroy 되거나, 죽어서 남아있을 수 있으니 정리용
    public void CleanupDeadOrNull()
    {
        inRange.RemoveWhere(m => m == null || m.Health.IsDead);
    }
}
