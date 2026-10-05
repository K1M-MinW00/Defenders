using UnityEngine;

public class Knight_Cleave_Skill : ActiveSkillBase
{
    [Header("Cleave")]
    [SerializeField] private SwordAura swordAuraPrefab;
    [SerializeField] private float damageMultiplier = 2.0f;
    [SerializeField] private float upgrade_damageMultiplier = 3f;

    [SerializeField] private float projectileSpeed = 8f;
    [SerializeField] private float lifeTime = 2f;
    [SerializeField] private LayerMask enemyLayer;

    public override ActiveSkillTargetType TargetType => ActiveSkillTargetType.EnemyInRange;
    public override SkillTargetFailPolicy TargetFailPolicy => SkillTargetFailPolicy.CancelAndRefund;
    public override TargetResolutionPolicy ResolutionPolicy => TargetResolutionPolicy.LockDirection;

    public override bool TryBuildContext(out SkillExecutionContext context)
    {
        if (!TryPrepareEnemyInRangeContext(out context))
            return false;

        context.SetCastDirection(
            (Vector2)context.EnemyTarget.TargetTransform.position - (Vector2)owner.transform.position);

        return true;
    }

    public override void OnSkillStart(SkillExecutionContext context)
    {
        if (context.EnemyTarget != null)
            owner.Animation.FaceTarget(context.EnemyTarget);

        Telegraph.ShowLine(
            owner.transform,
            context.CastDirection,
            projectileSpeed * lifeTime,
            new Color(1f, 0.85f, 0.2f, 0.95f));
    }

    public override void OnSkillApply(SkillExecutionContext context)
    {
        Vector2 dir = context.CastDirection;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.Euler(0f, 0f, angle);

        if (!TrySpawnSkillObject(
                swordAuraPrefab,
                owner.transform.position,
                rotation,
                PoolCategory.Projectile,
                out SwordAura projectile))
            return;

        float multiplier = ResolveActiveUpgrade(damageMultiplier, upgrade_damageMultiplier);
        float damage = owner.Attack * multiplier;
        projectile.Initialize(damage, dir, projectileSpeed, lifeTime, enemyLayer, owner);
    }

    public override void OnSkillEnd(SkillExecutionContext context) { }

    public override void CancelSkill() { }
}
