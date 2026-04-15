using UnityEngine;

public class SoldierMLeapSlashSkill : ActiveSkillBase
{
    [Header("Leap Slash")]
    [SerializeField] private float damageMultiplier = 2.2f;
    [SerializeField] private float impactRadius = 1.2f;
    [SerializeField] private LayerMask enemyLayer;

    public override ActiveSkillTargetType TargetType => ActiveSkillTargetType.SelfArea;
    public override SkillTargetFailPolicy TargetFailPolicy => SkillTargetFailPolicy.CastWithoutTarget;

    public override bool TryBuildContext(out SkillExecutionContext context)
    {
        context = new SkillExecutionContext();
        context.Initialize(owner);

        MonsterController target = owner.Targeting.GetClosestEnemyInRange();
        if(target != null)
            context.SetEnemyTarget(target);

        Vector3 castPos = owner.transform.position;
        context.SetCastPosition(castPos);

        return true;
    }

    public override void OnSkillStart(SkillExecutionContext context)
    {
        if(context.EnemyTarget != null)
            owner.Animation.FaceTarget(context.EnemyTarget);
    }

    public override void OnSkillApply(SkillExecutionContext context)
    {
        Vector3 center = context.CastPosition;
        
        Collider2D[] hits = Physics2D.OverlapCircleAll(center, impactRadius, enemyLayer);

        if (hits == null || hits.Length == 0)
            return;

        float damage = owner.Attack * damageMultiplier;

        foreach (Collider2D hit in hits)
        {
            if (hit.TryGetComponent<IDamageable>(out var damageable))
            {
                damageable.TakeDamage(damage);
            }
        }
    }

    public override void OnSkillEnd(SkillExecutionContext context)
    {
    }

    public override void CancelSkill()
    {
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, impactRadius);
    }
#endif
}