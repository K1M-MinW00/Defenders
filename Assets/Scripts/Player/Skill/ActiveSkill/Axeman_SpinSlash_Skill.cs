using UnityEngine;

public class Axeman_SpinSlash_Skill : ActiveSkillBase
{
    [Header("Spin Slash")]
    [SerializeField] private float damageMultiplier = 2.0f;
    [SerializeField] private float radius = 1.5f;
    [SerializeField] private LayerMask enemyLayer;

    [Header("Effect")]
    [SerializeField] private GameObject spinEffectPrefab;
    private GameObject spawnedEffect;

    public override ActiveSkillTargetType TargetType => ActiveSkillTargetType.SelfArea;
    public override SkillTargetFailPolicy TargetFailPolicy => SkillTargetFailPolicy.CastWithoutTarget;

    public override bool TryBuildContext(out SkillExecutionContext context)
    {
        context = new SkillExecutionContext();
        context.Initialize(owner);

        MonsterController target = owner.Targeting.GetClosestEnemyInRange();
        if (target == null)
            return false;

        context.SetEnemyTarget(target);

        // 자기 위치 기준
        context.SetCastPosition(owner.transform.position);
        return true;
    }

    public override void OnSkillStart(SkillExecutionContext context)
    {
        if(context.EnemyTarget !=  null)
            owner.Animation.FaceTarget(context.EnemyTarget);

        if (spinEffectPrefab != null)
            spawnedEffect = Instantiate(spinEffectPrefab, owner.transform.position, Quaternion.identity);
    }

    public override void OnSkillApply(SkillExecutionContext context)
    {
        Vector2 center = owner.transform.position;

        Collider2D[] hits = Physics2D.OverlapCircleAll(center, radius, enemyLayer);
        if (hits == null || hits.Length == 0)
            return;

        float damage = owner.Attack * damageMultiplier;

        foreach (var hit in hits)
        {
            if (hit.TryGetComponent<IDamageable>(out var dmg))
            {
                dmg.TakeDamage(damage);
            }
        }
    }

    public override void OnSkillEnd(SkillExecutionContext context) 
    {
        if(spawnedEffect != null)
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
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
#endif
}