public enum DamageRejectReason
{
    None = 0,
    InvalidAmount = 1,
    TargetAlreadyDead = 2,
    FullyPrevented = 3
}

public readonly struct DamageResult
{
    public float RequestedAmount { get; }
    public float AppliedAmount { get; }
    public bool WasApplied => AppliedAmount > 0f;
    public bool WasLethal { get; }
    public DamageRejectReason RejectReason { get; }

    private DamageResult(
        float requestedAmount,
        float appliedAmount,
        bool wasLethal,
        DamageRejectReason rejectReason)
    {
        RequestedAmount = requestedAmount;
        AppliedAmount = appliedAmount;
        WasLethal = wasLethal;
        RejectReason = rejectReason;
    }

    public static DamageResult Applied(float requestedAmount, float appliedAmount, bool wasLethal)
    {
        return new DamageResult(requestedAmount, appliedAmount, wasLethal, DamageRejectReason.None);
    }

    public static DamageResult Rejected(float requestedAmount, DamageRejectReason reason)
    {
        return new DamageResult(requestedAmount, 0f, false, reason);
    }
}
