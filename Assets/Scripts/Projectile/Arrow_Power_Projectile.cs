using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Arrow_Power_Projectile : MonoBehaviour, IPoolable
{
    private Poolable poolable;

    private float damage;
    private float speed;
    private float lifeTime;
    private LayerMask enemyLayer;
    private Vector2 direction;
    private bool isActive;

    private readonly HashSet<IDamageable> hitTargets = new();

    private void Awake()
    {
        poolable = GetComponent<Poolable>();

        if (poolable == null)
            poolable = gameObject.AddComponent<Poolable>();
    }

    public void Initialize(float damage, float speed, Vector2 direction, LayerMask enemyLayer, float lifeTime)
    {
        this.damage = damage;
        this.speed = speed;
        this.direction = direction.normalized;
        this.enemyLayer = enemyLayer;
        this.lifeTime = lifeTime;

        isActive = true;

        hitTargets.Clear();

        CancelInvoke(nameof(ReturnToPool));
        Invoke(nameof(ReturnToPool), lifeTime);
    }

    private void Update()
    {
        if (!isActive)
            return;

        transform.position += (Vector3)(direction * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isActive)
            return;

        if (((1 << other.gameObject.layer) & enemyLayer) == 0)
            return;

        if (!other.TryGetComponent<IDamageable>(out var damageable))
            return;

        if (!hitTargets.Add(damageable))
            return;

        damageable.TakeDamage(damage);
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

        damage = 0f;
        speed = 0f;
        lifeTime = 0f;
        direction = Vector2.zero;
        enemyLayer = 0;
    }
}