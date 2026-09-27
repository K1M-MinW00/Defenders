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
    }

    public MonsterController GetClosestAlive(Vector3 from)
    {
        CleanupDeadOrNull();
        return CombatTargetSelector.FindClosest(inRange, from);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & enemyLayer) == 0)
            return;

        if (!other.TryGetComponent(out MonsterController monster))
            return;

        if (!CombatTargetSelector.IsValid(monster))
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
        inRange.RemoveWhere(monster => !CombatTargetSelector.IsValid(monster));
    }
}
