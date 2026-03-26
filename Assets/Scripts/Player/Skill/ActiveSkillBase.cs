using UnityEditor.MPE;
using UnityEngine;

public abstract class ActiveSkillBase : MonoBehaviour, IActiveSkill
{
    protected UnitController owner;
    protected ActiveSkillDataSO data;
    protected int promotionLevel;

    protected bool isUsingSkill;
    public bool IsUsingSkill => isUsingSkill;

    public virtual void Initialize(UnitController owner, ActiveSkillDataSO data, int promotionLevel)
    {
        this.owner = owner;
        this.data = data;
        this.promotionLevel = promotionLevel;
    }

    public virtual bool CanUseSkill()
    {
        if (owner == null || owner.IsDead)
            return false;

        if (isUsingSkill)
            return false;

        return true;
    }

    public virtual void BeginSkill()
    {
        isUsingSkill = true;
        owner.PlaySkill();
    }


    public virtual void CancelSkill()
    {
        if (!isUsingSkill)
            return;

        isUsingSkill = false;
    }

    public virtual void EndSkill()
    {
        isUsingSkill = false;
    }

    public abstract void OnSkillHit();

}