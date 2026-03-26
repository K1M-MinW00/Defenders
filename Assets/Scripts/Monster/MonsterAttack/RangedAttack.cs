using UnityEngine;

public class RangedAttack : MonoBehaviour, IMonsterAttack
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

        Vector3 spawnPos = firePoint.position;
        Vector3 targetPos = target.transform.position;
        Vector2 dir = (targetPos - spawnPos).normalized;

        // TODO: Object Pool ¿¬µ¿
        ArrowProjectile proj = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
        proj.Initialize(owner.AtkDamage, speed, dir, targetLayer);

        return true;
    }
}
