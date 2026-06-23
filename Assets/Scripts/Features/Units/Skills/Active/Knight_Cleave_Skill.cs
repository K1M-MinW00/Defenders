using UnityEngine;

public class Knight_Cleave_Skill : ActiveSkillBase
{
    [Header("Cleave")]
    [SerializeField] private SwordAura swordAuraPrefab;
    [SerializeField] private float damageMultiplier = 2.0f;
    [SerializeField] private float projectileSpeed = 8f;
    [SerializeField] private float lifeTime = 2f;
    [SerializeField] private LayerMask enemyLayer;

    public override ActiveSkillTargetType TargetType => ActiveSkillTargetType.EnemyInRange;
    public override SkillTargetFailPolicy TargetFailPolicy => SkillTargetFailPolicy.CancelAndRefund;

    public override bool TryBuildContext(out SkillExecutionContext context)
    {
        context = new SkillExecutionContext();
        context.Initialize(owner);

        MonsterController target = owner.Targeting.GetClosestEnemyInRange();

        if (target == null)
            return false;

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
        Vector2 dir =(Vector2)context.EnemyTarget.transform.position - (Vector2)owner.transform.position;
        dir.Normalize();

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.Euler(0f,0f,angle);
       

        SwordAura projectile = owner.PoolManager.Spawn(swordAuraPrefab, owner.transform.position, rotation,PoolCategory.Projectile);

        if(projectile != null)
        { 
            float damage = owner.Attack * damageMultiplier;
            projectile.Initialize(damage, dir, projectileSpeed, lifeTime, enemyLayer);
        }
    }

    public override void OnSkillEnd(SkillExecutionContext context) { }

    public override void CancelSkill() { }
}