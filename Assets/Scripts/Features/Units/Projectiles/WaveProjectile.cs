using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class WaveProjectile : MonoBehaviour, IPoolable
{
    [Header("Runtime")]
    [SerializeField] private float lifetime = 2f;

    private DamageRequest damageRequest;
    private Vector2 direction;
    private float speed;
    private float maxDistance;
    private LayerMask targetLayer;
    private bool pierceTargets;
    private int maxHitCount;

    private Vector2 startPosition;
    private int currentHitCount;
    private Poolable poolable;
    private bool isActive;

    private readonly HashSet<ICombatHealth> hitTargets = new();

    private void Awake()
    {
        poolable = GetComponent<Poolable>();
        if (poolable == null)
            poolable = gameObject.AddComponent<Poolable>();
    }

    public void Initialize(float damage,Vector2 direction,float speed,float maxDistance,LayerMask targetLayer,bool pierceTargets,int maxHitCount, ICombatTarget source)
    {
        damageRequest = new DamageRequest(damage, source, DamageOrigin.Skill);
        this.direction = direction.normalized;
        this.speed = speed;
        this.maxDistance = maxDistance;
        this.targetLayer = targetLayer;
        this.pierceTargets = pierceTargets;
        this.maxHitCount = Mathf.Max(1, maxHitCount);

        startPosition = transform.position;
        currentHitCount = 0;
        hitTargets.Clear();
        isActive = true;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);

        CancelInvoke(nameof(ReturnToPool));
        Invoke(nameof(ReturnToPool), lifetime);
    }

    private void Update()
    {
        if (!isActive)
            return;

        transform.position += (Vector3)(direction * speed * Time.deltaTime);

        float traveled = Vector2.Distance(startPosition, transform.position);
        if (traveled >= maxDistance)
        {
            DisableSelf();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isActive)
            return;

        if (!ProjectileHitResolver.TryResolve(
                other,
                targetLayer,
                hitTargets,
                out ICombatHealth combatHealth))
            return;

        combatHealth.ApplyDamage(damageRequest);
        currentHitCount++;

        if (!pierceTargets || currentHitCount >= maxHitCount)
        {
            DisableSelf();
        }
    }

    private void DisableSelf()
    {
        ReturnToPool();
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
        CancelInvoke(nameof(ReturnToPool));
    }

    public void OnDespawn()
    {
        isActive = false;
        CancelInvoke(nameof(ReturnToPool));

        damageRequest = default;
        direction = Vector2.zero;
        speed = 0f;
        maxDistance = 0f;
        targetLayer = 0;
        pierceTargets = false;
        maxHitCount = 0;
        startPosition = Vector2.zero;
        currentHitCount = 0;
        hitTargets.Clear();
    }
}
