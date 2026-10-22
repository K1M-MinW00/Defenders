public static class UnitBasicAttackHitResolver
{
    public static DamageResult Apply(
        UnitController source,
        ICombatTarget target,
        ICombatHealth targetHealth,
        float damage)
    {
        if (targetHealth == null)
            return DamageResult.Rejected(damage, DamageRejectReason.InvalidTarget);

        float resolvedDamage = damage;
        source?.SkillController?.NotifyAttackHit(target, ref resolvedDamage);
        return targetHealth.ApplyDamage(
            new DamageRequest(resolvedDamage, source, DamageOrigin.BasicAttack));
    }
}
