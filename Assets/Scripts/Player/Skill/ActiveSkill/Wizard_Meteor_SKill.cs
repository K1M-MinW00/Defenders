using UnityEngine;

public class Wizard_Meteor_Skill : ActiveSkillBase
{
    [Header("Fireball")]
    [SerializeField] private MeteorProjectile meteorPrefab;
    [SerializeField] private float damageMultiplier = 3.0f;
    [SerializeField] private float spawnHeight = 3f;
    [SerializeField] private float explosionRadius = 1.5f;
    [SerializeField] private float projectileSpeed = 10f;
    [SerializeField] private LayerMask enemyLayer;

    public override ActiveSkillTargetType TargetType => ActiveSkillTargetType.EnemyInRangeOrGlobalClosest;
    public override SkillTargetFailPolicy TargetFailPolicy => SkillTargetFailPolicy.WaitUntilFound;

    public override bool TryBuildContext(out SkillExecutionContext context)
    {
        context = new SkillExecutionContext();
        context.Initialize(owner);

        MonsterController target = owner.Targeting.GetClosestEnemyInRange();
        if (target == null)
        {
            bool find = owner.Targeting.FindGlobalAliveMonster();
            if(!find)
                return false;

            target = owner.Targeting.CurrentTarget;
        }

        context.SetEnemyTarget(target);

        return true;
    }

    public override void OnSkillStart(SkillExecutionContext context)
    {
        if (context.EnemyTarget != null)
            owner.Animation.FaceTarget(context.EnemyTarget);
    }

    public override void OnSkillApply(SkillExecutionContext context)
    {
        if (context.EnemyTarget == null || context.EnemyTarget.Health.IsDead)
            return;

        Vector2 targetPos = context.EnemyTarget.transform.position;
        Vector2 spawnPos = targetPos + Vector2.up * spawnHeight;
        
        MeteorProjectile projectile = owner.PoolManager.Spawn(meteorPrefab, spawnPos, Quaternion.identity,PoolCategory.Projectile);

        float damage = owner.Attack * damageMultiplier;
        projectile.Initialize(damage, targetPos, projectileSpeed, explosionRadius, enemyLayer);

    }

    public override void OnSkillEnd(SkillExecutionContext context)
    {
    }

    public override void CancelSkill()
    {
    }
}