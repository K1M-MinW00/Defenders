using UnityEngine;

public class KnightTemplar_LightBeam_Skill : ActiveSkillBase
{
    [Header("Light Beam")]
    [SerializeField] private float damageMultiplier = 2.5f;
    [SerializeField] private float beamLength = 40f;
    [SerializeField] private float beamWidth = 1.2f;
    [SerializeField] private LayerMask enemyLayer;

    [Header("Effect")]
    [SerializeField] private LightBeam beamEffectPrefab;

    private LightBeam spawnedEffect;

    public override ActiveSkillTargetType TargetType => ActiveSkillTargetType.EnemyInRangeOrGlobalClosest;
    public override SkillTargetFailPolicy TargetFailPolicy => SkillTargetFailPolicy.WaitUntilFound;

    public override bool TryBuildContext(out SkillExecutionContext context)
    {
        context = new SkillExecutionContext();
        context.Initialize(owner);

        MonsterController target = owner.Targeting.GetClosestEnemyInRange();

        if (target == null)
        {
            bool enemyGlobal = owner.Targeting.FindGlobalAliveMonster();
            
            if (!enemyGlobal)
                return false;

            target = owner.Targeting.CurrentTarget;
        }

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
        SpawnBeamEffect(context);
    }

    public override void OnSkillEnd(SkillExecutionContext context) 
    {
        ReturnBeamEffect();
    }

    public override void CancelSkill()
    {
        ReturnBeamEffect();
    }

    private void SpawnBeamEffect(SkillExecutionContext context)
    {
        if (beamEffectPrefab == null || owner.PoolManager == null)
            return;

        Vector2 origin = owner.transform.position;
        Vector2 targetPos = context.EnemyTarget.transform.position;

        Vector2 dir = (targetPos - origin).normalized;
        
        Vector2 center = origin + dir * (beamLength * 0.5f);
        
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.Euler(0f, 0f, angle);

        float damamge = owner.Attack * damageMultiplier;

        spawnedEffect = owner.PoolManager.Spawn(beamEffectPrefab,center,rotation,PoolCategory.Effect);

        if (spawnedEffect == null)
            return;

        spawnedEffect.Initialize(center, dir, beamLength, beamWidth, damamge, enemyLayer);
    }

    private void ReturnBeamEffect()
    {
        if (spawnedEffect == null)
            return;

        spawnedEffect.ReturnToPool();
        spawnedEffect = null;
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