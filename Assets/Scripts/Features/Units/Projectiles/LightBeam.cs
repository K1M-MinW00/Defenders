using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
public class LightBeam : MonoBehaviour, IPoolable
{
    private Poolable poolable;
    private BoxCollider2D boxCollider;

    private DamageRequest damageRequest;
    private LayerMask enemyLayer;
    private bool isActive;

    private readonly HashSet<ICombatHealth> damagedTargets = new();

    private void Awake()
    {
        poolable = GetComponent<Poolable>();
        boxCollider = GetComponent<BoxCollider2D>();

        boxCollider.isTrigger = true;
    }

    public void Initialize(Vector2 center,Vector2 direction,float length,float width,float damage,LayerMask enemyLayer, ICombatTarget source)
    {
        damageRequest = new DamageRequest(damage, source, DamageOrigin.Skill);
        this.enemyLayer = enemyLayer;

        isActive = true;
        damagedTargets.Clear();

        transform.position = center;
        transform.localScale = new Vector2(length, width);

        if (boxCollider != null)
        {
            boxCollider.size = new Vector2(length, width);
            boxCollider.enabled = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryDamage(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        TryDamage(other);
    }

    private void TryDamage(Collider2D other)
    {
        if (!isActive)
            return;

        if (((1 << other.gameObject.layer) & enemyLayer) == 0)
            return;

        if (!other.TryGetComponent(out ICombatHealth combatHealth))
            return;

        if (!damagedTargets.Add(combatHealth))
            return;

        combatHealth.ApplyDamage(damageRequest);
    }

    public void ReturnToPool()
    {
        if (!isActive)
            return;

        poolable.ReturnToPool();
    }

    public void OnSpawn()
    {
        isActive = false;

        if (boxCollider != null)
            boxCollider.enabled = false;
    }

    public void OnDespawn()
    {
        isActive = false;

        damageRequest = default;
        enemyLayer = 0;

        if (boxCollider != null)
            boxCollider.enabled = false;
    }
}
