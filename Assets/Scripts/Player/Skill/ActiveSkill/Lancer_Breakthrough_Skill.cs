using UnityEngine;

public class Lancer_Breakthrough_Skill : ActiveSkillBase
{
    [Header("Lance Breakthrough")]
    [SerializeField] private float damageMultiplier = 2.8f;
    [SerializeField] private float lineLength = 3.2f;
    [SerializeField] private float lineWidth = 0.9f;
    [SerializeField] private LayerMask enemyLayer;

    [Header("Effect")]
    [SerializeField] private GameObject chargeTrailEffectPrefab;

    public override ActiveSkillTargetType TargetType => ActiveSkillTargetType.EnemyInRange;
    public override SkillTargetFailPolicy TargetFailPolicy => SkillTargetFailPolicy.WaitUntilFound;

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
        if (context.EnemyTarget == null)
            return;

        Vector2 origin = owner.transform.position;
        Vector2 targetPos = context.EnemyTarget.transform.position;

        Vector2 dir = targetPos - origin;
        dir.Normalize();

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        Vector2 center = origin + dir * (lineLength * 0.5f);

        if (chargeTrailEffectPrefab != null)
        {
            Instantiate(chargeTrailEffectPrefab, center, Quaternion.Euler(0f, 0f, angle));
        }

        Collider2D[] hits = Physics2D.OverlapBoxAll(
            center,
            new Vector2(lineLength, lineWidth),
            angle,
            enemyLayer
        );

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
}