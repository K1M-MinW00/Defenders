using UnityEngine;

public class Archer_PowerShot_Skill : ActiveSkillBase
{
    [Header("Power Shot")]
    [SerializeField] private float damageMultiplier = 3.5f;
    [SerializeField] private Arrow_Power_Projectile arrow_Power_Projectile;
    [SerializeField] private float projectileSpeed = 12f;
    [SerializeField] private float projectileLifeTime = 3f;
    [SerializeField] private LayerMask enemyLayer;

    public override ActiveSkillTargetType TargetType => ActiveSkillTargetType.EnemyInRangeOrGlobalClosest;
    public override SkillTargetFailPolicy TargetFailPolicy => SkillTargetFailPolicy.CancelAndRefund;

    public override bool TryBuildContext(out SkillExecutionContext context)
    {
        context = new SkillExecutionContext();
        context.Initialize(owner);

        MonsterController target = owner.Targeting.GetClosestEnemyInRange();

        if (target == null)
        {
            bool enemyGlobal = owner.Targeting.FindGlobalAliveMonster();
            if (!enemyGlobal)
                return false;
        }

        target = owner.Targeting.CurrentTarget;

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
        if (context.EnemyTarget == null)
            return;

        Vector3 spawnPos = owner.transform.position;
        Vector2 dir = ((Vector2)context.EnemyTarget.transform.position - (Vector2)spawnPos);

        dir.Normalize();

        Arrow_Power_Projectile arrow = Instantiate(arrow_Power_Projectile, spawnPos, Quaternion.identity);

        float damage = owner.Attack * damageMultiplier;
        arrow.Initialize(damage, projectileSpeed, dir, enemyLayer, projectileLifeTime);
    }

    public override void OnSkillEnd(SkillExecutionContext context) { }

    public override void CancelSkill() { }
}