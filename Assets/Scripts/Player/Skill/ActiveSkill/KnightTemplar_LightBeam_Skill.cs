using UnityEngine;

public class KnightTemplar_LightBeam_Skill : ActiveSkillBase
{
    [Header("Light Beam")]
    [SerializeField] private float damageMultiplier = 2.5f;
    [SerializeField] private float beamLength = 4f;
    [SerializeField] private float beamWidth = 1.2f;
    [SerializeField] private LayerMask enemyLayer;

    [Header("Effect")]
    [SerializeField] private GameObject beamEffectPrefab;
    private GameObject spawnedEffect;

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
        if (context.EnemyTarget == null)
            return;

        Vector2 origin = owner.transform.position;
        Vector2 targetPos = context.EnemyTarget.transform.position;

        Vector2 dir = (targetPos - origin);
        if (dir.sqrMagnitude <= 0.0001f)
            dir = owner.Animation.GetFacingDirection();

        dir.Normalize();

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        // 박스 중심 (앞쪽으로 절반 이동)
        Vector2 center = origin + dir * (beamLength * 0.5f);

        // 이펙트
        if (beamEffectPrefab != null)
        {
            Quaternion rot = Quaternion.Euler(0f, 0f, angle);
            spawnedEffect = Instantiate(beamEffectPrefab, center, rot);
            var sr = spawnedEffect.GetComponent<SpriteRenderer>();
            sr.size = new Vector2(beamLength, beamWidth);
        }

        // 판정
        Vector2 boxSize = new Vector2(beamLength, beamWidth);

        Collider2D[] hits = Physics2D.OverlapBoxAll(center, boxSize, angle, enemyLayer);

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