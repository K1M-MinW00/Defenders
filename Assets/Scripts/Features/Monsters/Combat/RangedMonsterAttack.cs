using UnityEngine;

public class RangedMonsterAttack : MonsterAttackBase
{
    [Header("Projectile")]
    [SerializeField] private ArrowProjectile projectilePrefab;
    [SerializeField] private Transform firePoint;

    [Header("Attack")]
    [SerializeField] private float speed = 6f;
    [SerializeField] private LayerMask targetLayer;

    protected override void Awake()
    {
        base.Awake();

        if (projectilePrefab == null)
        {
            Debug.LogWarning("Ranged Attack : bullet is null.");
            return;
        }
    }

    protected override void ApplyHit(ICombatTarget target)
    {
        Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position;
        Vector3 targetPos = target.TargetTransform.position;

        Vector2 dir = (targetPos - spawnPos).normalized;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.Euler(0f, 0f, angle);

        ArrowProjectile arrow = owner.PoolManager.Spawn(projectilePrefab, spawnPos, rotation, PoolCategory.Projectile);
        
        if(arrow != null)
            arrow.Initialize(owner.AtkDamage, speed, dir, targetLayer, owner);
    }
}
