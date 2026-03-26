using UnityEngine;

public abstract class RangedAttackBehavior : MonoBehaviour, IAttackBehavior
{
    [Header("References")]
    [SerializeField] protected UnitController owner;

    [Header("Combat")]
    protected float Damage => owner.Attack;
    protected float cooldown => 1f / owner.AttackPerSec;

    [SerializeField] protected LayerMask targetLayer;

    protected Transform pendingTarget;
    protected float lastAttackTime = -999f;
    protected bool isAttacking;
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

        if (Time.time < lastAttackTime + cooldown)
            return false;

        return true;
    }

    public virtual bool TryAttack(MonsterController target)
    {
        if (target == null || target.Health.IsDead)
            return false;

        if (!CanAttack())
            return false;


        pendingTarget = target.transform;

        owner.FaceTarget();
        owner.PlayAttack();
        isAttacking = true;

        return true;
    }

    public abstract void OnAttackHit();

    public virtual void OnAttackFinished()
    {
        lastAttackTime = Time.time;
        isAttacking = false;
        pendingTarget = null;
    }

    public void CancelAttack()
    {
        if (!isAttacking)
            return;
        
        OnAttackFinished();
    }
}
