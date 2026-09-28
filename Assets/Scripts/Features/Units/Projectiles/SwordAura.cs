using System.Collections.Generic;
using UnityEngine;

public class SwordAura : MonoBehaviour, IPoolable
{
    private Poolable poolable;

    private DamageRequest damageRequest;
    private Vector2 direction;
    private float speed;
    private float lifeTime;
    private LayerMask targetLayer;

    private bool isActive;

    private readonly HashSet<ICombatHealth> hitTargets = new();

    private void Awake()
    {
        poolable = GetComponent<Poolable>();
        if(poolable ==  null )
            poolable = gameObject.AddComponent<Poolable>();
    }

    public void Initialize(float damage, Vector2 direction, float projectileSpeed, float projectileLifeTime, LayerMask targetLayer, ICombatTarget source)
    {
        damageRequest = new DamageRequest(damage, source, DamageOrigin.Skill);
        this.direction = direction;
        this.speed = projectileSpeed;
        this.lifeTime = projectileLifeTime;
        this.targetLayer = targetLayer;

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

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isActive)
            return;

        if (!ProjectileHitResolver.TryResolve(
                collision,
                targetLayer,
                hitTargets,
                out ICombatHealth combatHealth))
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
        hitTargets.Clear();
        CancelInvoke(nameof(ReturnToPool));
    }

    public void OnDespawn()
    {
        isActive = false;
        hitTargets.Clear();
        CancelInvoke(nameof(ReturnToPool));

        damageRequest = default;
        speed = 0f;
        lifeTime = 0f;
        direction = Vector2.zero;
        targetLayer = 0;
    }
}
