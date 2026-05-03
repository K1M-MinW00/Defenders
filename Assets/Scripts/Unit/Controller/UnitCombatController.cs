using System;
using UnityEngine;

public class UnitCombatController : MonoBehaviour
{
    [SerializeField] private float targetRefreshInterval = 0.2f;

    private UnitController owner;
    private IUnitAttack attackBehavior;

    public float TargetRefreshInterval => targetRefreshInterval;

    public void Initialize(UnitController owner)
    {
        this.owner = owner;
        attackBehavior = GetComponent<IUnitAttack>();
    }

    public void TryAttackCurrentTarget()
    {
        var target = owner.Targeting.CurrentTarget;

        if (target == null || target.Health.IsDead)
            return;

        owner.Animation.FaceTarget(target);
        attackBehavior?.TryAttack(target);
    }

    public void CancelAttack()
    {
        attackBehavior?.CancelAttack();
    }
}