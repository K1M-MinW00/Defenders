using UnityEngine;

public class Swordsman_BladeDance : PassiveSkillBase
{
    [Header("Blade Dance")]
    [SerializeField] private int maxStacks = 5;
    [SerializeField] private int upgrade_maxStacks = 4;

    [SerializeField] private float attackSpeedBonusPerStack = 0.06f;
    [SerializeField] private float stackDuration = 2f;

    [SerializeField] private string buffId = "Swordsman_BladeDance";

    private int currentStacks;
    private float lastAttackTime;

    protected override void ResetRuntimeState()
    {
        currentStacks = 0;
        lastAttackTime = -999f;
    }

    public override void OnAttackStarted(ICombatTarget target)
    {
        if (!CanUsePassive())
            return;

        if (Time.time > lastAttackTime + stackDuration)
            currentStacks = 0;

        lastAttackTime = Time.time;
        int effectiveMaxStacks = skillController.HasPassiveUpgrade2
            ? upgrade_maxStacks
            : maxStacks;

        currentStacks = Mathf.Min(currentStacks + 1, effectiveMaxStacks);
        float maximumBonus = maxStacks * attackSpeedBonusPerStack;
        float totalBonus = StackingBonusCalculator.Calculate(
            currentStacks,
            effectiveMaxStacks,
            maximumBonus);

        BuffApplication buff = new(
            buffId: buffId,
            statType: StatType.AttackPerSec,
            modifyType: BuffModifyType.Percent,
            value: totalBonus,
            durationType: BuffDurationType.Timed,
            durationSeconds: stackDuration
        );

        owner.BuffController.ApplyOrRefreshBuff(buff, StatRefreshPolicy.KeepRatio);
    }
}
