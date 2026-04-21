using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class WaveProjectile : MonoBehaviour
{
    [Header("Runtime")]
    [SerializeField] private float lifetime = 2f;

    private float damage;
    private Vector2 direction;
    private float speed;
    private float maxDistance;
    private LayerMask targetLayer;
    private bool pierceTargets;
    private int maxHitCount;

    private Vector2 startPosition;
    private int currentHitCount;

    private readonly HashSet<IDamageable> hitTargets = new();

    public void Initialize(float damage,Vector2 direction,float speed,float maxDistance,LayerMask targetLayer,bool pierceTargets,int maxHitCount)
    {
        this.damage = damage;
        this.direction = direction.normalized;
        this.speed = speed;
        this.maxDistance = maxDistance;
        this.targetLayer = targetLayer;
        this.pierceTargets = pierceTargets;
        this.maxHitCount = Mathf.Max(1, maxHitCount);

        startPosition = transform.position;
        currentHitCount = 0;
        hitTargets.Clear();

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);

        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        transform.position += (Vector3)(direction * speed * Time.deltaTime);

        float traveled = Vector2.Distance(startPosition, transform.position);
        if (traveled >= maxDistance)
        {
            DisableSelf();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & targetLayer) == 0)
            return;

        if (!other.TryGetComponent<IDamageable>(out var damageable))
            return;

        if (!hitTargets.Add(damageable))
            return;

        damageable.TakeDamage(damage);
        currentHitCount++;

        if (!pierceTargets || currentHitCount >= maxHitCount)
        {
            DisableSelf();
        }
    }

    private void DisableSelf()
    {
        CancelInvoke();
        Destroy(gameObject);
    }
}