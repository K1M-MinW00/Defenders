using UnityEngine;

public abstract class PassiveSkillBase : MonoBehaviour, IPassiveSkill
{
    protected UnitController owner;
    protected UnitSkillController skillController;

    public UnitController Owner => owner;
    public bool IsInitialized => owner != null;

    public virtual void Initialize(UnitController owner, UnitSkillController skillController)
    {
        this.owner = owner;
        this.skillController = skillController;
        ResetRuntimeState();
    }


    protected virtual void ResetRuntimeState() { }

    protected bool CanUsePassive()
    {
        return owner != null && !owner.IsDead;
    }

    public virtual void OnBattleStart()
    {
        ResetRuntimeState();
    }

    public virtual void OnBattleEnd()
    {
        ResetRuntimeState();
    }

    public virtual void OnAttackStarted(MonsterController target) { }

    public virtual void OnAttackHit(MonsterController target, ref float damage) { }

    public virtual void OnBeforeTakeDamage(ref float damage) { }

    public virtual void OnAfterTakeDamage(float finalDamage) { }

    public virtual void OnActiveSkillStarted() { }

    public virtual void OnActiveSkillApplied() { }

    public virtual void OnActiveSkillEnded() { }
}