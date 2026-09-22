using System.Collections.Generic;

public sealed class RecruitRatePreviewRow
{
    public string Label { get; }
    public float TotalRate { get; }
    public IReadOnlyList<UnitDataSO> Units { get; }

    public RecruitRatePreviewRow(string label, float totalRate, IReadOnlyList<UnitDataSO> units)
    {
        Label = label;
        TotalRate = totalRate;
        Units = units;
    }
}
