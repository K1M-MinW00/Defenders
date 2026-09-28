using UnityEngine;

public sealed class MonsterAnimationEvent : MonoBehaviour
{
    private IAnimationDrivenAttack attackBehavior;

    private void Awake()
    {
        attackBehavior = GetComponentInParent<IAnimationDrivenAttack>();
    }

    public void OnAttackHit()
    {
        attackBehavior?.OnAttackHit();
    }

    public void OnAttackFinished()
    {
        attackBehavior?.OnAttackFinished();
    }
}
