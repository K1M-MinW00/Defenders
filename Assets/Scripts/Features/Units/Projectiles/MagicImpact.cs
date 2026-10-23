using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Poolable))]
public class MagicImpact : MonoBehaviour, IPoolable
{
    [Header("Impact")]
    [SerializeField] private float lifeTime = .5f;

    private Poolable poolable;
    private LayerMask targetLayer;
    private float damage;
    private UnitController source;
    private bool isActive;

    private readonly HashSet<ICombatHealth> hitTargets = new();


    private void Awake()
    {
        poolable = GetComponent<Poolable>();

    }

    public void Initialize(float damage, LayerMask target, ICombatTarget source)
    {
        this.damage = damage;
        this.source = source as UnitController;
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
                out ICombatTarget target,
                out ICombatHealth combatHealth))
            return;

        UnitBasicAttackHitResolver.Apply(source, target, combatHealth, damage);

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
        source = null;
        targetLayer = 0;
    }
}
