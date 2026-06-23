public class RecruitCostModel
{
    public int TicketUseCount;
    public int GemUseCount;

    public bool NeedGem => GemUseCount > 0;
}