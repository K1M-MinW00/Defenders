using UnityEngine;

public class Lancer_BattleInstinct:PassiveSkillBase
{
    [Header("Battle Instinct")]
    [SerializeField] private int maxStacks = 5;
    [SerializeField] private int upgrade_maxStacks = 4;

    [SerializeField] private float attackBonusPerStack = 0.10f;
    [SerializeField] private float bonusDamageOnMaxStack = 0.30f;

    private int currentStacks;
    private bool maxStackReady;

    protected override void ResetRuntimeState()
    {
        currentStacks = 0;
        maxStackReady = false;
    }

    public override void OnAttackStarted(MonsterController target)
    {
        if (!CanUsePassive())
            return;

        int stacks = skillController.HasPassiveUpgrade2 ? upgrade_maxStacks : maxStacks;

        if (currentStacks < stacks)
            currentStacks++;

        if (currentStacks >= stacks)
            maxStackReady = true;
    }

    public override void OnAttackHit(MonsterController target, ref float damage)
    {
        if (owner == null || owner.IsDead)
            return;

        // 스택 기반 공격력 증가
        float stackBonus = 1f + (currentStacks * attackBonusPerStack);
        damage *= stackBonus;

        // 최대 스택 시 추가 피해
        if (maxStackReady)
        {
            damage += owner.Attack * bonusDamageOnMaxStack;

            currentStacks = 0;
            maxStackReady = false;
        }
    }
}