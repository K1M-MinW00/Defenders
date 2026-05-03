using System;
using System.Collections.Generic;
using UnityEngine;

public class Priest_Heal_Skill : ActiveSkillBase
{
    [Header("Heal")]
    [SerializeField] private float healMultiplier = 2.0f;
    [SerializeField] private GameObject healEffectPrefab;
    private Poolable spawnedEffect;

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
    }

    public override void OnSkillApply(SkillExecutionContext context)
    {
        float healAmount = owner.Attack * healMultiplier;
        
        foreach (var target in context.AllyTargets)
        {
            target.Health.Heal(healAmount);
            SpawnHealEffect(target);
        }
    }

    public override void OnSkillEnd(SkillExecutionContext context)
    {
        ReturnHealEffect();
    }

    public override void CancelSkill()
    {
        ReturnHealEffect();
    }

    private void SpawnHealEffect(UnitController target)
    {
        spawnedEffect = owner.PoolManager.Spawn(healEffectPrefab, target.transform.position, Quaternion.identity, PoolCategory.Effect, target.transform);

        if (spawnedEffect != null && spawnedEffect.TryGetComponent(out PooledVfx vfx))
            vfx.Play();
    }

    private void ReturnHealEffect()
    {
        if (spawnedEffect == null)
            return;

        spawnedEffect.ReturnToPool();
        spawnedEffect = null;
    }

}