using System;
using UnityEngine;

public class UnitCombatController : MonoBehaviour
{
    private const float MinimumAttackRecoveryTimeout = 0.5f;

    [SerializeField] private float targetRefreshInterval = 0.2f;

    private UnitController owner;
    private IUnitAttack attackBehavior;
    private float attackStartedAt = float.NegativeInfinity;

    public float TargetRefreshInterval => targetRefreshInterval;

    public void Initialize(UnitController owner)
    {
        this.owner = owner;
        attackBehavior = GetComponent<IUnitAttack>();
    }

    public void TryAttackCurrentTarget()
    {
        RecoverInterruptedAttack();

        var target = owner.Targeting.CurrentTarget;

        if (!CombatTargetSelector.IsValid(target))
            return;

        owner.Animation.FaceTarget(target);
        if (attackBehavior?.TryAttack(target) == true)
            attackStartedAt = Time.time;
    }

    public void CancelAttack()
    {
        attackBehavior?.CancelAttack();
        attackStartedAt = float.NegativeInfinity;
    }

    private void RecoverInterruptedAttack()
    {
        if (attackBehavior == null || !attackBehavior.IsAttacking)
            return;

        float attacksPerSecond = Mathf.Max(0.01f, owner.AttackPerSec);
        float timeout = Mathf.Max(MinimumAttackRecoveryTimeout, 2f / attacksPerSecond);
        if (Time.time < attackStartedAt + timeout)
            return;

        CancelAttack();
    }
}
