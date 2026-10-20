public static class AccountLevelBenefitPolicy
{
    public const int MaxFuelPerLevel = 4;

    public static int GetMaxFuelIncrease(int gainedLevels) =>
        gainedLevels > 0 ? checked(gainedLevels * MaxFuelPerLevel) : 0;

    public static bool IsResearchMilestone(int level) => level > 0 && level % 5 == 0;
}
