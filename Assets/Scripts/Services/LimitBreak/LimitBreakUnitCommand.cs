public sealed class LimitBreakUnitCommand
{
    public string UnitId { get; }

    public LimitBreakUnitCommand(string unitId)
    {
        UnitId = unitId;
    }
}
