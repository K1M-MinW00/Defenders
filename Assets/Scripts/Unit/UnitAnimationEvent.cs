using UnityEngine;

public class UnitAnimationEvent : MonoBehaviour
{
    private IAttackBehavior attackBehavior;
    private MeleeAttackBehavior meleeAttackBehavior;
    private RangedAttackBehavior rangedAttacKBehavior;

    private void Awake()
    {
        attackBehavior = GetComponentInParent<IAttackBehavior>();
    }

    public void OnAttackHit()
    {
        if (attackBehavior != null)
        {
            meleeAttackBehavior = attackBehavior as MeleeAttackBehavior;
            meleeAttackBehavior?.OnAttackHit();
        }
    }

    public void OnAttackCast()
    {
        if (attackBehavior != null)
        {
            rangedAttacKBehavior = attackBehavior as RangedAttackBehavior;
            rangedAttacKBehavior?.OnAttackCast();
        }
    }

    public void OnAttackFinished()
    {
        if (meleeAttackBehavior != null)
            meleeAttackBehavior.OnAttackFinished();

        if (rangedAttacKBehavior != null)
            rangedAttacKBehavior.OnAttackFinished();
    }
}
