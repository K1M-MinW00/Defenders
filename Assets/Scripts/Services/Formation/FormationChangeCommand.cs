public enum FormationChangeType
{
    SwapPositions,
    ReplaceUnit,
}

public sealed class FormationChangeCommand
{
    public FormationChangeType Type { get; }
    public string CurrentUnitId { get; }
    public string TargetUnitId { get; }

    public FormationChangeCommand(
        FormationChangeType type,
        string currentUnitId,
        string targetUnitId)
    {
        Type = type;
        CurrentUnitId = currentUnitId;
        TargetUnitId = targetUnitId;
    }
}
