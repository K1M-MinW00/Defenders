using UnityEngine;

public class SwordAura : MonoBehaviour
{
    private float damage;
    private Vector2 direction;
    private float speed;
    private float lifeTime;
    private LayerMask targetLayer;

    private float spawnTime;


    public void Initialize(float damage, Vector2 direction, float projectileSpeed, float projectileLifeTime, LayerMask targetLayer)
    {
        this.damage = damage;
        this.direction = direction;
        this.speed = projectileSpeed;
        this.lifeTime = projectileLifeTime;
        this.targetLayer = targetLayer;

        spawnTime = Time.time;
        RotateVisual();
    }

    private void RotateVisual()
    {
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    private void Update()
    {
        Move();
        CheckDestroyCondition();
    }

    private void CheckDestroyCondition()
    {
        if (Time.time >= spawnTime + lifeTime)
        {
            Destroy(gameObject);
            return;
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

        if (collision.TryGetComponent<IDamageable>(out IDamageable damageable))
        {
            damageable.TakeDamage(damage);

        }
    }
}
