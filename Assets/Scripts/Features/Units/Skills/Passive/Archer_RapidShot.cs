using UnityEngine;

public class Archer_RapidShot : PassiveSkillBase
{
    [Header("Precision Shot")]
    [SerializeField] private int requiredShots = 4;
    [SerializeField] private float duration = 2f;
    [SerializeField] private float attackSpeedBonusPercent = 0.25f;

    [SerializeField] private string buffId = "Archer_Passive";

    private int shotCount;

    protected override void ResetRuntimeState()
    {
        shotCount = 0;
    }

    public override void OnAttackStarted(MonsterController target)
    {
        if (owner == null || owner.IsDead)
            return;

        shotCount++;

        if (shotCount < requiredShots)
            return;

        shotCount = 0;

        ApplyArcherBuff();
    }

    private void ApplyArcherBuff()
    {
        RuntimeBuff buff = new RuntimeBuff(
               buffId, BuffStatType.AttackPerSec, BuffModifyType.Multiplicative, attackSpeedBonusPercent, BuffDurationType.Timed, duration);

        owner.BuffController.RemoveBuff(buffId, StatRefreshPolicy.KeepRatio);
        owner.BuffController.AddBuff(buff, StatRefreshPolicy.KeepRatio);
    }
}