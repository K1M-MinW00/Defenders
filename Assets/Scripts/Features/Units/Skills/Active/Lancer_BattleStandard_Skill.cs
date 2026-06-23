using System.Collections.Generic;
using UnityEngine;

public class Lancer_BattleStandard_Skill : ActiveSkillBase
{
    [Header("Flag")]
    [SerializeField] private Lancer_Active_Aura flagPrefab;

    [Header("Aura Buff")]
    [SerializeField] private float radius = 2f;
    [SerializeField] private float duration = 5f;

    [SerializeField] private float attackBonusPercent = 0.2f;
    [SerializeField] private float attackSpeedBonusPercent = 0.15f;

    [SerializeField] private LayerMask allyLayer;

    public override ActiveSkillTargetType TargetType => ActiveSkillTargetType.SelfArea;
    public override SkillTargetFailPolicy TargetFailPolicy => SkillTargetFailPolicy.CastWithoutTarget;

    public override bool TryBuildContext(out SkillExecutionContext context)
    {
        context = new SkillExecutionContext();
        context.Initialize(owner);
        context.SetCastPosition(owner.transform.position);
        return true;
    }

    public override void OnSkillStart(SkillExecutionContext context) { }

    public override void OnSkillApply(SkillExecutionContext context)
    {
        if (flagPrefab == null)
            return;

        Vector3 spawnPos = context.CastPosition;

        Lancer_Active_Aura flag = owner.PoolManager.Spawn(flagPrefab, spawnPos, Quaternion.identity,PoolCategory.Effect);

        string uniqueId = $"{owner.GetInstanceID()}_{Time.frameCount}";

        flag.Initialize(duration,radius,attackBonusPercent,attackSpeedBonusPercent,allyLayer,uniqueId);
    }

    public override void OnSkillEnd(SkillExecutionContext context){ }

    public override void CancelSkill() { }

    private void OnDrawGizmosSelected()
    {
        Vector3 center = owner != null ? owner.transform.position : transform.position;

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(center, radius);
    }
}