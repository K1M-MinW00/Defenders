using System;
using System.Globalization;

public static class AdDailyLimitPolicy
{
    public const int DailyAdLimit = 2;

    public static bool Refresh(UserAdData adData, DateTime utcNow)
    {
        if (adData == null)
            return false;

        string today = utcNow.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

        if (adData.AdWatchDate == today)
            return false;

        adData.FuelAdWatchCount = 0;
        adData.GemAdWatchCount = 0;
        adData.AdWatchDate = today;
        return true;
    }

    public static bool CanWatch(UserAdData adData, DailyAdType type, DateTime utcNow)
    {
        if (adData == null)
            return false;

        Refresh(adData, utcNow);
        return GetWatchCount(adData, type) < DailyAdLimit;
    }

    public static int GetWatchCount(UserAdData adData, DailyAdType type)
    {
        if (adData == null)
            return 0;

        return type switch
        {
            DailyAdType.Fuel => adData.FuelAdWatchCount,
            DailyAdType.Gem => adData.GemAdWatchCount,
            _ => 0,
        };
    }

    public static bool TryConsume(UserAdData adData, DailyAdType type, DateTime utcNow)
    {
        if (!CanWatch(adData, type, utcNow))
            return false;

        switch (type)
        {
            case DailyAdType.Fuel:
                adData.FuelAdWatchCount++;
                return true;

            case DailyAdType.Gem:
                adData.GemAdWatchCount++;
                return true;

            default:
                return false;
        }
    }
}
