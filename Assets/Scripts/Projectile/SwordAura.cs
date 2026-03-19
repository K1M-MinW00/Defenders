using UnityEngine;

public class SwordAura : MonoBehaviour
{
    [Header("Runtime")]
    [SerializeField] private float damage;
    [SerializeField] private Vector2 direction;
    [SerializeField] private float speed;
    [SerializeField] private float lifeTime;
    [SerializeField] private float maxTravelDistance;
    [SerializeField] private float hitRadius;
    [SerializeField] private int pierceCount;
    [SerializeField] private LayerMask targetLayer;

    private Vector2 startPos;
    private float spawnTime;
    private int hitCount;


    public void Initialize(float damage, Vector2 direction, float projectileSpeed, float projectileLifeTime, float maxTravelDistance, float projectilRadius, int pierceCount, LayerMask targetLayer)
    {
        this.damage = damage;
        this.direction = direction;
        this.speed = projectileSpeed;
        this.lifeTime = projectileLifeTime;
        this.maxTravelDistance = maxTravelDistance;
        this.hitRadius = projectilRadius;
        this.pierceCount = pierceCount;
        this.targetLayer = targetLayer;

        startPos = transform.position;
        spawnTime = Time.time;

        RotateVisual();
    }

    private void RotateVisual()
    {
        float angle = Mathf.Atan2(direction.y,direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f,0f,angle);
    }

    private void Update()
    {
        Move();
        CheckDestroyCondition();
    }

    private void CheckDestroyCondition()
    {
        if(Time.time >= spawnTime + lifeTime)
        {
            Destroy(gameObject);
            return;
        }

        float sqrDistance = ((Vector2)transform.position - startPos).sqrMagnitude;
        if(sqrDistance >= maxTravelDistance * maxTravelDistance)
        {
            Destroy(gameObject);
        }
    }

    private void Move()
    {
        transform.position += (Vector3)(direction * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (((1 << collision.gameObject.layer) & targetLayer) == 0)
            return;

        if(collision.TryGetComponent<IDamageable>(out IDamageable damageable))
        {
            damageable.TakeDamage(damage);
            
        }
    }
}
