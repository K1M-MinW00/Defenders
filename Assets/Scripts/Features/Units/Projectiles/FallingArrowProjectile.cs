using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class ArrowRainFallingArrow : MonoBehaviour, IPoolable
{
    [Header("Fall")]
    [SerializeField] private float fallSpeed = 10f;
    [SerializeField] private float lifeTime = 0.5f;
    [SerializeField] private int hitBufferSize = 32;

    private Poolable poolable;

    private Collider2D[] hitBuffer;
    private ContactFilter2D hitFilter;
    private readonly HashSet<IDamageable> damagedTargets = new();

    private Vector2 targetPoint;
    private float damage;
    private float hitRadius;
    private bool isActive;
    private bool hasLanded;

    private void Awake()
    {
        poolable = GetComponent<Poolable>();

        if(poolable == null)
            poolable = gameObject.AddComponent<Poolable>();

        hitBuffer = new Collider2D[hitBufferSize];

        hitFilter = new ContactFilter2D();
        hitFilter.useLayerMask = true;
        hitFilter.useTriggers = true;
    }

    public void Initialize(Vector2 targetPoint, float damage, float hitRadius, LayerMask enemyLayer)
    {
        this.targetPoint = targetPoint;
        this.damage = damage;
        this.hitRadius = hitRadius;

        hitFilter.SetLayerMask(enemyLayer);

        isActive = true;
        hasLanded = false;

        Vector2 dir = (targetPoint - (Vector2)transform.position).normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    private void Update()
    {
        if (!isActive || hasLanded)
            return;

        Vector2 current = transform.position;
        Vector2 next = Vector2.MoveTowards(current, targetPoint, fallSpeed * Time.deltaTime);
        transform.position = next;

        if (Vector2.Distance(transform.position, targetPoint) <= 0.03f)
        {
            ApplyDamage();

            hasLanded = true;
            CancelInvoke(nameof(ReturnToPool));
            Invoke(nameof(ReturnToPool), lifeTime);
        }

    }

    private void ApplyDamage()
    {
        int hitCount = Physics2D.OverlapCircle(targetPoint, hitRadius, hitFilter,hitBuffer);

        damagedTargets.Clear();

        for (int i = 0; i < hitCount; i++)
        {
            Collider2D hit = hitBuffer[i];

            if (hit == null)
                continue;

            if (!hit.TryGetComponent<IDamageable>(out var damageable))
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
        hasLanded = false;

        CancelInvoke(nameof(ReturnToPool));
    }

    public void OnDespawn()
    {
        isActive = false;
        hasLanded = false;
        CancelInvoke(nameof(ReturnToPool));

        damagedTargets.Clear();

        targetPoint = Vector2.zero;
        damage = 0f;
        hitRadius = 0f;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(targetPoint, hitRadius);
    }
#endif
}