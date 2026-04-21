using UnityEngine;

public abstract class RangedAttackBehavior : MonoBehaviour, IAttackBehavior
{
    protected UnitController owner;
    protected MonsterController currentTarget;
    protected float lastAttackTime = -999f;
    protected bool isAttacking;

    [SerializeField] protected LayerMask targetLayer;

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

        isAttacking = true;

        owner.SkillController.NotifyAttackStarted(target);
        
        owner.FaceTarget();
        owner.Animation.PlayAttack();

        return true;
    }

    public abstract void OnAttackHit();

    public virtual void OnAttackFinished()
    {
        lastAttackTime = Time.time;
        isAttacking = false;
        currentTarget = null;
    }

    public void CancelAttack()
    {
        if (!isAttacking)
            return;

        OnAttackFinished();
    }
}
