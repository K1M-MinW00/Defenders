using System.Collections.Generic;
using UnityEngine;

public sealed class PlagueAlchemist_ToxicAccumulation : PassiveSkillBase
{
    [Header("Toxic Accumulation")]
    [SerializeField, Min(1)] private int maximumStacks = 3;
    [SerializeField, Min(1)] private int upgradedMaximumStacks = 4;
    [SerializeField, Min(0f)] private float firstStackDamageRatio = 0.1f;
    [SerializeField, Min(0f)] private float additionalStackDamageRatio = 0.05f;
    [SerializeField, Min(0.05f)] private float tickInterval = 1f;
    [SerializeField, Min(0.05f)] private float duration = 4f;

    private readonly Dictionary<MonsterController, int> stacksByTarget = new();

    protected override void ResetRuntimeState() => stacksByTarget.Clear();

    public override void OnAttackHit(ICombatTarget target, ref float damage)
    {
        if (!CanUsePassive() || target is not MonsterController monster || monster.IsDead)
            return;

        int stackLimit = skillController.HasPassiveUpgrade2
            ? upgradedMaximumStacks
            : maximumStacks;
        int stacks = 0;
        if (monster.StatusEffects != null &&
            monster.StatusEffects.TryGet(StatusEffectType.Poison, out _, out _))
        {
            stacksByTarget.TryGetValue(monster, out stacks);
        }
        stacks = Mathf.Clamp(stacks + 1, 1, Mathf.Max(1, stackLimit));
        stacksByTarget[monster] = stacks;

        float ratio = firstStackDamageRatio + additionalStackDamageRatio * (stacks - 1);
        monster.ApplyStatusEffect(StatusEffectApplication.Poison(
            duration,
            owner.Attack * ratio,
            owner,
            tickInterval));
    }
}
