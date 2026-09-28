using UnityEngine;

public class KnightTemplar_KnockbackStrike : PassiveSkillBase
{
    [Header("Knockback Strike")]
    [SerializeField] private float knockbackDistance = 0.6f;
    [SerializeField] private float upgrade_knockbackDistance = 1f;

    [SerializeField] private float knockbackDuration = 0.12f;
    [SerializeField] private float cooldown = 0.5f;

    private float lastProcTime;

    protected override void ResetRuntimeState()
    {
        lastProcTime = -999f;
    }

    public override void OnAttackHit(ICombatTarget target, ref float damage)
    {
        if (!CanUsePassive())
            return;

        if (!CombatTargetSelector.IsValid(target) || target is not IKnockbackReceiver receiver)
            return;

        if (Time.time < lastProcTime + cooldown)
            return;

        lastProcTime = Time.time;

        Vector2 dir = ((Vector2)target.TargetTransform.position - (Vector2)owner.transform.position).normalized;
        float distance = skillController.HasPassiveUpgrade2 ? upgrade_knockbackDistance : knockbackDistance;

        receiver.ApplyKnockback(dir, distance, knockbackDuration);
    }
}
