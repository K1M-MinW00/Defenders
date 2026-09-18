using System;
using System.Threading.Tasks;

public sealed class UserDataLoader
{
    private readonly IUserDataRepository repository;

    public UserDataLoader(IUserDataRepository repository)
    {
        this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async Task<UserDataRoot> LoadOrCreateAsync(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User ID is null or empty.", nameof(userId));

        UserDataLoadResult loadResult = await repository.LoadAsync(userId);

        if (!loadResult.Exists || loadResult.Data == null)
        {
            UserDataRoot newUserData = UserDataFactory.CreateDefault(userId);
            StaminaService.InitializeFullFuel(newUserData.Resource);
            await repository.CreateAsync(userId, newUserData);
            return newUserData;
        }

        UserDataRoot userData = loadResult.Data;
        bool wasMigrated = UserDataMigrator.MigrateToCurrent(userData, userId);
        bool fuelChanged = StaminaService.RefreshFuel(userData.Resource);

        if (wasMigrated)
            await repository.SaveAllAsync(userId, userData);
        else if (fuelChanged)
            await repository.SaveResourcesAsync(userId, userData.Resource);

        return userData;
    }
}
