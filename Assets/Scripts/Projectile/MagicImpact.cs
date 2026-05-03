using System.Collections.Generic;
using UnityEngine;

public class MagicImpact : MonoBehaviour, IPoolable
{
    [Header("Impact")]
    [SerializeField] private float lifeTime = .5f;

    private Poolable poolable;
    private LayerMask targetLayer;
    private float damage;
    private bool isActive;

    private readonly HashSet<IDamageable> hitTargets = new();


    private void Awake()
    {
        poolable = GetComponent<Poolable>();

        if (poolable == null)
            poolable = gameObject.AddComponent<Poolable>();
    }

    public void Initialize(float damamge, LayerMask target)
    {
        this.damage = damamge;
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

        if (((1 << collision.gameObject.layer) & targetLayer) == 0)
            return;

        if (!collision.TryGetComponent<IDamageable>(out IDamageable damageable))
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
        targetLayer = 0;
    }
}