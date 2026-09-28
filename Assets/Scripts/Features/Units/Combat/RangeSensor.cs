using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public class RangeSensor : MonoBehaviour
{
    [SerializeField] private LayerMask enemyLayer;

    private readonly HashSet<ICombatTarget> inRange = new();

    public IReadOnlyCollection<ICombatTarget> InRange => inRange;
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

    public ICombatTarget GetClosestAlive(Vector3 from)
    {
        CleanupDeadOrNull();
        return CombatTargetSelector.FindClosest(inRange, from);
    }

    public void ClearTrackedTargets()
    {
        inRange.Clear();
    }

    private void OnDisable()
    {
        ClearTrackedTargets();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & enemyLayer) == 0)
            return;

        if (!other.TryGetComponent(out ICombatTarget target))
            return;

        if (!CombatTargetSelector.IsValid(target))
            return;

        inRange.Add(target);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & enemyLayer) == 0)
            return;

        if (!other.TryGetComponent(out ICombatTarget target))
            return;

        inRange.Remove(target);
    }

    // 몬스터가 Destroy 되거나, 죽어서 남아있을 수 있으니 정리용
    public void CleanupDeadOrNull()
    {
        inRange.RemoveWhere(target => !CombatTargetSelector.IsValid(target));
    }
}
