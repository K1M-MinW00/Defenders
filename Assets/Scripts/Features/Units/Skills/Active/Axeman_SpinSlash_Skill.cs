using UnityEngine;

public class Axeman_SpinSlash_Skill : ActiveSkillBase
{
    [Header("Spin Slash")]
    [SerializeField] private float damageMultiplier = 2f;
    [SerializeField] private float upgrade_damageMultiplier = 3f;
    [SerializeField] private LayerMask enemyLayer;

    [Header("Effect")]
    [SerializeField] private GameObject spinEffectPrefab;
    [SerializeField] private int hitBufferSize = 32;

    private float radius = 3f;
    private Collider2D[] hitBuffer;
    private ContactFilter2D hitFilter;
    private readonly System.Collections.Generic.HashSet<ICombatHealth> damagedTargets = new();

    public override ActiveSkillTargetType TargetType => ActiveSkillTargetType.SelfArea;
    public override SkillTargetFailPolicy TargetFailPolicy => SkillTargetFailPolicy.CancelAndRefund;

    private void Awake()
    {
        hitBuffer = new Collider2D[hitBufferSize];

        hitFilter = new ContactFilter2D();
        hitFilter.useLayerMask = true;
        hitFilter.SetLayerMask(enemyLayer);
        hitFilter.useTriggers = true;
    }

    public override bool TryBuildContext(out SkillExecutionContext context)
    {
        radius = owner.DetectRange;
        return PrepareSelfAreaWithEnemyInRangeContext(radius, out context);
    }

    public override void OnSkillStart(SkillExecutionContext context)
    {
        if (context.EnemyTarget != null)
            owner.Animation.FaceTarget(context.EnemyTarget);

        Telegraph.ShowCircle(
            owner.transform,
            Vector3.zero,
            radius,
            new Color(1f, 0.12f, 0.08f, 0.9f));
        SpawnEffect();
    }

    public override void OnSkillApply(SkillExecutionContext context)
    {
        Vector2 center = owner.transform.position;

        float multiplier = ResolveActiveUpgrade(damageMultiplier, upgrade_damageMultiplier);
        float damage = owner.Attack * multiplier;

        SkillAreaDamageUtility.ApplyCircle(
            center,
            radius,
            hitFilter,
            hitBuffer,
            enemyLayer,
            damagedTargets,
            damage,
            owner);
    }

    public override void OnSkillEnd(SkillExecutionContext context)
    {
    }

    public override void CancelSkill()
    {
    }

    private void SpawnEffect()
    {
        if (TrySpawnSkillObject(
            spinEffectPrefab,
            owner.transform.position,
            Quaternion.identity,
            PoolCategory.Effect,
            out Poolable spawnedEffect,
            owner.transform))
        {
            TrackExecutionEffect(spawnedEffect);
        }
    }


#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
#endif
}
