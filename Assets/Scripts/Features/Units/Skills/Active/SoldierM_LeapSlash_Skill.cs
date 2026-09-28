using UnityEngine;

public class SoldierMLeapSlashSkill : ActiveSkillBase
{
    [Header("Leap Slash")]
    [SerializeField] private float damageMultiplier = 2f;
    [SerializeField] private float upgrade_damageMultiplier = 3f;

    [SerializeField] private float impactRadius = 1.2f;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private int hitBufferSize = 32;

    private Collider2D[] hitBuffer;
    private ContactFilter2D hitFilter;
    private readonly System.Collections.Generic.HashSet<ICombatHealth> damagedTargets = new();

    public override ActiveSkillTargetType TargetType => ActiveSkillTargetType.SelfArea;
    public override SkillTargetFailPolicy TargetFailPolicy => SkillTargetFailPolicy.CastWithoutTarget;

    private void Awake()
    {
        hitBuffer = new Collider2D[hitBufferSize];
        hitFilter = new ContactFilter2D
        {
            useLayerMask = true,
            useTriggers = true
        };
        hitFilter.SetLayerMask(enemyLayer);
    }

    public override bool TryBuildContext(out SkillExecutionContext context)
    {
        context = new SkillExecutionContext();
        context.Initialize(owner);

        ICombatTarget target = owner.Targeting.GetClosestEnemyInRange();

        if (target != null)
            context.SetEnemyTarget(target);

        Vector3 castPos = owner.transform.position;
        context.SetCastPosition(castPos);

        return true;
    }

    public override void OnSkillStart(SkillExecutionContext context)
    {
        if (context.EnemyTarget != null)
            owner.Animation.FaceTarget(context.EnemyTarget);
    }

    public override void OnSkillApply(SkillExecutionContext context)
    {
        Vector3 center = context.CastPosition;

        int hitCount = Physics2D.OverlapCircle(
            center,
            impactRadius,
            hitFilter,
            hitBuffer);

        if (hitCount <= 0)
            return;

        float multiplier = skillController.HasActiveUpgrade2 ? upgrade_damageMultiplier : damageMultiplier;
        float damage = owner.Attack * multiplier;

        damagedTargets.Clear();
        for (int i = 0; i < hitCount; i++)
        {
            if (!CombatHitResolver.TryResolve(
                    hitBuffer[i],
                    enemyLayer,
                    damagedTargets,
                    out ICombatHealth combatHealth))
                continue;

            combatHealth.ApplyDamage(new DamageRequest(damage, owner, DamageOrigin.Skill));
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
