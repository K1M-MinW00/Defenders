using UnityEngine;

public abstract class RangedUnitAttack : MonoBehaviour, IUnitAttack
{
    protected UnitController owner;
    protected MonsterController currentTarget => attackLifecycle.Target as MonsterController;
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
        if (target is not MonsterController monster || monster.IsDead)
            return false;

        if (!CanAttack())
            return false;

        if (!attackLifecycle.TryBegin(monster))
            return false;

        attackCooldown.Start(Time.time, Cooldown);

        owner.SkillController.NotifyAttackStarted(monster);
        
        owner.FaceTarget();
        owner.Animation.PlayAttack();
        GameAudioManager.Instance?.PlayCharacterSfx(owner.UnitData?.attackSound, GameAudioCue.UnitAttack, GameAudioPriority.Normal, 0.08f);

        return true;
    }

    public abstract void OnAttackHit();

    protected bool TryEnterHitPhase() => attackLifecycle.TryEnterHitPhase();

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
