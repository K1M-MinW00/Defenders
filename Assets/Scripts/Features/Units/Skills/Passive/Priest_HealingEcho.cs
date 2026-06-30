using UnityEngine;

public class Priest_HealingEcho : PassiveSkillBase
{
    [Header("Healing Echo")]
    [SerializeField] private int requiredHits = 5;
    [SerializeField] private int upgrade_requiredHits = 4;
    [SerializeField] private float healPercent = 0.1f; // 10%

    private int hitCount;

    protected override void ResetRuntimeState()
    {
        hitCount = 0;
    }

    public override void OnAttackHit(MonsterController target, ref float damage)
    {
        if (!CanUsePassive())
            return;

        hitCount++;

        int required = skillController.HasPassiveUpgrade2 ? upgrade_requiredHits : requiredHits;

        if (hitCount < required)
            return;

        hitCount = 0;

        UnitController lowest = owner.UnitRoster.GetLowestHpAliveUnit();

        if (lowest == null)
            return;

        float healAmount = lowest.Health.MaxHp * healPercent;
        lowest.Health.Heal(healAmount);
    }
}