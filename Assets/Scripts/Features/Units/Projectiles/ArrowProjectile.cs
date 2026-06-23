using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ArrowProjectile : MonoBehaviour, IPoolable
{
    [Header("Projectile")]
    [SerializeField] private float lifeTime = 3f;

    private Poolable poolable;
    private LayerMask targetLayer;
    private float damage;
    private Vector2 direction;
    private float speed;
    private bool isActive;

    private void Awake()
    {
        poolable = GetComponent<Poolable>();

        if(poolable == null)
            poolable = gameObject.AddComponent<Poolable>();
    }

    public void Initialize(float damage, float speed, Vector2 dir, LayerMask target)
    {
        this.damage = damage;
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
        
        if (((1 << collision.gameObject.layer) & targetLayer) == 0)
            return;

        if (collision.TryGetComponent<IDamageable>(out IDamageable damageable))
        {
            damageable.TakeDamage(damage);
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
        speed = 0f;
        direction = Vector2.zero;
        targetLayer = 0;
    }
}