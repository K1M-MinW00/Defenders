using UnityEngine;

public class MeteorProjectile : MonoBehaviour
{
    private float damage;
    private Vector2 targetPos;
    private float fallSpeed;
    private float explosionRadius;
    private LayerMask targetLayer;

    public void Initialize(float damage, Vector2 targetPos, float fallSpeed, float explosionRadius, LayerMask targetLayer)
    {
        this.damage = damage;
        this.targetPos = targetPos;
        this.fallSpeed = fallSpeed;
        this.explosionRadius = explosionRadius;
        this.targetLayer = targetLayer;

        Vector2 spawnPos = transform.position;
        Vector2 dir = (targetPos - spawnPos).normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f,0f,angle);
    }

    private void Update()
    {
        transform.position = Vector2.MoveTowards(transform.position, targetPos, fallSpeed * Time.deltaTime);

        if (Vector2.Distance(transform.position, targetPos) <= 0.05f)
        {
            Explode();
        }
    }

    private void Explode()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(targetPos, explosionRadius, targetLayer);

        foreach (var hit in hits)
        {
            if (hit.TryGetComponent<IDamageable>(out var damageable))
            {
                damageable.TakeDamage(damage);
            }
        }

        Destroy(gameObject);
    }
}