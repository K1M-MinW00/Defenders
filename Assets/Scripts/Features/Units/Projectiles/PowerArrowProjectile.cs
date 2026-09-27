using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class PowerArrowProjectile : MonoBehaviour, IPoolable
{
    private Poolable poolable;

    private DamageRequest damageRequest;
    private float speed;
    private float lifeTime;
    private LayerMask enemyLayer;
    private Vector2 direction;
    private bool isActive;

    private readonly HashSet<ICombatHealth> hitTargets = new();

    private void Awake()
    {
        poolable = GetComponent<Poolable>();

        if (poolable == null)
            poolable = gameObject.AddComponent<Poolable>();
    }

    public void Initialize(float damage, float speed, Vector2 direction, LayerMask enemyLayer, float lifeTime, ICombatTarget source)
    {
        damageRequest = new DamageRequest(damage, source, DamageOrigin.Skill);
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

        if (!other.TryGetComponent<ICombatHealth>(out var combatHealth))
            return;

        if (!hitTargets.Add(combatHealth))
            return;

        combatHealth.ApplyDamage(damageRequest);
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
        speed = 0f;
        lifeTime = 0f;
        direction = Vector2.zero;
        enemyLayer = 0;
    }
}
