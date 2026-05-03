using System.Collections.Generic;
using UnityEngine;

public class Werebear_EarthSlam_Skill : ActiveSkillBase
{
    [Header("Ground Smash")]
    [SerializeField] private float damageMultiplier = 2.4f;
    [SerializeField] private float impactRadius = 1.6f;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private int hitBufferSize = 32;

    [Header("Effect")]
    [SerializeField] private GameObject impactEffectPrefab;
    private GameObject spawnedEffect;

    private Collider2D[] hitBuffer;
    private ContactFilter2D hitFilter;
    private readonly HashSet<IDamageable> damagedTargets = new();

    public override ActiveSkillTargetType TargetType => ActiveSkillTargetType.SelfArea;
    public override SkillTargetFailPolicy TargetFailPolicy => SkillTargetFailPolicy.CastWithoutTarget;
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
        context = new SkillExecutionContext();
        context.Initialize(owner);

        MonsterController target = owner.Targeting.GetClosestEnemyInRange();

        if(target != null)
            context.SetEnemyTarget(target);

        Vector3 castPos = owner.transform.position;
        context.SetCastPosition(castPos);
        return true;
    }

    public override void OnSkillStart(SkillExecutionContext context)
    {
        if(context.EnemyTarget != null)
            owner.Animation.FaceTarget(context.EnemyTarget);
    }

    public override void OnSkillApply(SkillExecutionContext context)
    {
        Vector2 center = context.CastPosition;
        SpawnImpactEffect(center);

        int hitCount = Physics2D.OverlapCircle(
                    center,
                    impactRadius,
                    hitFilter,
                    hitBuffer
                );

        if (hitCount <= 0)
            return;

        float damage = owner.Attack * damageMultiplier;

        ApplyDamage(hitCount, damage);
    }

    public override void OnSkillEnd(SkillExecutionContext context) { }

    public override void CancelSkill() { }
    private void SpawnImpactEffect(Vector2 center)
    {
        if (impactEffectPrefab == null || owner.PoolManager == null)
            return;

        Poolable effect = owner.PoolManager.Spawn(
            impactEffectPrefab,
            center,
            Quaternion.identity,
            PoolCategory.Effect
        );

        if (effect != null && effect.TryGetComponent(out PooledVfx vfx))
            vfx.Play();
    }
    private void ApplyDamage(int hitCount, float damage)
    {
        damagedTargets.Clear();

        for (int i = 0; i < hitCount; i++)
        {
            Collider2D hit = hitBuffer[i];

            if (hit == null)
                continue;

            if (!hit.TryGetComponent(out IDamageable damageable))
                continue;

            if (!damagedTargets.Add(damageable))
                continue;

            damageable.TakeDamage(damage);

            // 추후 상태이상 시스템 추가
        }
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