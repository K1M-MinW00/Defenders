using UnityEngine;

public abstract class ActiveSkillBase : MonoBehaviour, IActiveSkill
{
    protected UnitController owner;
    protected ActiveSkillDataSO data;

    protected bool ActiveTier2Unlocked;
    protected bool ActiveTier4Unlocked;


    public virtual void Initialize(UnitController owner, ActiveSkillDataSO data)
    {
        this.owner = owner;
        this.data = data;

        ActiveTier2Unlocked = owner.ActiveTier2Unlocked;
        ActiveTier4Unlocked = owner.ActiveTier4Unlocked;
    }

    public virtual bool CanUseSkill()
    {
        if (owner == null || data == null || owner.IsDead)
            return false;

        return true;
    }

    public virtual void BeginSkill() { }
    public virtual void CancelSkill() { }

    public virtual void EndSkill()
    {
        owner.ConsumeAllEnergy();
    }

    public abstract void OnSkillHit();

}