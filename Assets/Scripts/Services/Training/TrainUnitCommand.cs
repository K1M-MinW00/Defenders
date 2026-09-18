using System.Collections.Generic;

public sealed class TrainUnitCommand
{
    public string UnitId { get; }
    public IReadOnlyDictionary<string, int> Materials { get; }

    public TrainUnitCommand(string unitId, IReadOnlyDictionary<string, int> materials)
    {
        UnitId = unitId;
        Materials = materials == null
            ? null
            : new Dictionary<string, int>(materials);
    }
}
