using System.Collections.Generic;

public class StageEnterData
{
    public int Sector { get; }
    public int Stage { get; }
    public IReadOnlyList<string> SelectedUnitIds { get; }
    public int EntryFuelCost { get; }
    public bool IsTutorial { get; }

    public string StageKey => $"{Sector}-{Stage}";

    public StageEnterData(
        int sector,
        int stage,
        IReadOnlyList<string> selectedUnitIds,
        int entryFuelCost = 0,
        bool isTutorial = false)
    {
        Sector = sector;
        Stage = stage;
        SelectedUnitIds = selectedUnitIds;
        EntryFuelCost = entryFuelCost;
        IsTutorial = isTutorial;
    }

    public StageEnterData WithEntryFuelCost(int entryFuelCost)
    {
        return new StageEnterData(
            Sector,
            Stage,
            SelectedUnitIds,
            System.Math.Max(0, entryFuelCost),
            IsTutorial);
    }

    public StageEnterData AsTutorial()
    {
        return new StageEnterData(Sector, Stage, SelectedUnitIds, 0, isTutorial: true);
    }
}
