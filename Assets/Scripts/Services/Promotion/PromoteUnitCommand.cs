public sealed class PromoteUnitCommand
{
    public string UnitId { get; }

    public PromoteUnitCommand(string unitId)
    {
        UnitId = unitId;
    }
}
