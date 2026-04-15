using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Arrow_Power_Projectile : MonoBehaviour
{
    private float damage;
    private float speed;
    private Vector2 direction;
    private LayerMask enemyLayer;
    private float lifeTime;

    private readonly HashSet<Collider2D> hitTargets = new();

    public void Initialize(float damage, float speed, Vector2 direction, LayerMask enemyLayer, float lifeTime)
    {
        this.damage = damage;
        this.speed = speed;
        this.direction = direction.normalized;
        this.enemyLayer = enemyLayer;
        this.lifeTime = lifeTime;

        float angle = Mathf.Atan2(this.direction.y, this.direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);

        Destroy(gameObject, this.lifeTime);
    }

    private void Update()
    {
        transform.position += (Vector3)(direction * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & enemyLayer) == 0)
            return;

        if (hitTargets.Contains(other))
            return;

        hitTargets.Add(other);

        if (other.TryGetComponent<IDamageable>(out var damageable))
        {
            damageable.TakeDamage(damage);
        }
    }
}