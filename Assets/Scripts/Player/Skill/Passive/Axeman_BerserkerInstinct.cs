using UnityEngine;

public class Axeman_BerserkerInstinct : PassiveSkillBase
{
    [Header("Berserk Instinct")]
    [SerializeField] private float attackBonusPercent = 0.20f;
    [SerializeField] private float attackSpeedBonusPercent = 0.20f;
    [SerializeField] private float duration = 3f;
    [SerializeField] private float cooldown = 4f;

    [Header("Buff Id")]
    [SerializeField] private string attackBuffId = "Axeman_BerserkInstinct_Attack";
    [SerializeField] private string attackSpeedBuffId = "Axeman_BerserkInstinct_AttackSpeed";

    private float lastProcTime;

    protected override void ResetRuntimeState()
    {
        lastProcTime = -999f;
    }

    public override void OnAfterTakeDamage(float finalDamage)
    {
        if (owner == null || owner.IsDead)
            return;

        if (finalDamage <= 0f)
            return;

        if (Time.time < lastProcTime + cooldown)
            return;

        lastProcTime = Time.time;

        ApplyBerserkBuff();
    }

    private void ApplyBerserkBuff()
    {
        RuntimeBuff attackBuff = new RuntimeBuff(
            buffId: attackBuffId,
            statType: BuffStatType.Attack,
            modifyType: BuffModifyType.Multiplicative,
            value: attackBonusPercent,
            durationType: BuffDurationType.Timed,
            durationSeconds: duration
        );

        RuntimeBuff attackSpeedBuff = new RuntimeBuff(
            buffId: attackSpeedBuffId,
            statType: BuffStatType.AttackPerSec,
            modifyType: BuffModifyType.Multiplicative,
            value: attackSpeedBonusPercent,
            durationType: BuffDurationType.Timed,
            durationSeconds: duration
        );

        owner.BuffController.RemoveBuff(attackBuffId, StatRefreshPolicy.KeepRatio);
        owner.BuffController.RemoveBuff(attackSpeedBuffId, StatRefreshPolicy.KeepRatio);

        owner.BuffController.AddBuff(attackBuff, StatRefreshPolicy.KeepRatio);
        owner.BuffController.AddBuff(attackSpeedBuff, StatRefreshPolicy.KeepRatio);
    }
}