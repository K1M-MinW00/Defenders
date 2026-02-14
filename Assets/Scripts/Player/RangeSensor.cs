using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public class RangeSensor : MonoBehaviour
{
    [SerializeField] private LayerMask enemyLayer;

    private readonly HashSet<Monster> inRange = new HashSet<Monster>();

    public IReadOnlyCollection<Monster> InRange => inRange;

    public event Action<Monster> OnEntered;
    public event Action<Monster> OnExited;

    private CircleCollider2D col;

    private void Awake()
    {
        col = GetComponent<CircleCollider2D>();
        col.isTrigger = true;
    }

    public void SetRadius(float radius)
    {
        if (col == null) col = GetComponent<CircleCollider2D>();
        col.radius = radius;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & enemyLayer) == 0)
            return;

        if (!other.TryGetComponent(out Monster monster))
            return;

        if (monster.IsDead)
            return;

        if (inRange.Add(monster))
            OnEntered?.Invoke(monster);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & enemyLayer) == 0)
            return;

        if (!other.TryGetComponent(out Monster monster))
            return;

        if (inRange.Remove(monster))
            OnExited?.Invoke(monster);
    }

    // 몬스터가 Destroy 되거나, 죽어서 남아있을 수 있으니 정리용
    public void CleanupDeadOrNull()
    {
        inRange.RemoveWhere(m => m == null || m.IsDead);
    }
}
