using UnityEngine;

public class Swordsman_BladeDance : PassiveSkillBase
{
    [Header("Blade Dance")]
    [SerializeField] private int maxStacks = 5;
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

    public override void OnAttackStarted(MonsterController target)
    {
        if (owner == null || owner.IsDead)
            return;

        if (Time.time > lastAttackTime + stackDuration)
            currentStacks = 0;

        lastAttackTime = Time.time;
        currentStacks = Mathf.Min(currentStacks + 1, maxStacks);

        float totalBonus = currentStacks * attackSpeedBonusPerStack;

        RuntimeBuff buff = new RuntimeBuff(
            buffId: buffId,
            statType: BuffStatType.AttackPerSec,
            modifyType: BuffModifyType.Multiplicative,
            value: totalBonus,
            durationType: BuffDurationType.Timed,
            durationSeconds: stackDuration
        );

        owner.BuffController.RemoveBuff(buffId, StatRefreshPolicy.KeepRatio);
        owner.BuffController.AddBuff(buff, StatRefreshPolicy.KeepRatio);
    }
}