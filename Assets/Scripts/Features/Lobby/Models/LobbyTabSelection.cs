public sealed class LobbyTabSelection
{
    private readonly int tabCount;

    public int SelectedIndex { get; private set; } = -1;

    public LobbyTabSelection(int tabCount)
    {
        this.tabCount = tabCount < 0 ? 0 : tabCount;
    }

    public bool TrySelect(int tabIndex)
    {
        if (tabIndex < 0 || tabIndex >= tabCount || tabIndex == SelectedIndex)
            return false;

        SelectedIndex = tabIndex;
        return true;
    }
}
