using System.Collections.Generic;
using UnityEngine;

public class MagicImpact : MonoBehaviour, IPoolable
{
    [Header("Impact")]
    [SerializeField] private float lifeTime = .5f;

    private Poolable poolable;
    private LayerMask targetLayer;
    private DamageRequest damageRequest;
    private bool isActive;

    private readonly HashSet<ICombatHealth> hitTargets = new();


    private void Awake()
    {
        poolable = GetComponent<Poolable>();

        if (poolable == null)
            poolable = gameObject.AddComponent<Poolable>();
    }

    public void Initialize(float damage, LayerMask target, ICombatTarget source)
    {
        damageRequest = new DamageRequest(damage, source, DamageOrigin.BasicAttack);
        targetLayer = target;

        isActive = true;

        hitTargets.Clear();

        CancelInvoke(nameof(ReturnToPool));
        Invoke(nameof(ReturnToPool), lifeTime);
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
        CancelInvoke(nameof(ReturnToPool));
    }

    public void OnDespawn()
    {
        isActive = false;
        CancelInvoke(nameof(ReturnToPool));

        damageRequest = default;
        targetLayer = 0;
    }
}
