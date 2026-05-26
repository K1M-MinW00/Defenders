public static class UnitExpTable
{
    public const int MaxLevel = 50;

    public static int GetRequiredExp(int level)
    {
        if (level >= MaxLevel)
            return 0;

        if (level <= 10)
            return level * 200;

        if (level <= 20)
            return 2000 + ((level - 10) * 400);

        if (level <= 30)
            return 6000 + ((level - 20) * 600);

        if (level <= 34)
            return 12000 + ((level - 30) * 1000);

        if (level <= 39)
            return 16000 + ((level - 34) * 5000);

        return 41000 + ((level - 39) * 10000);
    }
}