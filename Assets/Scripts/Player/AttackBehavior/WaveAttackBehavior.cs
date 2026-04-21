using UnityEngine;

public class WaveAttackBehavior : RangedAttackBehavior
{
    [Header("Wave")]
    [SerializeField] private WaveProjectile wavePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float projectileSpeed = 6f;
    [SerializeField] private float maxDistance = 3.5f;
    [SerializeField] private bool pierceTargets = true;
    [SerializeField] private int maxHitCount = 99;

    public override void OnAttackHit()
    {
        if (!IsAttacking)
            return;

        Vector3 origin = firePoint != null ? firePoint.position : transform.position;
        Vector3 targetPos = currentTarget.transform.position;
        Vector2 dir = (targetPos - origin).normalized;

        WaveProjectile wave = Instantiate(wavePrefab, origin, Quaternion.identity);
        wave.Initialize(Damage,dir,projectileSpeed,maxDistance,targetLayer,pierceTargets,maxHitCount);

        OnAttackFinished();
    }
}