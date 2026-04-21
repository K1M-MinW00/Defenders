using UnityEngine;

public class SoldierM_Grit : PassiveSkillBase
{
    [Header("Grit")]
    [SerializeField] private float procChance = 0.25f;
    [SerializeField] private float damageReductionPercent = 0.40f;

    protected override void ResetRuntimeState() { }

    public override void OnBeforeTakeDamage(ref float damage)
    {
        if (owner == null || owner.IsDead)
            return;

        if (damage <= 0f)
            return;

        if (Random.value > procChance)
            return;

        damage *= (1f - damageReductionPercent);
    }
}