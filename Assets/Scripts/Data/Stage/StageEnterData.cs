using System.Collections.Generic;

public class StageEnterData
{
    public int Sector { get; }
    public int Stage { get; }
    public IReadOnlyList<string> SelectedUnitIds { get; }
    public int EntryFuelCost { get; }

    public string StageKey => $"{Sector}-{Stage}";

    public StageEnterData(
        int sector,
        int stage,
        IReadOnlyList<string> selectedUnitIds,
        int entryFuelCost = 0)
    {
        Sector = sector;
        Stage = stage;
        SelectedUnitIds = selectedUnitIds;
        EntryFuelCost = entryFuelCost;
    }

    public StageEnterData WithEntryFuelCost(int entryFuelCost)
    {
        return new StageEnterData(
            Sector,
            Stage,
            SelectedUnitIds,
            System.Math.Max(0, entryFuelCost));
    }
}
