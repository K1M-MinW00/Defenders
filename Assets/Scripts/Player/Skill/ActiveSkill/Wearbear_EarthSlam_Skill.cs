using UnityEngine;

public class Werebear_EarthSlam_Skill : ActiveSkillBase
{
    [Header("Ground Smash")]
    [SerializeField] private float damageMultiplier = 2.4f;
    [SerializeField] private float impactRadius = 1.6f;
    [SerializeField] private LayerMask enemyLayer;

    [Header("Effect")]
    [SerializeField] private GameObject impactEffectPrefab;
    private GameObject spawnedEffect;

    public override ActiveSkillTargetType TargetType => ActiveSkillTargetType.SelfArea;
    public override SkillTargetFailPolicy TargetFailPolicy => SkillTargetFailPolicy.CastWithoutTarget;

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

        if (impactEffectPrefab != null)
            spawnedEffect = Instantiate(impactEffectPrefab, center, Quaternion.identity);
        

        Collider2D[] hits = Physics2D.OverlapCircleAll(center, impactRadius, enemyLayer);
        if (hits == null || hits.Length == 0)
            return;

        float damage = owner.Attack * damageMultiplier;

        foreach (Collider2D hit in hits)
        {
            if (hit.TryGetComponent<IDamageable>(out var damageable))
            {
                damageable.TakeDamage(damage);

                // 추후 상태이상 시스템 추가:
                // if (hit.TryGetComponent<IStatusAffectable>(out var statusTarget))
                // {
                //     statusTarget.ApplyStun(stunDuration);
                // }
            }
        }
    }

    public override void OnSkillEnd(SkillExecutionContext context)
    {
        if (spawnedEffect != null)
        {
            spawnedEffect.SetActive(false);
            Destroy(spawnedEffect);
            spawnedEffect = null;
        }
    }

    public override void CancelSkill()
    {
        if (spawnedEffect != null)
        {
            spawnedEffect.SetActive(false);
            Destroy(spawnedEffect);
            spawnedEffect = null;
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.8f);
        Gizmos.DrawWireSphere(transform.position, impactRadius);
    }
#endif
}