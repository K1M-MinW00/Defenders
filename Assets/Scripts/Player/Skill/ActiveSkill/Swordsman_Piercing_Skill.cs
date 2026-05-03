using Firebase.Auth;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Swordsman_Piercing_Skill : ActiveSkillBase
{
    [Header("Piercing Thrust")]
    [SerializeField] private float hitDamageMultiplier = 2.5f;
    [SerializeField] private Vector2 boxSize = new Vector2(1f, 1f);
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private int multCnt = 4;
    [SerializeField] private float hitInterval = 0.05f;

    [SerializeField] private int hitBufferSize = 32;

    [Header("Effect")]
    [SerializeField] private GameObject hitEffectPrefab;
    
    private Poolable spawnedEffect;
    private Coroutine multiHitRoutine;

    private Collider2D[] hitBuffer;
    private ContactFilter2D hitFilter;
    private readonly HashSet<IDamageable> damagedTargetsPerHit = new();

    private Vector2 origin;
    private Vector2 dir;
    private float angle;
    private Coroutine skillRoutine;

    public override ActiveSkillTargetType TargetType => ActiveSkillTargetType.EnemyInRange;
    public override SkillTargetFailPolicy TargetFailPolicy => SkillTargetFailPolicy.CancelAndRefund;

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
        SpawnHitEffect();

        origin = context.EnemyTarget.transform.position;
        dir = (Vector2)context.EnemyTarget.transform.position - (Vector2)owner.transform.position;
        dir.Normalize();

        angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        if (skillRoutine != null)
            owner.StopCoroutine(skillRoutine);

        skillRoutine = owner.StartCoroutine(CoPiercingThrust());
    }

    public override void OnSkillEnd(SkillExecutionContext context){ }

    public override void CancelSkill() 
    {
        if(skillRoutine != null)
        {
            owner.StopCoroutine(skillRoutine);
            skillRoutine = null;
        }
    }

    private IEnumerator CoPiercingThrust()
    {
        float damagePerHit = owner.Attack * hitDamageMultiplier / multCnt;

        for (int i = 0; i < multCnt; i++)
        {
            ExecuteSingleThrust(damagePerHit);

            yield return new WaitForSeconds(hitInterval);
        }

        skillRoutine = null;
    }

    private void ExecuteSingleThrust(float damage)
    {
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        int hitCount = Physics2D.OverlapBox(
            origin,
            boxSize,
            angle,
            hitFilter,
            hitBuffer
        );

        ApplyDamage(hitCount, damage);
    }

    private void ApplyDamage(int hitCount, float damage)
    {
        if (hitCount <= 0)
            return;

        damagedTargetsPerHit.Clear();

        for (int i = 0; i < hitCount; i++)
        {
            Collider2D hit = hitBuffer[i];

            if (hit == null)
                continue;

            if (!hit.TryGetComponent(out IDamageable damageable))
                continue;

            if (!damagedTargetsPerHit.Add(damageable))
                continue;

            damageable.TakeDamage(damage);
        }
    }

    private void SpawnHitEffect()
    {
        Poolable effect = owner.PoolManager.Spawn(hitEffectPrefab,origin,Quaternion.Euler(0f,0f,angle),
            PoolCategory.Effect
        );

        if (effect != null && effect.TryGetComponent(out PooledVfx vfx))
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