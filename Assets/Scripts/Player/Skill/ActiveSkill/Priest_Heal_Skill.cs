using System.Collections.Generic;
using UnityEngine;

public class Priest_Heal_Skill : ActiveSkillBase
{
    [Header("Heal")]
    [SerializeField] private float healMultiplier = 2.0f;
    [SerializeField] private GameObject healEffectPrefab;
    private GameObject spawnedEffect;

    public override ActiveSkillTargetType TargetType => ActiveSkillTargetType.LowestHpAlliesInRangeOrGlobal;
    public override SkillTargetFailPolicy TargetFailPolicy => SkillTargetFailPolicy.CancelAndRefund;

    public override bool TryBuildContext(out SkillExecutionContext context)
    {
        context = new SkillExecutionContext();
        context.Initialize(owner);

        if (owner.UnitRoster == null)
            return false;

        UnitController target = owner.UnitRoster.GetLowestHpAliveUnit();
        if (target == null)
            return false;

        context.SetAllyTargets(new List<UnitController> { target });
        return true;
    }

    public override void OnSkillStart(SkillExecutionContext context)
    {
        if (context.AllyTargets == null || context.AllyTargets.Count == 0)
            return;

        UnitController target = context.AllyTargets[0];
        if (target == null)
            return;

        owner.Animation.FaceTo(owner.transform.position, target.transform.position);


        if (healEffectPrefab != null)
        {
            spawnedEffect = Instantiate(healEffectPrefab, target.transform.position, Quaternion.identity);
        }
    }

    public override void OnSkillApply(SkillExecutionContext context)
    {
        if (context.AllyTargets == null || context.AllyTargets.Count == 0)
            return;

        UnitController target = context.AllyTargets[0];
        if (target == null || target.IsDead)
            return;

        float healAmount = owner.Attack * healMultiplier;
        target.Health.Heal(healAmount);

    }

    public override void OnSkillEnd(SkillExecutionContext context)
    {
        if (spawnedEffect != null)
        {
            spawnedEffect.SetActive(false);
            Destroy(spawnedEffect);
            spawnedEffect = null;
        }
    }

    public override void CancelSkill()
    {
        if (spawnedEffect != null)
        {
            spawnedEffect.SetActive(false);
            Destroy(spawnedEffect);
            spawnedEffect = null;
        }
    }
}