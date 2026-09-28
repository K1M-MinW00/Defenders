using UnityEngine;

public abstract class RangedUnitAttack : MonoBehaviour, IUnitAttack
{
    protected UnitController owner;
    protected ICombatTarget currentTarget => attackLifecycle.Target;
    protected readonly AttackCooldown attackCooldown = new();
    protected readonly AttackLifecycle attackLifecycle = new();
    protected bool isAttacking => attackLifecycle.IsActive;

    [SerializeField] protected LayerMask targetLayer;

    protected float Damage => owner.Attack;
    protected float Cooldown => 1f / owner.AttackPerSec;
    public bool IsAttacking => isAttacking;
    public AttackPhase Phase => attackLifecycle.Phase;

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
        if (!CombatTargetSelector.IsValid(target))
            return false;

        if (!CanAttack())
            return false;

        if (!attackLifecycle.TryBegin(target))
            return false;

        attackCooldown.Start(Time.time, Cooldown);

        owner.SkillController.NotifyAttackStarted(target);
        
        owner.FaceTarget();
        owner.Animation.PlayAttack();
        GameAudioManager.Instance?.PlayCharacterSfx(owner.UnitData?.attackSound, GameAudioCue.UnitAttack, GameAudioPriority.Normal, 0.08f);

        return true;
    }

    public abstract void OnAttackHit();

    protected bool TryEnterHitPhase()
    {
        if (!CombatTargetSelector.IsValid(currentTarget))
        {
            if (!owner.Targeting.TryFindTargetInSensor())
            {
                CancelAttack();
                return false;
            }

            ICombatTarget replacement = owner.Targeting.CurrentTarget;
            if (!attackLifecycle.TryReplaceTarget(replacement))
                return false;

            owner.Animation.FaceTarget(replacement);
        }

        return attackLifecycle.TryEnterHitPhase();
    }

    public virtual void OnAttackFinished()
    {
        if (!attackLifecycle.Complete())
            return;

    }

    public void CancelAttack()
    {
        attackLifecycle.Cancel();
    }
}
