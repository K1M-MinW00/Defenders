using System;
using System.Threading.Tasks;
using Firebase.Firestore;

public sealed class PurchaseFuelUseCase
{
    private readonly IUserDataRepository repository;
    private readonly string userId;
    private readonly UserDataRoot userData;
    private bool isExecuting;

    public PurchaseFuelUseCase(IUserDataRepository repository, string userId, UserDataRoot userData)
    {
        this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
        this.userId = string.IsNullOrWhiteSpace(userId)
            ? throw new ArgumentException("User ID is null or empty.", nameof(userId))
            : userId;
        this.userData = userData ?? throw new ArgumentNullException(nameof(userData));
    }

    public async Task<PurchaseFuelResult> ExecuteAsync(int gemCost, int fuelAmount)
    {
        if (isExecuting || gemCost <= 0 || fuelAmount <= 0 || userData.Resource == null)
            return PurchaseFuelResult.Fail(PurchaseFuelFailure.InvalidRequest);

        if (userData.Resource.Gem < gemCost)
            return PurchaseFuelResult.Fail(PurchaseFuelFailure.InsufficientGem);

        UserResourceData nextResources = UserDataCloner.Copy(userData.Resource);
        StaminaService.RefreshFuel(nextResources);

        try
        {
            nextResources.Gem -= gemCost;
            nextResources.Fuel = checked(nextResources.Fuel + fuelAmount);
        }
        catch (OverflowException)
        {
            return PurchaseFuelResult.Fail(PurchaseFuelFailure.Overflow);
        }

        if (nextResources.Fuel >= nextResources.MaxFuel)
            nextResources.LastFuelUpdateTime = Timestamp.GetCurrentTimestamp();

        isExecuting = true;

        try
        {
            await repository.SaveResourcesAsync(userId, nextResources);
        }
        catch
        {
            return PurchaseFuelResult.Fail(PurchaseFuelFailure.SaveFailed);
        }
        finally
        {
            isExecuting = false;
        }

        userData.Resource = nextResources;
        return PurchaseFuelResult.Success();
    }
}
