using System;

public sealed class AdDailyLimitPolicyTests
{
    public void TryConsume_ResetsAtUtcDateBoundary()
    {
        UserAdData adData = new()
        {
            FuelAdWatchCount = AdDailyLimitPolicy.DailyAdLimit,
            AdWatchDate = "2026-09-21",
        };

        bool consumed = AdDailyLimitPolicy.TryConsume(
            adData,
            DailyAdType.Fuel,
            new DateTime(2026, 9, 22, 0, 0, 0, DateTimeKind.Utc));

        Assert(consumed, "A new UTC date should reset and allow the reward.");
        Assert(adData.FuelAdWatchCount == 1 && adData.AdWatchDate == "2026-09-22", "Reset ad data should consume exactly one view.");
    }

    public void GetWatchCount_ClampsInvalidNegativeData()
    {
        UserAdData adData = new() { FuelAdWatchCount = -3 };

        Assert(AdDailyLimitPolicy.GetWatchCount(adData, DailyAdType.Fuel) == 0, "Negative ad count should be treated as zero.");
    }

    private static void Assert(bool condition, string message)
    {
        if (!condition)
            throw new InvalidOperationException(message);
    }
}
