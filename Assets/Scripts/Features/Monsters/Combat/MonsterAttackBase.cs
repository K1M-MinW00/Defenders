using UnityEngine;

public abstract class MonsterAttackBase : MonoBehaviour, IAnimationDrivenAttack
{
    [SerializeField] private bool useAnimationEvents;

    protected MonsterController owner;
    protected readonly AttackLifecycle attackLifecycle = new();

    public AttackPhase Phase => attackLifecycle.Phase;
    protected ICombatTarget CurrentTarget => attackLifecycle.Target;

    protected virtual void Awake()
    {
        if (owner == null)
            owner = GetComponent<MonsterController>();
    }

    public virtual bool CanAttack()
    {
        if (owner == null || owner.IsDead)
            return false;

        return owner.IsTargetInAttackRange();
    }

    public bool TryAttack(ICombatTarget target)
    {
        if (!IsValidTarget(target) || !CanAttack())
            return false;

        if (!attackLifecycle.TryBegin(target))
            return false;

        owner.PlayAttack();
        GameAudioManager.Instance?.PlayCharacterSfx(
            owner.Data?.attackSound,
            GameAudioCue.MonsterAttack,
            GameAudioPriority.Normal,
            0.08f);

        if (!useAnimationEvents)
        {
            OnAttackHit();
            OnAttackFinished();
        }

        return true;
    }

    public void OnAttackHit()
    {
        if (!attackLifecycle.TryEnterHitPhase())
            return;

        ICombatTarget target = CurrentTarget;
        if (!IsValidTarget(target))
        {
            CancelAttack();
            return;
        }

        ApplyHit(target);
    }

    public void OnAttackFinished()
    {
        attackLifecycle.Complete();
    }

    public void CancelAttack()
    {
        attackLifecycle.Cancel();
    }

    protected abstract void ApplyHit(ICombatTarget target);

    private static bool IsValidTarget(ICombatTarget target)
    {
        return CombatTargetSelector.IsValid(target);
    }
}
