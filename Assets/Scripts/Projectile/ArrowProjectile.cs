using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ArrowProjectile : MonoBehaviour
{
    [Header("Projectile")]
    [SerializeField] private float lifeTime = 3f;
    [SerializeField] private LayerMask targetLayer;
    private float damage;
    private float speed;
    private Vector2 direction;

    public void Initialize(float damage, float speed, Vector2 dir, LayerMask target)
    {
        this.damage = damage;
        this.speed = speed;
        this.direction = dir;
        targetLayer = target;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);

        Destroy(gameObject, lifeTime);
    }

    private void Update()
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
            Destroy(gameObject);
        }
    }
}