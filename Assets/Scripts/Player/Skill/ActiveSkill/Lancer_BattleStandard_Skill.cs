using System.Collections.Generic;
using UnityEngine;

public class Lancer_BattleStandard_Skill : ActiveSkillBase
{
    [Header("Aura Buff")]
    [SerializeField] private float radius = 2.5f;
    [SerializeField] private float duration = 5f;

    [SerializeField] private float attackBonusPercent = 0.20f;
    [SerializeField] private float attackSpeedBonusPercent = 0.15f;

    [SerializeField] private LayerMask allyLayer;

    [Header("Buff Id")]
    [SerializeField] private string attackBuffId = "Lancer_BattleStandard_Attack";
    [SerializeField] private string attackSpeedBuffId = "Lancer_BattleStandard_AttackSpeed";

    [Header("Effect")]
    [SerializeField] private GameObject auraEffectPrefab;
    private GameObject spawnedEffect;
    public override ActiveSkillTargetType TargetType => ActiveSkillTargetType.SelfArea;
    public override SkillTargetFailPolicy TargetFailPolicy => SkillTargetFailPolicy.CastWithoutTarget;

    public override bool TryBuildContext(out SkillExecutionContext context)
    {
        context = new SkillExecutionContext();
        context.Initialize(owner);

        Vector2 center = owner.transform.position;
        context.SetCastPosition(center);

        Collider2D[] hits = Physics2D.OverlapCircleAll(center, radius, allyLayer);
        if (hits == null || hits.Length == 0)
            return true;

        HashSet<UnitController> affectedUnits = new();

        foreach (Collider2D hit in hits)
        {
            if (!hit.TryGetComponent<UnitController>(out UnitController unit))
                continue;

            if (unit == null || unit.IsDead)
                continue;

            if (!affectedUnits.Add(unit))
                continue;

            context.AddAllyTarget(unit);
        }

        return true;
    }

    public override void OnSkillStart(SkillExecutionContext context)
    {
        if (auraEffectPrefab != null)
            spawnedEffect = Instantiate(auraEffectPrefab, context.CastPosition, Quaternion.identity);
    }

    public override void OnSkillApply(SkillExecutionContext context)
    {
        var targets = context.AllyTargets;

        if (targets == null || targets.Count == 0)
            return;

        foreach(UnitController unit in targets)
            ApplyBuffToUnit(unit);
    }

    private void ApplyBuffToUnit(UnitController targetUnit)
    {
        RuntimeBuff attackBuff = new RuntimeBuff(
            buffId: attackBuffId,
            statType: BuffStatType.Attack,
            modifyType: BuffModifyType.Multiplicative,
            value: attackBonusPercent,
            durationType: BuffDurationType.Timed,
            durationSeconds: duration
        );

        RuntimeBuff attackSpeedBuff = new RuntimeBuff(
            buffId: attackSpeedBuffId,
            statType: BuffStatType.AttackPerSec,
            modifyType: BuffModifyType.Multiplicative,
            value: attackSpeedBonusPercent,
            durationType: BuffDurationType.Timed,
            durationSeconds: duration
        );

        targetUnit.BuffController.RemoveBuff(attackBuffId, StatRefreshPolicy.KeepRatio);
        targetUnit.BuffController.RemoveBuff(attackSpeedBuffId, StatRefreshPolicy.KeepRatio);

        targetUnit.BuffController.AddBuff(attackBuff, StatRefreshPolicy.KeepRatio);
        targetUnit.BuffController.AddBuff(attackSpeedBuff, StatRefreshPolicy.KeepRatio);
    }

    public override void OnSkillEnd(SkillExecutionContext context)
    {
        if (spawnedEffect != null)
        {
            Destroy(spawnedEffect);
            spawnedEffect = null;
        }
    }

    public override void CancelSkill()
    {
        if (spawnedEffect != null)
        {
            Destroy(spawnedEffect);
            spawnedEffect = null;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (owner == null)
            owner = GetComponent<UnitController>();

        Vector3 center = owner != null ? owner.transform.position : transform.position;

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(center, radius);
    }
}