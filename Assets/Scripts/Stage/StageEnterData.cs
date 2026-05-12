using System.Collections.Generic;

public class StageEnterData
{
    public int Sector { get; }
    public int Stage { get; }
    public IReadOnlyList<string> SelectedUnitIds { get; }

    public string StageKey => $"{Sector}-{Stage}";

    public StageEnterData(int sector,int stage,IReadOnlyList<string> selectedUnitIds)
    {
        Sector = sector;
        Stage = stage;
        SelectedUnitIds = selectedUnitIds;
    }
}