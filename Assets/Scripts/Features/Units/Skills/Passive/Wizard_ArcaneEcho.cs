using UnityEngine;

public class Wizard_ArcaneEcho : PassiveSkillBase
{
    [Header("Arcane Echo")]
    [SerializeField] private float procChance = 0.25f;
    [SerializeField] private float refundEnergy = 25f;
    [SerializeField] private float upgrade_refundEnergy = 50f;

    protected override void ResetRuntimeState() { }

    public override void OnActiveSkillEnded()
    {
        if (!CanUsePassive())
            return;

        if (Random.value > procChance)
            return;

        float refund = skillController.HasPassiveUpgrade2 ? upgrade_refundEnergy : refundEnergy;
        owner.Energy.Add(refund);
    }
}