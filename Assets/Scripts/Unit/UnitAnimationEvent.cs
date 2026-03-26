using UnityEngine;

public class UnitAnimationEvent : MonoBehaviour
{
    private IAttackBehavior attackBehavior;
    private UnitController unitController;

    private void Awake()
    {
        attackBehavior = GetComponentInParent<IAttackBehavior>();
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
    public void OnSkillHit()
    {
        unitController?.OnActiveSkillHit();
    }

    public void OnSkillFinished()
    {
        unitController?.OnEndActiveSkill();
    }
}
