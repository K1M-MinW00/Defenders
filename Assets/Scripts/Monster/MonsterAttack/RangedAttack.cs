using UnityEngine;

public class RangedAttack : MonoBehaviour, IMonsterAttack
{
    [Header("Projectile")]
    [SerializeField] private ArcProjectile projectilePrefab;
    [SerializeField] private Transform firePoint;

    [Header("Attack")]
    [SerializeField] private float flightTime = 0.6f;
    [SerializeField] private float arcHeight = 1.2f;

    [Header("Splash")]
    [SerializeField] private float splashRadius = 0f;
    [SerializeField] private LayerMask targetLayer;

    public void Execute(MonsterController ctx)
    {
        if (ctx == null)
            return;

        var target = ctx.TargetUnit;
        if (target == null || !target.IsAlive)
            return;

        if (projectilePrefab == null)
        {
            Debug.LogWarning("Ranged Attack : bullet is null.");
            return;
        }

        Vector3 start = firePoint != null ? firePoint.position : ctx.transform.position;
        Vector3 end = target.transform.position;

        var proj = Instantiate(projectilePrefab, start, Quaternion.identity); // Object Pool ¿¬µ¿
        proj.Initialize(end, ctx.AtkDamage, flightTime, arcHeight, splashRadius, targetLayer);
    }
}
