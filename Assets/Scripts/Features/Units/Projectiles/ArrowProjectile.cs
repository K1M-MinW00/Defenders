using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Poolable))]
public class ArrowProjectile : MonoBehaviour, IPoolable
{
    [Header("Projectile")]
    [SerializeField] private float lifeTime = 3f;

    private Poolable poolable;
    private LayerMask targetLayer;
    private float damage;
    private ICombatTarget source;
    private Vector2 direction;
    private float speed;
    private bool isActive;

    private void Awake()
    {
        poolable = GetComponent<Poolable>();

    }

    public void Initialize(float damage, float speed, Vector2 dir, LayerMask target, ICombatTarget source)
    {
        this.damage = damage;
        this.source = source;
        this.speed = speed;
        this.direction = dir;
        targetLayer = target;

        isActive = true;

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
        if(!isActive)
            return;
        
        if (!ProjectileHitResolver.TryResolve(
                collision,
                targetLayer,
                damagedTargets: null,
                out ICombatTarget target,
                out ICombatHealth combatHealth))
            return;

        if (source is UnitController unitSource)
        {
            UnitBasicAttackHitResolver.Apply(unitSource, target, combatHealth, damage);
        }
        else
        {
            combatHealth.ApplyDamage(
                new DamageRequest(damage, source, DamageOrigin.BasicAttack));
        }

        ReturnToPool();
    }

    private void ReturnToPool()
    {
        if(!isActive)
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
        speed = 0f;
        direction = Vector2.zero;
        targetLayer = 0;
    }
}
