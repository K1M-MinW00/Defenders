using Firebase.Firestore;
using System;
using System.Threading.Tasks;

public sealed class ClaimAdFuelRewardUseCase
{
    private readonly IUserDataRepository repository;
    private readonly string userId;
    private readonly UserDataRoot userData;
    private bool isExecuting;

    public ClaimAdFuelRewardUseCase(IUserDataRepository repository, string userId, UserDataRoot userData)
    {
        this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
        this.userId = string.IsNullOrWhiteSpace(userId)
            ? throw new ArgumentException("User ID is null or empty.", nameof(userId))
            : userId;
        this.userData = userData ?? throw new ArgumentNullException(nameof(userData));
    }

    public async Task<ClaimAdFuelRewardResult> ExecuteAsync(int fuelAmount)
    {
        if (isExecuting || fuelAmount <= 0 || userData.Resource == null || userData.Ad == null)
            return ClaimAdFuelRewardResult.Fail(ClaimAdFuelRewardFailure.InvalidRequest);

        UserResourceData nextResources = UserDataCloner.Copy(userData.Resource);
        UserAdData nextAd = UserDataCloner.Copy(userData.Ad);
        DateTime utcNow = DateTime.UtcNow;

        StaminaService.RefreshFuel(nextResources);

        if (!AdDailyLimitPolicy.TryConsume(nextAd, DailyAdType.Fuel, utcNow))
            return ClaimAdFuelRewardResult.Fail(ClaimAdFuelRewardFailure.DailyLimitReached);

        try
        {
            nextResources.Fuel = checked(nextResources.Fuel + fuelAmount);
        }
        catch (OverflowException)
        {
            return ClaimAdFuelRewardResult.Fail(ClaimAdFuelRewardFailure.Overflow);
        }

        if (nextResources.Fuel >= nextResources.MaxFuel)
            nextResources.LastFuelUpdateTime = Timestamp.GetCurrentTimestamp();

        isExecuting = true;

        try
        {
            await repository.SaveSectionsAsync(userId, new UserDataUpdate
            {
                Resources = nextResources,
                Ad = nextAd,
            });
        }
        catch
        {
            return ClaimAdFuelRewardResult.Fail(ClaimAdFuelRewardFailure.SaveFailed);
        }
        finally
        {
            isExecuting = false;
        }

        userData.Resource = nextResources;
        userData.Ad = nextAd;
        return ClaimAdFuelRewardResult.Success();
    }
}
