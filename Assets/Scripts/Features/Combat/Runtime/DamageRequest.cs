public enum DamageOrigin
{
    Unknown = 0,
    BasicAttack = 1,
    Skill = 2,
    Effect = 3
}

public readonly struct DamageRequest
{
    public float Amount { get; }
    public ICombatTarget Source { get; }
    public DamageOrigin Origin { get; }

    public DamageRequest(
        float amount,
        ICombatTarget source = null,
        DamageOrigin origin = DamageOrigin.Unknown)
    {
        Amount = amount;
        Source = source;
        Origin = origin;
    }
}
