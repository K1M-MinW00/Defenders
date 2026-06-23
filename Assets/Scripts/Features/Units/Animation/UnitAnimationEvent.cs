using UnityEngine;

public class UnitAnimationEvent : MonoBehaviour
{
    private IUnitAttack attackBehavior;
    private UnitController unitController;

    private void Awake()
    {
        attackBehavior = GetComponentInParent<IUnitAttack>();
        unitController = GetComponentInParent<UnitController>();
    }

    public void OnAttackHit()
    {
        attackBehavior?.OnAttackHit();
    }

    public void OnAttackFinished()
    {
        attackBehavior?.OnAttackFinished();
    }

    public void OnSkillApplyEvent()
    {
        unitController.SkillController.ApplySkill();
    }

    public void OnSkillFinishedEvent()
    {
        unitController.SkillController.EndSkill();
    }
}
