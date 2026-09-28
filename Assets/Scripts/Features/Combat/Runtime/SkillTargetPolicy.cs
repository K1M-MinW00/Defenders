public static class SkillTargetPolicy
{
    public static bool TryResolve(
        ActiveSkillTargetType targetType,
        TargetResolutionPolicy resolutionPolicy,
        SkillExecutionContext context,
        UnitController owner)
    {
        if (context == null || !context.IsValid)
            return false;

        if (resolutionPolicy != TargetResolutionPolicy.RetargetOnResolve)
            return CanApply(targetType, resolutionPolicy, context);

        if (targetType != ActiveSkillTargetType.EnemyInRange &&
            targetType != ActiveSkillTargetType.EnemyInRangeOrGlobalClosest)
        {
            return CanApply(targetType, context);
        }

        if (CombatTargetSelector.IsValid(context.EnemyTarget))
            return true;

        ICombatTarget replacement = owner?.Targeting?.GetClosestEnemyInRange();
        if (!CombatTargetSelector.IsValid(replacement))
            return false;

        context.SetEnemyTarget(replacement);
        owner.Animation.FaceTarget(replacement);
        return true;
    }

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

    public static bool CanApply(
        ActiveSkillTargetType targetType,
        TargetResolutionPolicy resolutionPolicy,
        SkillExecutionContext context)
    {
        if (context == null || !context.IsValid)
            return false;

        switch (resolutionPolicy)
        {
            case TargetResolutionPolicy.LockDirection:
                return context.CastDirection.sqrMagnitude > 0.0001f;

            case TargetResolutionPolicy.LockPosition:
            case TargetResolutionPolicy.Self:
                return context.Caster == null || !context.Caster.IsDead;

            case TargetResolutionPolicy.LockTarget:
            case TargetResolutionPolicy.RetargetOnResolve:
            default:
                return CanApply(targetType, context);
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
