using UnityEngine;

public class Archer_PowerShot_Skill : ActiveSkillBase
{
    [Header("Power Shot")]
    [SerializeField] private float damageMultiplier = 3.5f;
    [SerializeField] private float upgrade_damageMultiplier = 5f;

    [SerializeField] private PowerArrowProjectile arrow_Power_Projectile;
    [SerializeField] private float projectileSpeed = 12f;
    [SerializeField] private float projectileLifeTime = 3f;
    [SerializeField] private LayerMask enemyLayer;

    public override ActiveSkillTargetType TargetType => ActiveSkillTargetType.EnemyInRange;
    public override SkillTargetFailPolicy TargetFailPolicy => SkillTargetFailPolicy.CancelAndRefund;

    public override bool TryBuildContext(out SkillExecutionContext context)
    {
        return TryPrepareEnemyInRangeContext(out context);
    }

    public override void OnSkillStart(SkillExecutionContext context)
    {
        if (context.EnemyTarget != null)
        {
            owner.Animation.FaceTarget(context.EnemyTarget);
            Telegraph.ShowCircle(
                context.EnemyTarget.TargetTransform,
                Vector3.up * 0.55f,
                0.22f,
                new Color(1f, 0.12f, 0.08f, 0.95f));
        }
    }

    public override void OnSkillApply(SkillExecutionContext context)
    {
        Vector3 spawnPos = owner.transform.position;
        Vector2 dir = (Vector2)context.EnemyTarget.TargetTransform.position - (Vector2)spawnPos;

        dir.Normalize();

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.Euler(0f, 0f, angle);

        if (!TrySpawnSkillObject(
                arrow_Power_Projectile,
                spawnPos,
                rotation,
                PoolCategory.Projectile,
                out PowerArrowProjectile arrow))
            return;

        float multiplier = ResolveActiveUpgrade(damageMultiplier, upgrade_damageMultiplier);
        float damage = owner.Attack * multiplier;
        arrow.Initialize(damage, projectileSpeed, dir, enemyLayer, projectileLifeTime, owner);
    }

    public override void OnSkillEnd(SkillExecutionContext context) { }

    public override void CancelSkill() { }
}
