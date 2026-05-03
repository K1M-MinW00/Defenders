using UnityEngine;

public class RangedMonsterAttack : MonoBehaviour, IMonsterAttack
{
    protected MonsterController owner;

    [Header("Projectile")]
    [SerializeField] private ArrowProjectile projectilePrefab;
    [SerializeField] private Transform firePoint;

    [Header("Attack")]
    [SerializeField] private float speed = 6f;
    [SerializeField] private LayerMask targetLayer;

    protected virtual void Awake()
    {
        if (owner == null)
            owner = GetComponent<MonsterController>();

        if (projectilePrefab == null)
        {
            Debug.LogWarning("Ranged Attack : bullet is null.");
            return;
        }
    }

    public bool CanAttack()
    {
        if (owner == null || owner.Health.IsDead)
            return false;

        if (!owner.IsTargetInAttackRange())
            return false;

        return true;
    }

    public bool TryAttack(UnitController target)
    {
        if (target == null || target.IsDead)
            return false;

        if (!CanAttack())
            return false;

        owner.PlayAttack();

        Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position;
        Vector3 targetPos = target.transform.position;

        Vector2 dir = (targetPos - spawnPos).normalized;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.Euler(0f, 0f, angle);

        ArrowProjectile arrow = owner.PoolManager.Spawn(projectilePrefab, spawnPos, rotation, PoolCategory.Projectile);
        
        if(arrow != null)
            arrow.Initialize(owner.AtkDamage, speed, dir, targetLayer);

        return true;
    }
}
