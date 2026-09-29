using UnityEngine;

public abstract class ActiveSkillBase : MonoBehaviour
{
    private readonly SkillExecutionContext reusableContext = new();

    protected UnitController owner;
    protected UnitSkillController skillController;
    protected int Promotion => owner.UserUnit.Promotion;
    public abstract ActiveSkillTargetType TargetType { get; }
    public virtual SkillTargetFailPolicy TargetFailPolicy => SkillTargetFailPolicy.CancelAndRefund;
    public virtual TargetResolutionPolicy ResolutionPolicy =>
        TargetType == ActiveSkillTargetType.SelfArea
            ? TargetResolutionPolicy.Self
            : TargetResolutionPolicy.RetargetOnResolve;
    public virtual int TargetCount => 1;

    public virtual void Initialize(UnitController owner, UnitSkillController skillController)
    {
        this.owner = owner;
        this.skillController = skillController;
    }

    public abstract bool TryBuildContext(out SkillExecutionContext context);

    protected SkillExecutionContext PrepareReusableContext()
    {
        reusableContext.Initialize(owner);
        return reusableContext;
    }

    public virtual bool CanApply(SkillExecutionContext context)
    {
        return SkillTargetPolicy.CanApply(TargetType, ResolutionPolicy, context);
    }

    public virtual bool TryResolveContext(SkillExecutionContext context)
    {
        return SkillTargetPolicy.TryResolve(
            TargetType,
            ResolutionPolicy,
            context,
            owner);
    }

    public virtual void OnSkillStart(SkillExecutionContext context) { }
    public abstract void OnSkillApply(SkillExecutionContext context);
    public virtual void OnSkillEnd(SkillExecutionContext context) { }
    public virtual void CancelSkill() { }
}
