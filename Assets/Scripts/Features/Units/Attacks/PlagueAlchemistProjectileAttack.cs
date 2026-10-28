using UnityEngine;

public sealed class PlagueAlchemistProjectileAttack : RangedUnitAttack
{
    [Header("Poison Vial")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private PoisonVialProjectile projectilePrefab;
    [SerializeField, Min(0.05f)] private float flightDuration = 0.45f;
    [SerializeField, Min(0f)] private float arcHeight = 0.8f;
    [SerializeField, Min(0.05f)] private float hitRadius = 0.65f;

    public override void OnAttackHit()
    {
        if (!TryEnterHitPhase())
            return;

        if (!CombatTargetSelector.IsValid(currentTarget))
        {
            CancelAttack();
            return;
        }

        if (projectilePrefab == null || owner.PoolManager == null)
        {
            Debug.LogError("[PlagueAlchemistProjectileAttack] Projectile prefab or pool manager is missing.", this);
            CancelAttack();
            return;
        }

        Vector3 spawnPosition = firePoint != null ? firePoint.position : transform.position;
        Vector3 targetPosition = currentTarget.TargetTransform.position;
        PoisonVialProjectile projectile = owner.PoolManager.Spawn(
            projectilePrefab,
            spawnPosition,
            Quaternion.identity,
            PoolCategory.Projectile);

        projectile?.Initialize(
            owner,
            currentTarget,
            targetPosition,
            Damage,
            flightDuration,
            arcHeight,
            hitRadius);
    }
}
