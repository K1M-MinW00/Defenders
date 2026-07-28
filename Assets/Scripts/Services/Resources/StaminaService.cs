using System;

public static class StaminaService
{
    public const int RecoverSecondsPerFuel = 300;

    public static long GetUnixNow()
    {
        return DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public static void InitializeFullFuel(UserResourceData resources)
    {
        if (resources == null)
            return;

        resources.Fuel = resources.MaxFuel;
        resources.LastFuelUpdateTime = GetUnixNow();
    }

    public static bool RefreshFuel(UserResourceData resources)
    {
        if (resources == null)
            return false;

        int maxFuel = resources.MaxFuel;
        long now = GetUnixNow();

        if (resources.Fuel >= maxFuel)
        {
            resources.LastFuelUpdateTime = now;
            return false;
        }

        if (resources.LastFuelUpdateTime <= 0)
        {
            return false;
        }

        long elapsed = now - resources.LastFuelUpdateTime;

        if (elapsed < RecoverSecondsPerFuel)
            return false;

        int recoveredFuel = (int)(elapsed / RecoverSecondsPerFuel);

        if (recoveredFuel <= 0)
            return false;

        int oldFuel = resources.Fuel;

        resources.Fuel = Math.Min(maxFuel, resources.Fuel + recoveredFuel);

        if (resources.Fuel >= maxFuel)
        {
            resources.Fuel = maxFuel;
        }
        else
        {
            resources.LastFuelUpdateTime += recoveredFuel * RecoverSecondsPerFuel;
        }

        return oldFuel != resources.Fuel;
    }

    public static bool ConsumeFuel(UserResourceData resources, int amount)
    {
        if (resources == null)
            return false;

        if (amount <= 0)
            return false;

        if (resources.Fuel < amount)
            return false;

        bool wasFull = resources.Fuel >= resources.MaxFuel;

        resources.Fuel -= amount;

        if (wasFull)
        {
            resources.LastFuelUpdateTime = GetUnixNow();
        }

        return true;
    }

    public static void AddFuel(UserResourceData resources, int amount, bool force = false)
    {
        if (resources == null)
            return;

        if (amount <= 0)
            return;

        if(force)
            resources.Fuel += amount;

        else
            resources.Fuel = Math.Min(resources.MaxFuel, resources.Fuel + amount);
    }

    public static int GetRemainingSecondsToNextFuel(UserResourceData resources)
    {
        if (resources == null)
            return 0;

        if (resources.Fuel >= resources.MaxFuel)
            return 0;

        long now = GetUnixNow();

        long elapsed = now - resources.LastFuelUpdateTime;

        int remain = RecoverSecondsPerFuel - (int)(elapsed % RecoverSecondsPerFuel);

        return Math.Max(remain, 0);
    }

    public static int GetRemainingSecondsToFullFuel(UserResourceData resources)
    {
        if (resources == null)
            return 0;

        if (resources.Fuel >= resources.MaxFuel)
            return 0;

        int remainFuel = resources.MaxFuel - resources.Fuel;

        int nextRecover = GetRemainingSecondsToNextFuel(resources);

        return ((remainFuel - 1) * RecoverSecondsPerFuel) + nextRecover;
    }
}