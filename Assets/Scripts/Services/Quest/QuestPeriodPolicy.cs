using System;

public static class QuestPeriodPolicy
{
    private static readonly TimeSpan KoreaOffset = TimeSpan.FromHours(9);

    public static string GetDailyKey(DateTimeOffset utcNow)
    {
        DateTime date = utcNow.ToOffset(KoreaOffset).Date;
        return $"D:{date:yyyyMMdd}";
    }

    public static string GetWeeklyKey(DateTimeOffset utcNow)
    {
        DateTime date = utcNow.ToOffset(KoreaOffset).Date;
        int daysSinceSunday = (int)date.DayOfWeek;
        return $"W:{date.AddDays(-daysSinceSunday):yyyyMMdd}";
    }

    public static DateTimeOffset GetNextDailyResetUtc(DateTimeOffset utcNow)
    {
        DateTimeOffset korea = utcNow.ToOffset(KoreaOffset);
        return new DateTimeOffset(korea.Date.AddDays(1), KoreaOffset).ToUniversalTime();
    }
}
