using Firebase.Firestore;
using System;

public static class StaminaService
{
    public const int RecoverSecondsPerFuel = 300;

    public static void InitializeFullFuel(UserResourceData resources)
    {
        if (resources == null)
            return;

        Timestamp now = Timestamp.GetCurrentTimestamp();
        resources.Fuel = resources.MaxFuel;
        resources.LastFuelUpdateTime = now;
    }

    public static bool RefreshFuel(UserResourceData resources)
    {
        if (resources == null)
            return false;

        int maxFuel = resources.MaxFuel;
        Timestamp now = Timestamp.GetCurrentTimestamp();

        if (resources.Fuel >= maxFuel)
        {
            resources.LastFuelUpdateTime = now;
            return false;
        }

        if (IsInvalidTimestamp(resources.LastFuelUpdateTime))
            return false;

        long elapsedSeconds = GetElapsedSeconds(resources.LastFuelUpdateTime, now);

        if (elapsedSeconds < RecoverSecondsPerFuel)
            return false;

        int recoveredFuel = (int)(elapsedSeconds / RecoverSecondsPerFuel);
        
        if (recoveredFuel <= 0)
            return false;

        int oldFuel = resources.Fuel;

        resources.Fuel = Math.Min(maxFuel, resources.Fuel + recoveredFuel);

        if (resources.Fuel >= maxFuel)
        {
            resources.Fuel = maxFuel;
            resources.LastFuelUpdateTime = now;
        }
        else
        {
            long consumedSeconds = recoveredFuel * RecoverSecondsPerFuel;

            resources.LastFuelUpdateTime = AddSeconds(resources.LastFuelUpdateTime,consumedSeconds);
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
            resources.LastFuelUpdateTime = Timestamp.GetCurrentTimestamp();
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

        if (resources.Fuel >= resources.MaxFuel)
            resources.LastFuelUpdateTime = Timestamp.GetCurrentTimestamp();
    }

    public static int GetRemainingSecondsToNextFuel(UserResourceData resources)
    {
        if (resources == null)
            return 0;

        if (resources.Fuel >= resources.MaxFuel)
            return 0;

        if (IsInvalidTimestamp(resources.LastFuelUpdateTime))
            return 0;
        
        Timestamp now = Timestamp.GetCurrentTimestamp();
        long elapsedSeconds = GetElapsedSeconds(resources.LastFuelUpdateTime,now);

        int elapsedInCurrentCycle = (int)(elapsedSeconds % RecoverSecondsPerFuel);

        int remain = RecoverSecondsPerFuel - elapsedInCurrentCycle;

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

    private static long GetElapsedSeconds(Timestamp from,Timestamp to)
    {
        DateTime fromDateTime = from.ToDateTime();
        DateTime toDateTime = to.ToDateTime();

        return (long)(toDateTime - fromDateTime).TotalSeconds;
    }

    private static Timestamp AddSeconds(Timestamp timestamp,long seconds)
    {
        DateTime dateTime = timestamp.ToDateTime();

        return Timestamp.FromDateTime(dateTime.AddSeconds(seconds));
    }

    private static bool IsInvalidTimestamp(Timestamp timestamp)
    {
        return timestamp == null || timestamp.ToDateTime() == DateTime.MinValue;
    }
}