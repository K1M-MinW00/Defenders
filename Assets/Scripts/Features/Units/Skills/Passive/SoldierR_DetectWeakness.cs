using UnityEngine;

public class SoldierR_DetectWeakness : PassiveSkillBase
{
    [Header("Detect Weakness")]
    [SerializeField] private float procChance = 0.25f;
    [SerializeField] private float damageMultiplier = 1.2f;
    [SerializeField] private float upgrade_damageMultiplier = 1.5f;

    public override void OnAttackHit(ICombatTarget target, ref float damage)
    {
        if (!CanUsePassive())
            return;

        if (!CombatTargetSelector.IsValid(target))
            return;

        if (Random.value > procChance)
            return;

        float multiplier = skillController.HasPassiveUpgrade2 ? upgrade_damageMultiplier : damageMultiplier;
        float additiveDamage = damage * Mathf.Max(0f, multiplier - 1f);

        target.CombatHealth.ApplyDamage(
            new DamageRequest(additiveDamage, owner, DamageOrigin.Effect));
    }
}
