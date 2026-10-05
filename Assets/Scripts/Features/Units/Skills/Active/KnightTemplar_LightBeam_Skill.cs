using UnityEngine;

public class KnightTemplar_LightBeam_Skill : ActiveSkillBase
{
    [Header("Light Beam")]
    [SerializeField] private float damageMultiplier = 2.5f;
    [SerializeField] private float upgrade_damageMultiplier = 3f;

    [SerializeField] private float beamLength = 40f;
    [SerializeField] private float beamWidth = 1.2f;
    [SerializeField] private LayerMask enemyLayer;

    [Header("Effect")]
    [SerializeField] private LightBeam beamEffectPrefab;

    public override ActiveSkillTargetType TargetType => ActiveSkillTargetType.EnemyInRange;
    public override SkillTargetFailPolicy TargetFailPolicy => SkillTargetFailPolicy.WaitUntilFound;

    public override bool TryBuildContext(out SkillExecutionContext context)
    {
        return TryPrepareEnemyInRangeContext(out context);
    }

    public override void OnSkillStart(SkillExecutionContext context)
    {
        if (context.EnemyTarget != null)
        {
            owner.Animation.FaceTarget(context.EnemyTarget);
            Telegraph.ShowLine(
                owner.transform,
                context.EnemyTarget.TargetTransform,
                beamLength,
                new Color(1f, 0.85f, 0.2f, 0.95f));
        }
    }

    public override void OnSkillApply(SkillExecutionContext context)
    {
        SpawnBeamEffect(context);
    }

    public override void OnSkillEnd(SkillExecutionContext context)
    {
    }

    public override void CancelSkill()
    {
    }

    private void SpawnBeamEffect(SkillExecutionContext context)
    {
        Vector2 origin = owner.transform.position;
        Vector2 targetPos = context.EnemyTarget.TargetTransform.position;

        Vector2 dir = (targetPos - origin).normalized;

        Vector2 center = origin + dir * (beamLength * 0.5f);

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.Euler(0f, 0f, angle);

        float multiplier = ResolveActiveUpgrade(damageMultiplier, upgrade_damageMultiplier);
        float damamge = owner.Attack * multiplier;

        if (!TrySpawnSkillObject(
                beamEffectPrefab,
                center,
                rotation,
                PoolCategory.Effect,
                out LightBeam spawnedEffect))
            return;

        TrackExecutionEffect(spawnedEffect);
        spawnedEffect.Initialize(center, dir, beamLength, beamWidth, damamge, enemyLayer, owner);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;

        Vector2 origin = transform.position;
        Vector2 dir = Vector2.right;

        Vector2 center = origin + dir * (beamLength * 0.5f);

        Matrix4x4 old = Gizmos.matrix;
        Gizmos.matrix = Matrix4x4.TRS(center, Quaternion.identity, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(beamLength, beamWidth, 1));
        Gizmos.matrix = old;
    }
#endif
}
