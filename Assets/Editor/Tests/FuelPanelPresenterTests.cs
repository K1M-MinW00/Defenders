using System;
using Firebase.Firestore;

public sealed class FuelPanelPresenterTests
{
    public void Build_CreatesFuelAndAdState()
    {
        DateTime utcNow = new(2026, 9, 22, 0, 0, 0, DateTimeKind.Utc);
        UserDataRoot userData = CreateUserData(80, 100, 1, "2026-09-22");
        FuelPanelPresenter presenter = new(userData);

        FuelPanelViewState state = presenter.Build(false, true, utcNow);

        Assert(state != null, "Fuel panel state should be created.");
        Assert(state.Fuel == 80 && state.MaxFuel == 100, "Fuel values should be preserved.");
        Assert(state.DailyAdWatchCount == 1, "Daily ad count should be displayed.");
        Assert(state.CanRequestRewardAd, "Reward ad should be available below the limit.");
    }

    public void Build_ResetsExpiredAdCountAndHonorsBusyState()
    {
        DateTime utcNow = new(2026, 9, 22, 0, 0, 0, DateTimeKind.Utc);
        UserDataRoot userData = CreateUserData(100, 100, 2, "2026-09-21");
        FuelPanelPresenter presenter = new(userData);

        FuelPanelViewState state = presenter.Build(true, true, utcNow);

        Assert(state.DailyAdWatchCount == 0, "Expired daily ad count should reset in the view state.");
        Assert(!state.CanRequestRewardAd, "Reward ad should remain disabled while an operation is pending.");
        Assert(userData.Ad.FuelAdWatchCount == 2, "Building UI state must not mutate stored user data.");
    }

    private static UserDataRoot CreateUserData(int fuel, int maxFuel, int adCount, string adDate)
    {
        return new UserDataRoot
        {
            Resource = new UserResourceData
            {
                Fuel = fuel,
                MaxFuel = maxFuel,
                LastFuelUpdateTime = Timestamp.GetCurrentTimestamp(),
            },
            Ad = new UserAdData
            {
                FuelAdWatchCount = adCount,
                AdWatchDate = adDate,
            },
        };
    }

    private static void Assert(bool condition, string message)
    {
        if (!condition)
            throw new InvalidOperationException(message);
    }
}
