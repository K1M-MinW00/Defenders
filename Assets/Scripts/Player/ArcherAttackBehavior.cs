
using UnityEngine;

public class ArcherAttackBehavior : RangedAttackBehavior
{
    [Header("Archer")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private ArrowProjectile arrowPrefab;
    [SerializeField] private float projectileSpeed = 8f;

    public override void OnAttackCast()
    {
        if (!isAttacking)
            return;

        if (pendingTarget == null)
        {
            OnAttackFinished();
            return;
        }

        Vector3 spawnPos = firePoint.position;
        Vector3 targetPos = pendingTarget.transform.position;
        Vector2 dir = (targetPos - spawnPos).normalized;

        ArrowProjectile arrow = Instantiate(arrowPrefab, spawnPos, Quaternion.identity);
        arrow.Initialize(Damage, projectileSpeed, dir);

        OnAttackFinished();
    }
}