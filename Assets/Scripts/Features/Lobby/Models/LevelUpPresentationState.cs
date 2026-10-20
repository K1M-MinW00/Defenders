public readonly struct LevelUpPresentationData
{
    public LevelUpPresentationData(int previousLevel, int currentLevel, int previousMaxFuel, int currentMaxFuel, string iconId)
    {
        PreviousLevel = previousLevel;
        CurrentLevel = currentLevel;
        PreviousMaxFuel = previousMaxFuel;
        CurrentMaxFuel = currentMaxFuel;
        IconId = iconId;
    }

    public int PreviousLevel { get; }
    public int CurrentLevel { get; }
    public int PreviousMaxFuel { get; }
    public int CurrentMaxFuel { get; }
    public string IconId { get; }
    public bool UnlocksResearch => CurrentLevel / 5 > PreviousLevel / 5;
}

public static class LevelUpPresentationState
{
    private static LevelUpPresentationData? pending;

    public static void Set(LevelUpPresentationData data) => pending = data;

    public static bool TryConsume(out LevelUpPresentationData data)
    {
        if (!pending.HasValue)
        {
            data = default;
            return false;
        }

        data = pending.Value;
        pending = null;
        return true;
    }
}
