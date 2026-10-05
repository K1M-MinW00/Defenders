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
    private DropSpawnView presentation;
    private Vector2 pendingImpactCenter;
    private bool impactPending;

    public override ActiveSkillTargetType TargetType => ActiveSkillTargetType.SelfArea;
    public override SkillTargetFailPolicy TargetFailPolicy => SkillTargetFailPolicy.CancelAndRefund;

    private void Awake()
    {
        presentation = GetComponent<DropSpawnView>();
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
            new Color(1f, 0.35f, 0.08f, 0.9f));
    }

    public override void OnSkillApply(SkillExecutionContext context)
    {
        pendingImpactCenter = owner.transform.position;
        impactPending = true;

        if (presentation != null)
        {
            presentation.PlaySkillLeap(ApplyLandingImpact);
            return;
        }

        ApplyLandingImpact();
    }

    private void ApplyLandingImpact()
    {
        if (!impactPending)
            return;

        impactPending = false;

        if (owner == null || owner.IsDead || !owner.gameObject.activeInHierarchy)
            return;

        float multiplier = ResolveActiveUpgrade(damageMultiplier, upgrade_damageMultiplier);
        float damage = owner.Attack * multiplier;

        SkillAreaDamageUtility.ApplyCircle(
            pendingImpactCenter,
            impactRadius,
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
        impactPending = false;
        presentation?.CancelSkillLeap();
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, impactRadius);
    }
#endif
}
