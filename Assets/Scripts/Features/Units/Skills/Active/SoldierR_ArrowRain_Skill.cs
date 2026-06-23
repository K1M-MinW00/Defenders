using System.Collections.Generic;
using UnityEngine;

public class SoldierR_ArrowRain_Skill : ActiveSkillBase
{
    [Header("Arrow Rain")]
    [SerializeField] private float damageMultiplier = 1.2f;
    [SerializeField] private int arrowCount = 6;
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

    public override bool TryBuildContext(out SkillExecutionContext context)
    {
        context = new SkillExecutionContext();
        context.Initialize(owner);

        MonsterController target = owner.Targeting.GetClosestEnemyInRange();
        if (target == null)
            return false;

        context.SetEnemyTarget(target);
        context.SetCastPosition(target.transform.position);
        return true;
    }

    public override void OnSkillStart(SkillExecutionContext context)
    {
        if (context.EnemyTarget != null)
            owner.Animation.FaceTarget(context.EnemyTarget);
    }

    public override void OnSkillApply(SkillExecutionContext context)
    {
        Vector2 center = context.CastPosition;
        float damage = owner.Attack * damageMultiplier;

#if UNITY_EDITOR
        debugCenter = center;
        debugLandingPoints.Clear();
#endif

        for (int i = 0; i < arrowCount; i++)
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

            ArrowRainFallingArrow fallingArrow = owner.PoolManager.Spawn(
                fallingArrowPrefab,
                spawnPos,
                Quaternion.identity,
                PoolCategory.Projectile
            );

            if (fallingArrow == null)
                continue;

            fallingArrow.Initialize(landingPoint, damage, hitRadius, enemyLayer);
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