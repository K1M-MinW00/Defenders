public readonly struct DamageResolution
{
    public float RequestedAmount { get; }
    public float ModifiedAmount { get; }
    public float AppliedAmount { get; }
    public bool CanApply => RejectReason == DamageRejectReason.None && AppliedAmount > 0f;
    public bool IsLethal { get; }
    public DamageRejectReason RejectReason { get; }

    private DamageResolution(
        float requestedAmount,
        float modifiedAmount,
        float appliedAmount,
        bool isLethal,
        DamageRejectReason rejectReason)
    {
        RequestedAmount = requestedAmount;
        ModifiedAmount = modifiedAmount;
        AppliedAmount = appliedAmount;
        IsLethal = isLethal;
        RejectReason = rejectReason;
    }

    public static DamageResolution Accepted(
        float requestedAmount,
        float modifiedAmount,
        float appliedAmount,
        bool isLethal)
    {
        return new DamageResolution(
            requestedAmount,
            modifiedAmount,
            appliedAmount,
            isLethal,
            DamageRejectReason.None);
    }

    public static DamageResolution Rejected(float requestedAmount, DamageRejectReason reason)
    {
        return new DamageResolution(requestedAmount, 0f, 0f, false, reason);
    }
}
