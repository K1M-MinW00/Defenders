public sealed class RecruitUnitsCommand
{
    public GachaDataSO Banner { get; }
    public int Count { get; }
    public UnitDataSO PickupUnitOverride { get; }

    public RecruitUnitsCommand(GachaDataSO banner, int count, UnitDataSO pickupUnitOverride = null)
    {
        Banner = banner;
        Count = count;
        PickupUnitOverride = pickupUnitOverride;
    }
}
