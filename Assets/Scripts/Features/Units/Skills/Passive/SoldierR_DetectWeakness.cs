using UnityEngine;

public class SoldierR_DetectWeakness : PassiveSkillBase
{
    [Header("Detect Weakness")]
    [SerializeField] private float procChance = 0.25f;
    [SerializeField] private float bonusDamagePercent = 0.4f;

    public override void OnAttackHit(MonsterController target, ref float damage)
    {
        if (owner == null || owner.IsDead)
            return;

        if (target == null || target.Health.IsDead)
            return;

        if (Random.value > procChance)
            return;

        float additiveDamage = damage * bonusDamagePercent;
        target.Health.TakeDamage(additiveDamage);
    }
}