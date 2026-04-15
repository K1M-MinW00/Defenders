using System.Collections;
using UnityEngine;

public class Swordsman_Piercing_Skill : ActiveSkillBase
{
    [Header("Piercing Thrust")]
    [SerializeField] private float hitDamageMultiplier = 2.5f;
    [SerializeField] private Vector2 boxSize = new Vector2(1f, 1f);
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private int multCnt = 4;

    [Header("Effect")]
    [SerializeField] private GameObject hitEffectPrefab;
    private GameObject spawnedEffect;

    public override ActiveSkillTargetType TargetType => ActiveSkillTargetType.EnemyInRange;
    public override SkillTargetFailPolicy TargetFailPolicy => SkillTargetFailPolicy.CancelAndRefund;

    public override bool TryBuildContext(out SkillExecutionContext context)
    {
        context = new SkillExecutionContext();
        context.Initialize(owner);

        MonsterController target = owner.Targeting.GetClosestEnemyInRange();
        if (target == null)
            return false;

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
        Vector3 spawnPos = context.EnemyTarget.transform.position;
        spawnedEffect = Instantiate(hitEffectPrefab, spawnPos, Quaternion.identity);

        Collider2D[] hits = Physics2D.OverlapBoxAll(spawnPos, boxSize, 0f, enemyLayer);

        if (hits == null || hits.Length == 0)
            return;

        float damagePerHit = owner.Attack * (hitDamageMultiplier / multCnt);
        owner.StartCoroutine(ApplyMultHit(hits, damagePerHit));
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

    private IEnumerator ApplyMultHit(Collider2D[] hits, float damagePerHit)
    {
        for (int i = 0; i < multCnt; ++i)
        {
            foreach (Collider2D hit in hits)
            {
                if (hit != null && hit.TryGetComponent<IDamageable>(out var damageable))
                    damageable.TakeDamage(damagePerHit);
            }

            yield return new WaitForSeconds(.05f);
        }
    }
}