public sealed class RecruitUnitsCommand
{
    public GachaDataSO Banner { get; }
    public int Count { get; }

    public RecruitUnitsCommand(GachaDataSO banner, int count)
    {
        Banner = banner;
        Count = count;
    }
}
