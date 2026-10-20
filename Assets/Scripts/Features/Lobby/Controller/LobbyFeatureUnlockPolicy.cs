public static class LobbyFeatureUnlockPolicy
{
    public const int LaboratoryRequiredLevel = 5;

    public static bool IsUnlocked(int accountLevel, int requiredLevel) =>
        requiredLevel <= 0 || accountLevel >= requiredLevel;
}
