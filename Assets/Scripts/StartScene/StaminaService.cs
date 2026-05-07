using System;

public static class StaminaService
{
    public const int MaxFuel = 100;
    public const int RecoverSecondsPerFuel = 300; // 5분

    public static long GetUnixNow()
    {
        return DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public static bool RefreshFuel(UserResourceData resources)
    {
        if (resources == null)
            return false;

        long now = GetUnixNow();

        if (resources.LastFuelUpdateTime <= 0)
        {
            resources.LastFuelUpdateTime = now;
            return true;
        }

        if (resources.Fuel >= MaxFuel)
        {
            resources.Fuel = MaxFuel;
            resources.LastFuelUpdateTime = now;
            return true;
        }

        long elapsed = now - resources.LastFuelUpdateTime;

        if (elapsed < RecoverSecondsPerFuel)
            return false;

        int recovered = (int)(elapsed / RecoverSecondsPerFuel);

        if (recovered <= 0)
            return false;

        int oldFuel = resources.Fuel;
        resources.Fuel = Math.Min(MaxFuel, resources.Fuel + recovered);

        if (resources.Fuel >= MaxFuel)
            resources.LastFuelUpdateTime = now;
        else
            resources.LastFuelUpdateTime += recovered * RecoverSecondsPerFuel;

        return resources.Fuel != oldFuel;
    }

    public static int GetRemainingSecondsToNextFuel(UserResourceData resources)
    {
        if (resources == null)
            return 0;

        if (resources.Fuel >= MaxFuel)
            return 0;

        long now = GetUnixNow();
        long elapsed = now - resources.LastFuelUpdateTime;

        int remaining = RecoverSecondsPerFuel - (int)(elapsed % RecoverSecondsPerFuel);

        if (remaining < 0)
            return 0;

        return remaining;
    }

    public static string FormatSecondsToMinuteSecond(int seconds)
    {
        if (seconds < 0)
            seconds = 0;

        int minute = seconds / 60;
        int second = seconds % 60;

        return $"{minute:00}:{second:00}";
    }
}