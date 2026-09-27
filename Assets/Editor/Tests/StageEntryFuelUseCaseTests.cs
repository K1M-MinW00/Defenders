using System;
using System.Threading.Tasks;
using NUnit.Framework;

public sealed class StageEntryFuelUseCaseTests
{
    [Test]
    public async Task ConsumeAndRefund_PersistsFuelChanges()
    {
        UserDataRoot user = new()
        {
            Resource = new UserResourceData { Fuel = 20, MaxFuel = 100 },
        };
        RecordingRepository repository = new();
        StageEntryFuelUseCase useCase = new(repository, "user", user);

        StageEntryFuelResult consumed = await useCase.ConsumeAsync(10);
        Assert.That(consumed.Succeeded, Is.True);
        Assert.That(user.Resource.Fuel, Is.EqualTo(10));

        bool refunded = await useCase.RefundAsync(10);
        Assert.That(refunded, Is.True);
        Assert.That(user.Resource.Fuel, Is.EqualTo(20));
        Assert.That(repository.SaveCount, Is.EqualTo(2));
    }

    [Test]
    public async Task Consume_WithInsufficientFuel_DoesNotSave()
    {
        UserDataRoot user = new()
        {
            Resource = new UserResourceData { Fuel = 9, MaxFuel = 100 },
        };
        RecordingRepository repository = new();
        StageEntryFuelUseCase useCase = new(repository, "user", user);

        StageEntryFuelResult result = await useCase.ConsumeAsync(10);

        Assert.That(result.Failure, Is.EqualTo(StageEntryFuelFailure.InsufficientFuel));
        Assert.That(user.Resource.Fuel, Is.EqualTo(9));
        Assert.That(repository.SaveCount, Is.Zero);
    }

    private sealed class RecordingRepository : IUserDataRepository
    {
        public int SaveCount { get; private set; }

        public Task SaveResourcesAsync(string userId, UserResourceData resources)
        {
            SaveCount++;
            return Task.CompletedTask;
        }

        public Task<UserDataLoadResult> LoadAsync(string userId) => throw new NotSupportedException();
        public Task CreateAsync(string userId, UserDataRoot data) => throw new NotSupportedException();
        public Task SaveAllAsync(string userId, UserDataRoot data) => throw new NotSupportedException();
        public Task SaveProfileAsync(string userId, UserProfileData profile) => throw new NotSupportedException();
        public Task SaveProgressAsync(string userId, UserProgressData progress) => throw new NotSupportedException();
        public Task SaveRosterAsync(string userId, UserRosterData roster) => throw new NotSupportedException();
        public Task SaveSectionsAsync(string userId, UserDataUpdate update) => throw new NotSupportedException();
    }
}
