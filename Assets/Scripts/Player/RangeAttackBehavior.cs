using UnityEngine;

public abstract class RangedAttackBehavior : MonoBehaviour, IAttackBehavior
{
    [Header("References")]
    [SerializeField] protected PlayerCharacter owner;

    [Header("Combat")]
    protected float Damage => owner.Atk;
    protected float cooldown => 1f / owner.AttackPerSec;

    [SerializeField] protected LayerMask targetLayer;
    [SerializeField] protected string attackTrigger = "Attack";

    protected Transform pendingTarget;
    protected float lastAttackTime = -999f;
    protected bool isAttacking;
    public bool IsAttacking => isAttacking;

    protected virtual void Awake()
    {
        if (owner == null)
            owner = GetComponent<PlayerCharacter>();
    }

    public virtual bool CanAttack()
    {
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


        owner.FaceTo(target.transform.position);

        isAttacking = true;
        pendingTarget = target.transform;
        owner.animator.SetTrigger(attackTrigger);

        return true;
    }

    public abstract void OnAttackCast();

    public virtual void OnAttackFinished()
    {
        isAttacking = false;
        pendingTarget = null;
        lastAttackTime = Time.time;
    }

}
