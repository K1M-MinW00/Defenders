using System.Collections.Generic;
using UnityEngine;

public abstract class MeleeAttackBehavior : MonoBehaviour, IAttackBehavior
{
    [Header("References")]
    protected UnitController owner;
    protected MonsterController currentTarget;

    [Header("Combat")]
    [SerializeField] protected LayerMask targetLayer;

    [Header("Target")]
    [SerializeField] protected int multiTargetUnlockStar = 3;

    protected float Damage => owner.Attack;
    protected float Cooldown => 1f / owner.AttackPerSec;
    
    protected float lastAttackTime = -999f;
    protected bool isAttacking;
    public bool IsAttacking => isAttacking;

    protected virtual void Awake()
    {
        if (owner == null)
            owner = GetComponent<UnitController>();
    }


    protected TargetSelectionMode CurrentTargetMode =>
        owner != null && owner.Star >= multiTargetUnlockStar
        ? TargetSelectionMode.Multi 
        : TargetSelectionMode.Single;

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
    }

    public void CancelAttack()
    {
        if (!isAttacking)
            return;

        OnAttackFinished();
    }

    protected virtual void ApplyDamage(MonsterController target)
    {
        if (target == null || target.Health.IsDead)
            return;

        float damage = Damage;
        owner.SkillController.NotifyAttackHit(target, ref damage);
        target.Health.TakeDamage(damage);
    }

    protected virtual void ApplyDamage(Collider2D[] hits)
    {
        if (hits == null || hits.Length == 0)
            return;

        HashSet<IDamageable> damagedTargets = new();

        foreach (var hit in hits)
        {
            if (!hit.TryGetComponent<IDamageable>(out var damageable))
                continue;

            if (!damagedTargets.Add(damageable))
                continue;

            var target = hit.GetComponent<MonsterController>();

            float damage = Damage;
            owner.SkillController.NotifyAttackHit(target, ref damage);
            damageable.TakeDamage(damage);
        }
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
