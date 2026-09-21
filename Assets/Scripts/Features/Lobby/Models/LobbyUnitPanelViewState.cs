using System.Collections.Generic;

public sealed class LobbyUnitPanelViewState
{
    public LobbyUnitPanelViewState(
        IReadOnlyList<LobbyUnitViewModel> selectedUnits,
        IReadOnlyList<LobbyUnitViewModel> availableUnits)
    {
        SelectedUnits = selectedUnits;
        AvailableUnits = availableUnits;
    }

    public IReadOnlyList<LobbyUnitViewModel> SelectedUnits { get; }
    public IReadOnlyList<LobbyUnitViewModel> AvailableUnits { get; }
}
