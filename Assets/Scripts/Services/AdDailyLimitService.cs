using System;
using Firebase.Firestore;

public static class AdDailyLimitService
{
    public const int DailyAdLimit = 2;

    /// <summary>
    /// 현재 날짜가 변경되었다면 모든 일일 광고 횟수를 초기화한다.
    /// </summary>
    public static bool Refresh(UserAdData adData)
    {
        if (adData == null)
            return false;

        string today = DateTime.Now.ToString("yyyy-MM-dd");

        if (today == adData.AdWatchDate)
            return false;

        // 최초 생성된 데이터라면 리셋 시간을 초기화한다.
        adData.FuelAdWatchCount = 0;
        adData.GemAdWatchCount = 0;
        adData.AdWatchDate = today;
        
        return true;
    }

    /// <summary>
    /// 특정 광고를 오늘 시청할 수 있는지 확인한다.
    /// </summary>
    public static bool CanWatch(UserAdData adData,DailyAdType type)
    {
        if (adData == null)
            return false;

        Refresh(adData);

        return GetWatchCountWithoutRefresh(adData, type) < DailyAdLimit;
    }

    /// <summary>
    /// 특정 광고의 현재 시청 횟수를 반환한다.
    /// </summary>
    public static int GetWatchCount(UserAdData adData,DailyAdType type)
    {
        if (adData == null)
            return 0;

        Refresh(adData);

        return GetWatchCountWithoutRefresh(adData, type);
    }

    private static int GetWatchCountWithoutRefresh(UserAdData adData,DailyAdType type)
    {
        return type switch
        {
            DailyAdType.Fuel => adData.FuelAdWatchCount,
            DailyAdType.Gem => adData.GemAdWatchCount,
            _ => 0
        };
    }

    /// <summary>
    /// 특정 광고의 시청 횟수를 1 증가시킨다.
    /// </summary>
    public static bool Consume(UserAdData adData,DailyAdType type)
    {
        if (adData == null)
            return false;

        Refresh(adData);

        switch (type)
        {
            case DailyAdType.Fuel:

                if (adData.FuelAdWatchCount >= DailyAdLimit)
                    return false;

                adData.FuelAdWatchCount++;
                return true;

            case DailyAdType.Gem:

                if (adData.GemAdWatchCount >= DailyAdLimit)
                    return false;

                adData.GemAdWatchCount++;
                return true;

            default:
                return false;
        }
    }
}