using System.Collections.Generic;
using UnityEngine;

public class MeteorProjectile : MonoBehaviour, IPoolable
{
    [Header("Hit Buffer")]
    [SerializeField] private int hitBufferSize = 32;

    private Poolable poolable;
    private Collider2D[] hitBuffer;
    private ContactFilter2D hitFilter;

    private float damage;
    private Vector2 targetPos;
    private float fallSpeed;
    private float explosionRadius;
    private LayerMask enemyLayer;

    private bool isActive;

    private readonly HashSet<IDamageable> damagedTargets = new();

    private void Awake()
    {
        poolable = GetComponent<Poolable>();
        hitBuffer = new Collider2D[hitBufferSize];

        hitFilter = new ContactFilter2D();
        hitFilter.useLayerMask = true;
        hitFilter.useTriggers = true;
    }

    public void Initialize(float damage, Vector2 targetPos, float fallSpeed, float explosionRadius, LayerMask targetLayer)
    {
        this.damage = damage;
        this.targetPos = targetPos;
        this.fallSpeed = fallSpeed;
        this.explosionRadius = explosionRadius;
        this.enemyLayer = targetLayer;

        hitFilter.SetLayerMask(targetLayer);

        damagedTargets.Clear();
        isActive = true;

        Vector2 spawnPos = transform.position;
        Vector2 dir = (targetPos - spawnPos).normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f,0f,angle);
    }

    private void Update()
    {
        transform.position = Vector2.MoveTowards(transform.position, targetPos, fallSpeed * Time.deltaTime);

        if (Vector2.Distance(transform.position, targetPos) <= 0.05f)
        {
            Explode();
            ReturnToPool();
        }
    }

    private void Explode()
    {
        int hitCount = Physics2D.OverlapCircle(
             targetPos,
             explosionRadius,
             hitFilter,
             hitBuffer
         );

        damagedTargets.Clear();

        for (int i = 0; i < hitCount; i++)
        {
            Collider2D hit = hitBuffer[i];

            if (hit == null)
                continue;

            if (!hit.TryGetComponent(out IDamageable damageable))
                continue;

            if (!damagedTargets.Add(damageable))
                continue;

            damageable.TakeDamage(damage);
        }
    }

    private void ReturnToPool()
    {
        if (!isActive)
            return;

        poolable.ReturnToPool();
    }

    public void OnSpawn()
    {
        isActive = false;
        damagedTargets.Clear();
        CancelInvoke(nameof(ReturnToPool));
    }

    public void OnDespawn()
    {
        isActive = false;
        damagedTargets.Clear();
        CancelInvoke(nameof(ReturnToPool));

        targetPos = Vector2.zero;
        damage = 0f;
        fallSpeed = 0f;
        explosionRadius = 0f;
        enemyLayer = 0;
    }
}