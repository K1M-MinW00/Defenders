using UnityEngine;

public abstract class RangedUnitAttack : MonoBehaviour, IUnitAttack
{
    protected UnitController owner;
    protected MonsterController currentTarget;
    protected readonly AttackCooldown attackCooldown = new();
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

        if (!attackCooldown.IsReady(Time.time))
            return false;

        return true;
    }

    public virtual bool TryAttack(ICombatTarget target)
    {
        if (target is not MonsterController monster || monster.IsDead)
            return false;

        if (!CanAttack())
            return false;

        currentTarget = monster;

        isAttacking = true;

        owner.SkillController.NotifyAttackStarted(monster);
        
        owner.FaceTarget();
        owner.Animation.PlayAttack();
        GameAudioManager.Instance?.PlayCharacterSfx(owner.UnitData?.attackSound, GameAudioCue.UnitAttack, GameAudioPriority.Normal, 0.08f);

        return true;
    }

    public abstract void OnAttackHit();

    public virtual void OnAttackFinished()
    {
        attackCooldown.Start(Time.time, Cooldown);
        isAttacking = false;
        currentTarget = null;
    }

    public void CancelAttack()
    {
        if (!isAttacking)
            return;

        isAttacking = false;
        currentTarget = null;
    }
}
