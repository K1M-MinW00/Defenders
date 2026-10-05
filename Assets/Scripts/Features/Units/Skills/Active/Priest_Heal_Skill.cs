using System;
using UnityEngine;

public class Priest_Heal_Skill : ActiveSkillBase
{
    [Header("Heal")]
    [SerializeField] private float healMultiplier = 2.0f;
    [SerializeField] private float upgrade_healMultiplier = 3f;

    [SerializeField] private GameObject healEffectPrefab;

    public override ActiveSkillTargetType TargetType => ActiveSkillTargetType.LowestHpAlliesInRangeOrGlobal;
    public override SkillTargetFailPolicy TargetFailPolicy => SkillTargetFailPolicy.CancelAndRefund;

    public override bool TryResolveContext(SkillExecutionContext context)
    {
        if (context == null || owner.UnitRoster == null)
            return false;

        UnitController target = owner.UnitRoster.GetLowestHpAliveUnit();
        if (target == null)
            return false;

        context.SetAllyTarget(target);
        owner.Animation.FaceTo(owner.transform.position, target.transform.position);
        return true;
    }

    public override bool TryBuildContext(out SkillExecutionContext context)
    {
        context = PrepareReusableContext();

        if (owner.UnitRoster == null)
            return false;

        UnitController target = owner.UnitRoster.GetLowestHpAliveUnit();

        if (target == null)
            return false;

        context.SetAllyTarget(target);
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
        Telegraph.ShowCircle(
            target.transform,
            Vector3.zero,
            0.45f,
            new Color(0.2f, 1f, 0.4f, 0.9f));
    }

    public override void OnSkillApply(SkillExecutionContext context)
    {
        float multiplier = ResolveActiveUpgrade(healMultiplier, upgrade_healMultiplier);

        float healAmount = owner.Attack * multiplier;
        
        foreach (var target in context.AllyTargets)
        {
            target.Health.Heal(healAmount);
            SpawnHealEffect(target);
        }
    }

    public override void OnSkillEnd(SkillExecutionContext context)
    {
    }

    public override void CancelSkill()
    {
    }

    private void SpawnHealEffect(UnitController target)
    {
        if (TrySpawnSkillObject(
                healEffectPrefab,
                target.transform.position,
                Quaternion.identity,
                PoolCategory.Effect,
                out Poolable spawnedEffect,
                target.transform) &&
            spawnedEffect.TryGetComponent(out PooledVfx vfx))
        {
            TrackExecutionEffect(spawnedEffect);
            vfx.Play();
        }
    }

}
