using System;
using System.Globalization;

public static class ShopTimePolicy
{
    private static readonly TimeSpan ResetOffset = TimeSpan.FromHours(9);
    private const string PermanentPeriodKey = "permanent";

    public static bool IsInSalePeriod(ShopProductData product, DateTimeOffset utcNow)
    {
        if (product == null)
            return false;

        return (!TryParseUtc(product.SaleStartUtc, out DateTimeOffset start) || utcNow >= start) &&
               (!TryParseUtc(product.SaleEndUtc, out DateTimeOffset end) || utcNow < end);
    }

    public static string GetPeriodKey(ShopResetPeriod resetPeriod, DateTimeOffset utcNow)
    {
        DateTimeOffset local = utcNow.ToOffset(ResetOffset).AddHours(-9);
        DateTime date = local.Date;

        return resetPeriod switch
        {
            ShopResetPeriod.Daily => $"D:{date:yyyyMMdd}",
            ShopResetPeriod.Weekly => $"W:{GetMonday(date):yyyyMMdd}",
            ShopResetPeriod.Monthly => $"M:{date:yyyyMM}",
            _ => PermanentPeriodKey,
        };
    }

    public static DateTimeOffset? GetNextResetUtc(ShopResetPeriod resetPeriod, DateTimeOffset utcNow)
    {
        DateTimeOffset shifted = utcNow.ToOffset(ResetOffset).AddHours(-9);
        DateTime next = resetPeriod switch
        {
            ShopResetPeriod.Daily => shifted.Date.AddDays(1),
            ShopResetPeriod.Weekly => GetMonday(shifted.Date).AddDays(7),
            ShopResetPeriod.Monthly => new DateTime(shifted.Year, shifted.Month, 1).AddMonths(1),
            _ => default,
        };

        if (resetPeriod == ShopResetPeriod.None)
            return null;

        return new DateTimeOffset(next.AddHours(9), ResetOffset).ToUniversalTime();
    }

    public static bool TryParseUtc(string value, out DateTimeOffset result)
    {
        return DateTimeOffset.TryParse(
            value,
            CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
            out result);
    }

    private static DateTime GetMonday(DateTime date)
    {
        int daysSinceMonday = ((int)date.DayOfWeek + 6) % 7;
        return date.AddDays(-daysSinceMonday);
    }
}
