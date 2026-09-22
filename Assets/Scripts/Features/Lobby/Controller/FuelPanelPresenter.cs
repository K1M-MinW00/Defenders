using System;

public sealed class FuelPanelPresenter
{
    private readonly UserDataRoot userData;

    public FuelPanelPresenter(UserDataRoot userData)
    {
        this.userData = userData ?? throw new ArgumentNullException(nameof(userData));
    }

    public FuelPanelViewState Build(bool isRewardAdBusy, DateTime utcNow)
    {
        if (userData.Resource == null || userData.Ad == null)
            return null;

        UserResourceData resources = UserDataCloner.Copy(userData.Resource);
        UserAdData adData = UserDataCloner.Copy(userData.Ad);

        StaminaService.RefreshFuel(resources);
        AdDailyLimitPolicy.Refresh(adData, utcNow);

        int watchCount = AdDailyLimitPolicy.GetWatchCount(adData, DailyAdType.Fuel);
        return new FuelPanelViewState
        {
            Fuel = resources.Fuel,
            MaxFuel = resources.MaxFuel,
            NextRecoverSeconds = StaminaService.GetRemainingSecondsToNextFuel(resources),
            FullRecoverSeconds = StaminaService.GetRemainingSecondsToFullFuel(resources),
            DailyAdWatchCount = watchCount,
            DailyAdLimit = AdDailyLimitPolicy.DailyAdLimit,
            CanRequestRewardAd = !isRewardAdBusy && watchCount < AdDailyLimitPolicy.DailyAdLimit,
        };
    }

    public bool CanWatchRewardAd(DateTime utcNow)
    {
        if (userData.Ad == null)
            return false;

        UserAdData adData = UserDataCloner.Copy(userData.Ad);
        return AdDailyLimitPolicy.CanWatch(adData, DailyAdType.Fuel, utcNow);
    }
}
