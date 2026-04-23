using UnityEngine;

public class Wizard_ArcaneEcho : PassiveSkillBase
{
    [Header("Arcane Echo")]
    [SerializeField] private float procChance = 0.25f;
    [SerializeField] private float refundEnergy = 50f;

    protected override void ResetRuntimeState()
    {
    }

    public override void OnActiveSkillEnded()
    {
        if (owner == null || owner.IsDead)
            return;

        if (Random.value > procChance)
            return;

        owner.Energy.Add(refundEnergy);

    }
}