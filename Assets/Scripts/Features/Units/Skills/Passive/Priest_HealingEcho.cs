using UnityEngine;

public class Priest_HealingEcho : PassiveSkillBase
{
    [Header("Healing Echo")]
    [SerializeField] private int requiredHits = 4;
    [SerializeField] private float healPercent = 0.05f; // 5%

    private int hitCount;

    protected override void ResetRuntimeState()
    {
        hitCount = 0;
    }

    public override void OnAttackHit(MonsterController target, ref float damage)
    {
        if (owner == null || owner.IsDead)
            return;

        hitCount++;

        if (hitCount < requiredHits)
            return;

        hitCount = 0;

        UnitController lowest = owner.UnitRoster.GetLowestHpAliveUnit();

        if (lowest == null)
            return;

        float healAmount = lowest.Health.MaxHp * healPercent;
        lowest.Health.Heal(healAmount);
    }
}