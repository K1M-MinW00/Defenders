using UnityEngine;

public class Werebear_BloodThirst : PassiveSkillBase
{
    [Header("Blood Thirst")]
    [SerializeField] private float healRatio = 0.15f; // 입힌 피해의 15%
    [SerializeField] private float upgrade_healRatio = 0.3f;

    public override void OnAttackHit(ICombatTarget target, ref float damage)
    {
        if (!CanUsePassive())
            return;

        if (!CombatTargetSelector.IsValid(target))
            return;

        if (damage <= 0f)
            return;

        float ratio = skillController.HasPassiveUpgrade2 ? upgrade_healRatio : healRatio;
        float healAmount = damage * ratio;
        owner.Health.Heal(healAmount);
    }
}
