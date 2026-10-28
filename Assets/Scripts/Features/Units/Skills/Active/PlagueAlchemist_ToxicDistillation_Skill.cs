using UnityEngine;

public sealed class PlagueAlchemist_ToxicDistillation_Skill : ActiveSkillBase
{
    [Header("Toxic Distillation")]
    [SerializeField] private Transform throwPoint;
    [SerializeField] private ToxicFlaskProjectile flaskPrefab;
    [SerializeField] private ToxicPoolArea poolPrefab;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField, Min(0f)] private float damageMultiplier = 1.25f;
    [SerializeField, Min(0f)] private float upgradedDamageMultiplier = 1.65f;
    [SerializeField, Min(0.05f)] private float impactRadius = 1.35f;
    [SerializeField, Min(0.05f)] private float flightDuration = 0.65f;
    [SerializeField, Min(0f)] private float arcHeight = 1.5f;
    [SerializeField, Min(0.05f)] private float poolDuration = 5f;
    [SerializeField, Min(0.05f)] private float poolRadius = 1.6f;
    [SerializeField, Min(0f)] private float poolDamageRatioPerTick = 0.12f;
    [SerializeField, Min(0.05f)] private float poolTickInterval = 0.5f;
    [SerializeField, Range(0f, 1f)] private float poolSlowRatio = 0.25f;

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
        if (CombatTargetSelector.IsValid(context.EnemyTarget))
            owner.Animation.FaceTarget(context.EnemyTarget);

        Telegraph?.ShowCircle(
            context.CastPosition,
            poolRadius,
            new Color(0.48f, 0.58f, 0.12f, 0.9f));
    }

    public override void OnSkillApply(SkillExecutionContext context)
    {
        Vector3 spawnPosition = throwPoint != null ? throwPoint.position : owner.transform.position;
        if (!TrySpawnSkillObject(
                flaskPrefab,
                spawnPosition,
                Quaternion.identity,
                PoolCategory.Projectile,
                out ToxicFlaskProjectile flask))
        {
            return;
        }

        float impactMultiplier = ResolveActiveUpgrade(damageMultiplier, upgradedDamageMultiplier);
        flask.Initialize(
            owner,
            context.CastPosition,
            poolPrefab,
            owner.Attack * impactMultiplier,
            impactRadius,
            flightDuration,
            arcHeight,
            poolDuration,
            poolRadius,
            owner.Attack * poolDamageRatioPerTick,
            poolTickInterval,
            poolSlowRatio,
            enemyLayer);
    }
}
