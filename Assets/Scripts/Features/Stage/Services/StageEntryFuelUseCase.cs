using System;
using System.Threading.Tasks;
using Firebase.Firestore;

public enum StageEntryFuelFailure
{
    None,
    InvalidRequest,
    InsufficientFuel,
    SaveFailed,
}

public sealed class StageEntryFuelResult
{
    public bool Succeeded => Failure == StageEntryFuelFailure.None;
    public StageEntryFuelFailure Failure { get; }

    private StageEntryFuelResult(StageEntryFuelFailure failure)
    {
        Failure = failure;
    }

    public static StageEntryFuelResult Success() => new(StageEntryFuelFailure.None);
    public static StageEntryFuelResult Fail(StageEntryFuelFailure failure) => new(failure);
}

public sealed class StageEntryFuelUseCase
{
    private readonly IUserDataRepository repository;
    private readonly string userId;
    private readonly UserDataRoot userData;

    public StageEntryFuelUseCase(IUserDataRepository repository, string userId, UserDataRoot userData)
    {
        this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
        this.userId = string.IsNullOrWhiteSpace(userId)
            ? throw new ArgumentException("User ID is null or empty.", nameof(userId))
            : userId;
        this.userData = userData ?? throw new ArgumentNullException(nameof(userData));
    }

    public async Task<StageEntryFuelResult> ConsumeAsync(int amount)
    {
        if (amount <= 0 || userData.Resource == null)
            return StageEntryFuelResult.Fail(StageEntryFuelFailure.InvalidRequest);

        UserResourceData next = UserDataCloner.Copy(userData.Resource);
        if (!StaminaService.ConsumeFuel(next, amount))
            return StageEntryFuelResult.Fail(StageEntryFuelFailure.InsufficientFuel);

        if (!await TrySaveAsync(next))
            return StageEntryFuelResult.Fail(StageEntryFuelFailure.SaveFailed);

        userData.Resource = next;
        return StageEntryFuelResult.Success();
    }

    public async Task<bool> RefundAsync(int amount)
    {
        if (amount <= 0 || userData.Resource == null)
            return false;

        UserResourceData next = UserDataCloner.Copy(userData.Resource);
        try
        {
            next.Fuel = checked(next.Fuel + amount);
        }
        catch (OverflowException)
        {
            return false;
        }

        if (next.Fuel >= next.MaxFuel)
            next.LastFuelUpdateTime = Timestamp.GetCurrentTimestamp();

        if (!await TrySaveAsync(next))
            return false;

        userData.Resource = next;
        return true;
    }

    private async Task<bool> TrySaveAsync(UserResourceData resources)
    {
        try
        {
            await repository.SaveResourcesAsync(userId, resources);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
