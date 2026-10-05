using System.Collections.Generic;
using UnityEngine;

public class Werebear_EarthSlam_Skill : ActiveSkillBase
{
    [Header("Ground Smash")]
    [SerializeField] private float damageMultiplier = 2f;
    [SerializeField] private float upgrade_damageMultiplier = 3f;

    [SerializeField] private float impactRadius = 1.6f;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private int hitBufferSize = 32;

    [Header("Effect")]
    [SerializeField] private GameObject impactEffectPrefab;

    private Collider2D[] hitBuffer;
    private ContactFilter2D hitFilter;
    private readonly HashSet<ICombatHealth> damagedTargets = new();

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
        return PrepareSelfAreaWithEnemyInRangeContext(impactRadius, out context);
    }

    public override void OnSkillStart(SkillExecutionContext context)
    {
        if (context.EnemyTarget != null)
            owner.Animation.FaceTarget(context.EnemyTarget);

        Telegraph.ShowCircle(
            owner.transform,
            Vector3.zero,
            impactRadius,
            new Color(1f, 0.42f, 0.08f, 0.9f));
    }

    public override void OnSkillApply(SkillExecutionContext context)
    {
        Vector2 center = context.CastPosition;
        SpawnImpactEffect(center);

        float multiplier = ResolveActiveUpgrade(damageMultiplier, upgrade_damageMultiplier);
        float damage = owner.Attack * multiplier;

        SkillAreaDamageUtility.ApplyCircle(
            center,
            impactRadius,
            hitFilter,
            hitBuffer,
            enemyLayer,
            damagedTargets,
            damage,
            owner);
    }

    public override void OnSkillEnd(SkillExecutionContext context) { }

    public override void CancelSkill() { }
    private void SpawnImpactEffect(Vector2 center)
    {
        if (TrySpawnSkillObject(
                impactEffectPrefab,
                center,
                Quaternion.identity,
                PoolCategory.Effect,
                out Poolable effect) &&
            effect.TryGetComponent(out PooledVfx vfx))
            vfx.Play();
    }
#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Vector3 center = owner != null ? owner.transform.position : transform.position;

        Gizmos.color = new Color(1f, 0.5f, 0f, 0.8f);
        Gizmos.DrawWireSphere(center, impactRadius);
    }
#endif
}
