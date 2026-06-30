using UnityEngine;

public class SoldierM_Grit : PassiveSkillBase
{
    [Header("Grit")]
    [SerializeField] private float procChance = 0.25f;
    [SerializeField] private float damageReductionPercent = 0.4f;
    [SerializeField] private float upgrade_damageReductionPercent = 0.6f;

    protected override void ResetRuntimeState() { }

    public override void OnBeforeTakeDamage(ref float damage)
    {
        if (!CanUsePassive())
            return;

        if (damage <= 0f)
            return;

        if (Random.value > procChance)
            return;

        float damageReduction = skillController.HasPassiveUpgrade2 ? upgrade_damageReductionPercent : damageReductionPercent;

        damage *= (1f - damageReduction);
    }
}