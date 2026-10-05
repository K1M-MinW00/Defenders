using UnityEngine;

public class Wizard_Meteor_Skill : ActiveSkillBase
{
    [Header("Fireball")]
    [SerializeField] private MeteorProjectile meteorPrefab;
    [SerializeField] private float damageMultiplier = 2f;
    [SerializeField] private float upgrade_damageMultiplier = 3f;

    [SerializeField] private float spawnHeight = 3f;
    [SerializeField] private float explosionRadius = 1.5f;
    [SerializeField] private float projectileSpeed = 10f;
    [SerializeField] private LayerMask enemyLayer;

    public override ActiveSkillTargetType TargetType => ActiveSkillTargetType.EnemyInRange;
    public override SkillTargetFailPolicy TargetFailPolicy => SkillTargetFailPolicy.WaitUntilFound;
    public override TargetResolutionPolicy ResolutionPolicy => TargetResolutionPolicy.LockPosition;

    public override bool TryBuildContext(out SkillExecutionContext context)
    {
        if (!TryPrepareEnemyInRangeContext(out context))
            return false;

        context.SetCastPosition(context.EnemyTarget.TargetTransform.position);

        return true;
    }

    public override void OnSkillStart(SkillExecutionContext context)
    {
        if (context.EnemyTarget != null)
            owner.Animation.FaceTarget(context.EnemyTarget);

        Telegraph.ShowCircle(
            context.CastPosition,
            explosionRadius,
            new Color(0.75f, 0.2f, 1f, 0.9f));
    }

    public override void OnSkillApply(SkillExecutionContext context)
    {
        Vector2 targetPos = context.CastPosition;
        Vector2 spawnPos = targetPos + Vector2.up * spawnHeight;
        
        if (!TrySpawnSkillObject(
                meteorPrefab,
                spawnPos,
                Quaternion.identity,
                PoolCategory.Projectile,
                out MeteorProjectile projectile))
            return;

        float multiplier = ResolveActiveUpgrade(damageMultiplier, upgrade_damageMultiplier);
        float damage = owner.Attack * multiplier;
        projectile.Initialize(damage, targetPos, projectileSpeed, explosionRadius, enemyLayer, owner);
    }

    public override void OnSkillEnd(SkillExecutionContext context)
    {
    }

    public override void CancelSkill()
    {
    }
}
