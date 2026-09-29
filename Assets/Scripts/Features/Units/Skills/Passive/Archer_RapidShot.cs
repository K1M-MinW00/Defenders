using UnityEngine;

public class Archer_RapidShot : PassiveSkillBase
{
    [Header("Precision Shot")]
    [SerializeField] private int requiredShots = 5;

    [SerializeField] private float buff_duration = 2f;
    [SerializeField] private float bonusPercent = 0.25f;
    [SerializeField] private float upgrade_bonusPercent = 0.5f;

    [SerializeField] private string buffId = "Archer_Passive";

    private int shotCount;

    protected override void ResetRuntimeState()
    {
        shotCount = 0;
    }

    public override void OnAttackStarted(ICombatTarget target)
    {
        if (!CanUsePassive())
            return;

        shotCount++;

        if (shotCount < requiredShots)
            return;

        shotCount = 0;

        ApplyArcherBuff();
    }

    private void ApplyArcherBuff()
    {
        float bonus = skillController.HasPassiveUpgrade2 ? upgrade_bonusPercent : bonusPercent;
        BuffApplication buff = new(
            buffId,
            StatType.AttackPerSec,
            BuffModifyType.Percent,
            bonus,
            BuffDurationType.Timed,
            buff_duration);

        owner.BuffController.ApplyOrRefreshBuff(buff, StatRefreshPolicy.KeepRatio);
    }
}
