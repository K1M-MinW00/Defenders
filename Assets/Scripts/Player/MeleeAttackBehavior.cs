using UnityEngine;

public abstract class MeleeAttackBehavior : MonoBehaviour, IAttackBehavior
{
    [Header("References")]
    [SerializeField] protected PlayerCharacter owner;

    [Header("Combat")]
    protected float Damage => owner.Atk;
    protected float Cooldown => 1f / owner.AttackPerSec;

    [SerializeField] protected LayerMask targetLayer;
    [SerializeField] protected string attackTrigger = "Attack";

    [Header("Hit Direction")]
    public float hitRadius = .6f;

    protected float lastAttackTime = -999f;
    protected bool isAttacking;
    public bool IsAttacking => isAttacking;
    protected MonsterController currentTarget;


    protected virtual void Awake()
    {
        if (owner == null)
            owner = GetComponent<PlayerCharacter>();
    }

    public virtual bool CanAttack()
    {
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

        owner.FaceTo(target.transform.position);

        isAttacking = true;
        owner.animator.SetTrigger(attackTrigger);

        return true;
    }

    public abstract void OnAttackHit();

    public virtual void OnAttackFinished()
    {
        isAttacking = false;
        lastAttackTime = Time.time;
        currentTarget = null;
    }

    protected void ApplyDamage(Collider2D[] hits)
    {
        foreach (var hit in hits)
        {
            if (hit.TryGetComponent<IDamageable>(out var damageable))
                damageable.TakeDamage(Damage);
        }
    }
    protected Vector2 GetAttackDirection()
    {
        if (currentTarget != null && !currentTarget.Health.IsDead)
        {
            Vector2 dirToTarget = ((Vector2)currentTarget.transform.position - (Vector2)transform.position);
            return dirToTarget.normalized;
        }

        return owner.GetFacingDirection();
    }
}
