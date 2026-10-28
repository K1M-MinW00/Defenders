using System.Collections;
using UnityEngine;

public sealed class MagmaMawAttack : MonsterAttackBase
{
    private enum Pattern
    {
        SingleFireball,
        TripleFireball,
        LargeMagmaBall
    }

    [Header("Spawn")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private ArrowProjectile smallFireballPrefab;
    [SerializeField] private MagmaMawMagmaBallProjectile largeMagmaBallPrefab;
    [SerializeField] private LayerMask targetLayer;

    [Header("Small Fireball")]
    [SerializeField, Min(0.1f)] private float smallFireballSpeed = 8f;
    [SerializeField, Min(0.01f)] private float tripleDamageMultiplier = 0.45f;
    [SerializeField, Min(0.01f)] private float tripleInterval = 0.14f;

    [Header("Large Magma Ball")]
    [SerializeField, Min(0.1f)] private float largeMagmaBallSpeed = 5f;
    [SerializeField, Min(0.1f)] private float largeDamageMultiplier = 1.6f;
    [SerializeField, Min(0.1f)] private float largeImpactRadius = 2.2f;
    [SerializeField, Min(0f)] private float burnDuration = 3f;
    [SerializeField, Min(0f)] private float burnDamageMultiplier = 0.12f;

    private int patternIndex;
    private Coroutine tripleRoutine;

    protected override void ApplyHit(ICombatTarget target)
    {
        Pattern pattern = (Pattern)(patternIndex++ % 3);
        switch (pattern)
        {
            case Pattern.TripleFireball:
                StopTripleRoutine();
                tripleRoutine = StartCoroutine(FireTriple(target));
                break;
            case Pattern.LargeMagmaBall:
                FireLargeMagmaBall(target);
                break;
            default:
                FireSmallFireball(target, 1f);
                break;
        }
    }

    private IEnumerator FireTriple(ICombatTarget target)
    {
        for (int index = 0; index < 3; index++)
        {
            if (owner == null || owner.IsDead || !CombatTargetSelector.IsValid(target))
                break;

            FireSmallFireball(target, tripleDamageMultiplier);
            if (index < 2)
                yield return new WaitForSeconds(tripleInterval);
        }

        tripleRoutine = null;
    }

    private void FireSmallFireball(ICombatTarget target, float damageMultiplier)
    {
        if (smallFireballPrefab == null || owner?.PoolManager == null ||
            !CombatTargetSelector.IsValid(target))
            return;

        Vector3 spawnPosition = GetSpawnPosition();
        Vector2 direction = (target.TargetTransform.position - spawnPosition).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        ArrowProjectile projectile = owner.PoolManager.Spawn(
            smallFireballPrefab,
            spawnPosition,
            Quaternion.Euler(0f, 0f, angle),
            PoolCategory.Projectile);

        projectile?.Initialize(
            owner.AtkDamage * damageMultiplier,
            smallFireballSpeed,
            direction,
            targetLayer,
            owner);
    }

    private void FireLargeMagmaBall(ICombatTarget target)
    {
        if (largeMagmaBallPrefab == null || owner?.PoolManager == null ||
            !CombatTargetSelector.IsValid(target))
            return;

        MagmaMawMagmaBallProjectile projectile = owner.PoolManager.Spawn(
            largeMagmaBallPrefab,
            GetSpawnPosition(),
            Quaternion.identity,
            PoolCategory.Projectile);

        projectile?.Initialize(
            owner,
            target.TargetTransform.position,
            owner.AtkDamage * largeDamageMultiplier,
            largeMagmaBallSpeed,
            largeImpactRadius,
            burnDuration,
            owner.AtkDamage * burnDamageMultiplier);
    }

    private Vector3 GetSpawnPosition() => firePoint != null ? firePoint.position : transform.position;

    private void OnDisable()
    {
        StopTripleRoutine();
    }

    private void StopTripleRoutine()
    {
        if (tripleRoutine == null)
            return;

        StopCoroutine(tripleRoutine);
        tripleRoutine = null;
    }
}
