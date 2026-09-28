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
    private Poolable spawnedEffect;
    private Collider2D[] hitBuffer;
    private ContactFilter2D hitFilter;
    private readonly System.Collections.Generic.HashSet<ICombatHealth> damagedTargets = new();

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

        radius = owner.DetectRange;
        context.SetCastPosition(owner.transform.position);

        ICombatTarget target = owner.Targeting.GetClosestEnemyInRange();

        if (target != null)
            context.SetEnemyTarget(target);

        // 자기 위치 기준
        return true;
    }

    public override void OnSkillStart(SkillExecutionContext context)
    {
        if (context.EnemyTarget != null)
            owner.Animation.FaceTarget(context.EnemyTarget);

        SpawnEffect();
    }

    public override void OnSkillApply(SkillExecutionContext context)
    {
        Vector2 center = owner.transform.position;

        float multiplier = skillController.HasActiveUpgrade2 ? upgrade_damageMultiplier : damageMultiplier;
        float damage = owner.Attack * multiplier;

        int hitCount = Physics2D.OverlapCircle(center, radius, hitFilter, hitBuffer);

        if (hitCount <= 0)
            return;

        damagedTargets.Clear();

        for (int i = 0; i < hitCount; i++)
        {
            Collider2D hit = hitBuffer[i];

            if (hit == null)
                continue;

            if (!CombatHitResolver.TryResolve(
                    hit,
                    enemyLayer,
                    damagedTargets,
                    out ICombatHealth combatHealth))
                continue;

            combatHealth.ApplyDamage(new DamageRequest(damage, owner, DamageOrigin.Skill));
        }
    }

    public override void OnSkillEnd(SkillExecutionContext context)
    {
        ReturnEffect();
    }

    public override void CancelSkill()
    {
        ReturnEffect();
    }

    private void SpawnEffect()
    {
        spawnedEffect = owner.PoolManager.Spawn(spinEffectPrefab, owner.transform.position, Quaternion.identity, PoolCategory.Effect, owner.transform);
    }

    private void ReturnEffect()
    {
        if (spawnedEffect == null)
            return;

        spawnedEffect.ReturnToPool();
        spawnedEffect = null;
    }


#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
#endif
}
