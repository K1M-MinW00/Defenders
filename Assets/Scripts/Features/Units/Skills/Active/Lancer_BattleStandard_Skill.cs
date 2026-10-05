using UnityEngine;

public class Lancer_BattleStandard_Skill : ActiveSkillBase
{
    [Header("Flag")]
    [SerializeField] private Lancer_Active_Aura flagPrefab;

    [Header("Aura Buff")]
    [SerializeField] private float radius = 2f;
    [SerializeField] private float duration = 3f;
    [SerializeField] private float upgrade_duration = 5f;

    [SerializeField] private float attackBonusPercent = 0.2f;
    [SerializeField] private float attackSpeedBonusPercent = 0.15f;

    [SerializeField] private LayerMask allyLayer;

    public override ActiveSkillTargetType TargetType => ActiveSkillTargetType.SelfArea;
    public override SkillTargetFailPolicy TargetFailPolicy => SkillTargetFailPolicy.CastWithoutTarget;

    public override bool TryBuildContext(out SkillExecutionContext context)
    {
        return PrepareSelfAreaContext(out context, includeClosestEnemy: false);
    }

    public override void OnSkillStart(SkillExecutionContext context)
    {
        Telegraph.ShowCircle(
            owner.transform,
            Vector3.zero,
            radius,
            new Color(1f, 0.82f, 0.12f, 0.9f));
    }

    public override void OnSkillApply(SkillExecutionContext context)
    {
        Vector3 spawnPos = context.CastPosition;

        if (!TrySpawnSkillObject(
                flagPrefab,
                spawnPos,
                Quaternion.identity,
                PoolCategory.Effect,
                out Lancer_Active_Aura flag))
            return;

        string uniqueId = $"{owner.GetInstanceID()}_{Time.frameCount}";

        float buffTime = ResolveActiveUpgrade(duration, upgrade_duration);

        flag.Initialize(buffTime, radius, attackBonusPercent, attackSpeedBonusPercent, allyLayer, uniqueId);
    }

    public override void OnSkillEnd(SkillExecutionContext context) { }

    public override void CancelSkill() { }

    private void OnDrawGizmosSelected()
    {
        Vector3 center = owner != null ? owner.transform.position : transform.position;

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(center, radius);
    }
}
