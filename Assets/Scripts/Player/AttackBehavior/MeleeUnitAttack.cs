using System.Collections.Generic;
using UnityEngine;

public abstract class MeleeUnitAttack : MonoBehaviour, IUnitAttack
{
    [Header("References")]
    protected UnitController owner;
    protected MonsterController currentTarget;

    [Header("Combat")]
    [SerializeField] protected LayerMask targetLayer;
    [SerializeField] protected int hitBufferSize = 32;

    [Header("Target")]
    [SerializeField] protected int multiTargetUnlockStar = 3;

    protected Collider2D[] hitBuffer;
    protected ContactFilter2D hitFilter;
    protected readonly HashSet<IDamageable> damagedTargets = new();

    protected float Damage => owner.Attack;
    protected float Cooldown => 1f / owner.AttackPerSec;

    protected float lastAttackTime = -999f;
    protected bool isAttacking;

    protected TargetSelectionMode CurrentTargetMode =>
        owner != null && owner.Star >= multiTargetUnlockStar
        ? TargetSelectionMode.Multi
        : TargetSelectionMode.Single;

    public bool IsAttacking => isAttacking;

    protected virtual void Awake()
    {
        if (owner == null)
            owner = GetComponent<UnitController>();

        hitBuffer = new Collider2D[hitBufferSize];

        hitFilter = new ContactFilter2D();
        hitFilter.useLayerMask = true;
        hitFilter.SetLayerMask(targetLayer);
        hitFilter.useTriggers = true;
    }

    public virtual bool CanAttack()
    {
        if (owner == null || owner.IsDead)
            return false;

        if (isAttacking)
            return false;

        if (Time.time < lastAttackTime + Cooldown)
            return false;

        return true;
    }

    public virtual bool TryAttack(MonsterController target)
    {
        if (target == null || target.Health.IsDead)
            return false;

        if (!CanAttack())
            return false;

        currentTarget = target;
        isAttacking = true;

        owner.SkillController.NotifyAttackStarted(target);
        owner.FaceTarget();
        owner.Animation.PlayAttack();

        return true;
    }

    public abstract void OnAttackHit();

    public virtual void OnAttackFinished()
    {
        isAttacking = false;
        lastAttackTime = Time.time;
        currentTarget = null;
        damagedTargets.Clear();
    }

    public void CancelAttack()
    {
        if (!isAttacking)
            return;

        isAttacking = false;
        currentTarget = null;
        damagedTargets.Clear();
    }

    protected virtual void ApplyDamage(MonsterController target)
    {
        if (target == null || target.Health.IsDead)
            return;

        float damage = Damage;
        owner.SkillController.NotifyAttackHit(target, ref damage);
        target.Health.TakeDamage(damage);
    }

    protected virtual void ApplyDamage(Collider2D[] hits,int hitCount)
    {
        if (hits == null || hits.Length == 0)
            return;

        damagedTargets.Clear();

        for (int i=0;i< hitCount;i++)
        {
            Collider2D hit = hits[i];

            if (hit == null)
                continue;

            if (!hit.TryGetComponent<IDamageable>(out var damageable))
                continue;

            if (!damagedTargets.Add(damageable))
                continue;

            MonsterController target = hit.GetComponent<MonsterController>();

            float damage = Damage;
            owner.SkillController.NotifyAttackHit(target, ref damage);
            damageable.TakeDamage(damage);
        }
    }

    protected int OverlapBox(Vector2 center, Vector2 size, float angle)
    {
        return Physics2D.OverlapBox(center,size,angle,hitFilter,hitBuffer);
    }

    protected int OverlapCircle(Vector2 center, float radius)
    {
        return Physics2D.OverlapCircle(center, radius, hitFilter, hitBuffer);
    }

    protected void SpawnVFX(GameObject vfxPrefab, Vector2 position, float angle)
    {
        if (owner.PoolManager == null || vfxPrefab == null)
            return;

        Poolable poolable = owner.PoolManager.Spawn(vfxPrefab, position, Quaternion.Euler(0f, 0f, angle), PoolCategory.Effect);

        if (poolable != null && poolable.TryGetComponent(out PooledVfx vfx))
            vfx.Play();
    }

    protected Vector2 GetAttackDirection()
    {
        if (currentTarget != null && !currentTarget.Health.IsDead)
        {
            Vector2 dirToTarget = ((Vector2)currentTarget.transform.position - (Vector2)transform.position);
            return dirToTarget.normalized;
        }

        return owner.Animation.GetFacingDirection();
    }

}
