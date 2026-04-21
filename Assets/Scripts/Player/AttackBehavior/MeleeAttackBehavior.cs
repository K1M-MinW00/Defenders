using UnityEngine;

public abstract class MeleeAttackBehavior : MonoBehaviour, IAttackBehavior
{
    protected UnitController owner;
    protected MonsterController currentTarget;
    protected float lastAttackTime = -999f;
    protected bool isAttacking;

    [SerializeField] protected LayerMask targetLayer;
    [SerializeField] protected TargetSelectionMode targetMode = TargetSelectionMode.Single;

    protected float Damage => owner.Attack;
    protected float Cooldown => 1f / owner.AttackPerSec;
    public bool IsAttacking => isAttacking;

    protected virtual void Awake()
    {
        if (owner == null)
            owner = GetComponent<UnitController>();
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
        owner.SkillController.NotifyAttackStarted(target);

        owner.FaceTarget();
        owner.Animation.PlayAttack();
        isAttacking = true;

        return true;
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

    protected virtual void ApplyDamage(MonsterController hit)
    {
        if (hit.TryGetComponent<IDamageable>(out var damageable))
        {
            float damage = Damage;
            owner.SkillController.NotifyAttackHit(hit,ref damage);
            damageable.TakeDamage(damage);
        }
    }

    protected virtual void ApplyDamage(Collider2D[] hits)
    {
        foreach (var hit in hits)
        {
            if (hit.TryGetComponent<IDamageable>(out var damageable))
            {
                float damage = Damage;
                var target = hit.GetComponent<MonsterController>();
                owner.SkillController.NotifyAttackHit(target, ref damage);
                damageable.TakeDamage(damage);
            }
        }
    }
}
