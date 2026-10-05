using System.Collections.Generic;
using UnityEngine;

public class SoldierR_ArrowRain_Skill : ActiveSkillBase
{
    [Header("Arrow Rain")]
    [Tooltip("화살 한 발의 공격력 배율입니다. 기본 총 기대 피해는 약 2.1배입니다.")]
    [SerializeField] private float damageMultiplier = 0.35f;
    [Tooltip("강화 후 화살 한 발의 공격력 배율입니다. 총 기대 피해는 약 3배입니다.")]
    [SerializeField] private float upgrade_damageMultiplier = 0.3f;
    
    [SerializeField] private int arrowCount = 6;
    [SerializeField] private int upgrade_arrowCount = 10;

    [SerializeField] private float rainRadius = 1.5f;
    [SerializeField] private float hitRadius = 0.4f;

    [SerializeField] private LayerMask enemyLayer;

    [Header("Arrow Rain Visual")]
    [SerializeField] private ArrowRainFallingArrow fallingArrowPrefab;
    [SerializeField] private float spawnHeight = 3.5f;
    [SerializeField] private float horizontalScatter = 0.15f;

#if UNITY_EDITOR
    private readonly List<Vector2> debugLandingPoints = new();
    private Vector2 debugCenter;
#endif

    public override ActiveSkillTargetType TargetType => ActiveSkillTargetType.EnemyInRange;
    public override SkillTargetFailPolicy TargetFailPolicy => SkillTargetFailPolicy.WaitUntilFound;
    public override TargetResolutionPolicy ResolutionPolicy => TargetResolutionPolicy.LockPosition;

    public override bool CanApply(SkillExecutionContext context)
    {
        return context != null && context.IsValid;
    }

    public override bool TryBuildContext(out SkillExecutionContext context)
    {
        if (!TryPrepareEnemyInRangeContext(out context))
            return false;

        context.SetCastPosition(context.EnemyTarget.TargetTransform.position);
        return true;
    }

    public override void OnSkillStart(SkillExecutionContext context)
    {
        if (context.EnemyTarget != null)
            owner.Animation.FaceTarget(context.EnemyTarget);

        Telegraph.ShowCircle(
            context.CastPosition,
            rainRadius,
            new Color(1f, 0.25f, 0.08f, 0.9f));
    }

    public override void OnSkillApply(SkillExecutionContext context)
    {
        Vector2 center = context.CastPosition;

        float multiplier = ResolveActiveUpgrade(damageMultiplier, upgrade_damageMultiplier);
        float damage = owner.Attack * multiplier;

#if UNITY_EDITOR
        debugCenter = center;
        debugLandingPoints.Clear();
#endif

        int count = ResolveActiveUpgrade(arrowCount, upgrade_arrowCount);
        for (int i = 0; i < count; i++)
        {
            Vector2 landingOffset = Random.insideUnitCircle * rainRadius;
            Vector2 landingPoint = center + landingOffset;

#if UNITY_EDITOR
            debugLandingPoints.Add(landingPoint);
#endif

            Vector2 spawnOffset = new Vector2(
                Random.Range(-horizontalScatter, horizontalScatter),
                0f
            );

            Vector3 spawnPos = (Vector3)(landingPoint + spawnOffset) + Vector3.up * spawnHeight;

            if (!TrySpawnSkillObject(
                    fallingArrowPrefab,
                    spawnPos,
                    Quaternion.identity,
                    PoolCategory.Projectile,
                    out ArrowRainFallingArrow fallingArrow))
                continue;

            fallingArrow.Initialize(landingPoint, damage, hitRadius, enemyLayer, owner);
        }
    }

    public override void OnSkillEnd(SkillExecutionContext context) { }

    public override void CancelSkill() { }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(debugCenter, rainRadius);

        Gizmos.color = Color.red;

        for (int i = 0; i < debugLandingPoints.Count; i++)
        {
            Vector2 point = debugLandingPoints[i];

            Gizmos.DrawWireSphere(point, hitRadius);
            Gizmos.DrawSphere(point, 0.05f);
        }
    }
#endif
}
