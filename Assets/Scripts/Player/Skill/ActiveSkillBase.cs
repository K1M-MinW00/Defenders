using UnityEngine;

public abstract class ActiveSkillBase : MonoBehaviour
{
    protected UnitController owner;
    protected UnitSkillController skillController;

    public abstract ActiveSkillTargetType TargetType { get; }
    public virtual SkillTargetFailPolicy TargetFailPolicy => SkillTargetFailPolicy.CancelAndRefund;
    public virtual int TargetCount => 1;

    public virtual void Initialize(UnitController owner, UnitSkillController skillController)
    {
        this.owner = owner;
        this.skillController = skillController;
    }

    public abstract bool TryBuildContext(out SkillExecutionContext context);

    public virtual void OnSkillStart(SkillExecutionContext context) { }
    public abstract void OnSkillApply(SkillExecutionContext context);
    public virtual void OnSkillEnd(SkillExecutionContext context) { }
    public virtual void CancelSkill() { }
}