using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Swordsman_Piercing_Skill : ActiveSkillBase
{
    [Header("Piercing Thrust")]
    [SerializeField] private float damageMultiplier = 2f;
    [SerializeField] private float upgrade_damageMultiplier = 3f;
    [SerializeField] private Vector2 boxSize = new Vector2(1f, 1f);
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private int multCnt = 4;
    [SerializeField] private float hitInterval = 0.05f;

    [SerializeField] private int hitBufferSize = 32;

    [Header("Effect")]
    [SerializeField] private GameObject hitEffectPrefab;

    private Collider2D[] hitBuffer;
    private ContactFilter2D hitFilter;
    private readonly HashSet<ICombatHealth> damagedTargetsPerHit = new();

    private Vector2 origin;
    private Vector2 dir;
    private float angle;
    private WaitForSeconds hitDelay;

    public override ActiveSkillTargetType TargetType => ActiveSkillTargetType.EnemyInRange;
    public override SkillTargetFailPolicy TargetFailPolicy => SkillTargetFailPolicy.CancelAndRefund;

    private void Awake()
    {
        hitBuffer = new Collider2D[hitBufferSize];
        hitFilter = new ContactFilter2D();
        hitFilter.useLayerMask = true;
        hitFilter.SetLayerMask(enemyLayer);
        hitFilter.useTriggers = true;
        hitDelay = new WaitForSeconds(Mathf.Max(0f, hitInterval));
    }

    public override bool TryBuildContext(out SkillExecutionContext context)
    {
        return TryPrepareEnemyWithinRangeContext(owner.DetectRange, out context);
    }

    public override void OnSkillStart(SkillExecutionContext context)
    {
        if (context.EnemyTarget != null)
            owner.Animation.FaceTarget(context.EnemyTarget);
    }

    public override void OnSkillApply(SkillExecutionContext context)
    {
        origin = context.EnemyTarget.TargetTransform.position;
        dir = (Vector2)context.EnemyTarget.TargetTransform.position - (Vector2)owner.transform.position;
        dir.Normalize();

        angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        SpawnHitEffect();

        StartExecutionCoroutine(CoPiercingThrust());
    }

    public override void OnSkillEnd(SkillExecutionContext context){ }

    public override void CancelSkill() 
    {
        damagedTargetsPerHit.Clear();
    }

    private IEnumerator CoPiercingThrust()
    {
        int hitCount = Mathf.Max(1, multCnt);
        float multiplier = ResolveActiveUpgrade(damageMultiplier, upgrade_damageMultiplier);
        float damagePerHit = owner.Attack * multiplier / hitCount;

        for (int i = 0; i < hitCount; i++)
        {
            ExecuteSingleThrust(damagePerHit);

            if (i < hitCount - 1)
                yield return hitDelay;
        }

    }

    private void ExecuteSingleThrust(float damage)
    {
        SkillAreaDamageUtility.ApplyBox(
            origin,
            boxSize,
            angle,
            hitFilter,
            hitBuffer,
            enemyLayer,
            damagedTargetsPerHit,
            damage,
            owner);
    }

    private void SpawnHitEffect()
    {
        if (TrySpawnSkillObject(
                hitEffectPrefab,
                origin,
                Quaternion.Euler(0f, 0f, angle),
                PoolCategory.Effect,
                out Poolable effect) &&
            effect.TryGetComponent(out PooledVfx vfx))
            vfx.Play();
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Vector2 drawOrigin = Application.isPlaying ? origin : transform.position;
        float drawAngle = Application.isPlaying ? angle : 0f;

        Matrix4x4 old = Gizmos.matrix;
        Gizmos.matrix = Matrix4x4.TRS(drawOrigin, Quaternion.Euler(0f, 0f, drawAngle), Vector3.one);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(Vector3.zero, boxSize);

        Gizmos.matrix = old;
    }
#endif
}
