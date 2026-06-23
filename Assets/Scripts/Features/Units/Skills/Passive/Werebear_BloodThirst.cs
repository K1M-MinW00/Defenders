using UnityEngine;

public class Werebear_BloodThirst : PassiveSkillBase
{
    [Header("Blood Thirst")]
    [SerializeField] private float healRatio = 0.15f; // 입힌 피해의 15%

    public override void OnAttackHit(MonsterController target, ref float damage)
    {
        if (owner == null || owner.IsDead)
            return;

        if (target == null || target.Health.IsDead)
            return;

        if (damage <= 0f)
            return;

        float healAmount = damage * healRatio;
        owner.Health.Heal(healAmount);
    }
}