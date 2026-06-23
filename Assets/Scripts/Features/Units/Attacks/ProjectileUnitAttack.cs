
using UnityEngine;

public class ProjectileUnitAttack : RangedUnitAttack
{
    [Header("Archer")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private ArrowProjectile arrowPrefab;
    [SerializeField] private float projectileSpeed = 8f;

    public override void OnAttackHit()
    {
        if (!isAttacking)
            return;

        if (currentTarget == null)
        {
            CancelAttack();
            return;
        }

        Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position;
        Vector3 targetPos = currentTarget.transform.position;

        Vector2 dir = (targetPos - spawnPos).normalized;

        float angle = Mathf.Atan2(dir.y,dir.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.Euler(0f, 0f, angle);

        ArrowProjectile arrow = owner.PoolManager.Spawn(arrowPrefab, spawnPos, rotation, PoolCategory.Projectile);

        if (arrow != null)
            arrow.Initialize(Damage, projectileSpeed, dir, targetLayer);
        
        OnAttackFinished();
    }
}