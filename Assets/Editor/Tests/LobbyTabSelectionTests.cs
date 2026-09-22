using System;

public sealed class LobbyTabSelectionTests
{
    public void TrySelect_RejectsInvalidAndDuplicateSelection()
    {
        LobbyTabSelection selection = new(3);

        Assert(!selection.TrySelect(-1), "Negative tab index should be rejected.");
        Assert(!selection.TrySelect(3), "Out-of-range tab index should be rejected.");
        Assert(selection.TrySelect(1), "Valid tab should be selected.");
        Assert(!selection.TrySelect(1), "Duplicate selection should be ignored.");
        Assert(selection.SelectedIndex == 1, "Selected tab index should be retained.");
    }

    public void TrySelect_AllowsChangingSelection()
    {
        LobbyTabSelection selection = new(2);

        Assert(selection.TrySelect(0), "First tab should be selected.");
        Assert(selection.TrySelect(1), "Selection should change to another tab.");
        Assert(selection.SelectedIndex == 1, "Changed selection should be retained.");
    }

    private static void Assert(bool condition, string message)
    {
        if (!condition)
            throw new InvalidOperationException(message);
    }
}
