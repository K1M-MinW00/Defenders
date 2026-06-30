using UnityEngine;

public class SoldierR_DetectWeakness : PassiveSkillBase
{
    [Header("Detect Weakness")]
    [SerializeField] private float procChance = 0.25f;
    [SerializeField] private float damageMultiplier = 1.2f;
    [SerializeField] private float upgrade_damageMultiplier = 1.5f;

    public override void OnAttackHit(MonsterController target, ref float damage)
    {
        if (!CanUsePassive())
            return;

        if (target == null || target.Health.IsDead)
            return;

        if (Random.value > procChance)
            return;

        float multiplier = skillController.HasPassiveUpgrade2 ? upgrade_damageMultiplier : damageMultiplier;
        float additiveDamage = damage * multiplier;

        target.Health.TakeDamage(additiveDamage);
    }
}