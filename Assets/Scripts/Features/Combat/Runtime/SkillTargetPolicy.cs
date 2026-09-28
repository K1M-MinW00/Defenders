public static class SkillTargetPolicy
{
    public static bool CanApply(ActiveSkillTargetType targetType, SkillExecutionContext context)
    {
        if (context == null || !context.IsValid)
            return false;

        switch (targetType)
        {
            case ActiveSkillTargetType.EnemyInRange:
            case ActiveSkillTargetType.EnemyInRangeOrGlobalClosest:
                return CombatTargetSelector.IsValid(context.EnemyTarget);

            case ActiveSkillTargetType.LowestHpAlliesInRangeOrGlobal:
            case ActiveSkillTargetType.ClosestAllies:
                return HasAliveAlly(context);

            case ActiveSkillTargetType.SelfArea:
                return context.Caster == null || !context.Caster.IsDead;

            default:
                return false;
        }
    }

    private static bool HasAliveAlly(SkillExecutionContext context)
    {
        for (int i = 0; i < context.AllyTargets.Count; i++)
        {
            UnitController target = context.AllyTargets[i];
            if (target != null && !target.IsDead)
                return true;
        }

        return false;
    }
}
