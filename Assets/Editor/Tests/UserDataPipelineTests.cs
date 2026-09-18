using System;
using System.Threading.Tasks;
using Firebase.Firestore;
using NUnit.Framework;

public sealed class UserDataPipelineTests
{
    private const string UserId = "test-user";

    [Test]
    public async Task LoadOrCreateAsync_CreatesDefaultData_WhenUserDoesNotExist()
    {
        GameConfig.Initialize();
        FakeUserDataRepository repository = new()
        {
            LoadResult = UserDataLoadResult.NotFound(),
        };
        UserDataLoader loader = new(repository);

        UserDataRoot result = await loader.LoadOrCreateAsync(UserId);

        Assert.That(repository.CreateCallCount, Is.EqualTo(1));
        Assert.That(repository.CreatedData, Is.SameAs(result));
        Assert.That(result.Profile.UserId, Is.EqualTo(UserId));
        Assert.That(result.SchemaVersion, Is.EqualTo(UserDataSchema.CurrentVersion));
        Assert.That(result.Resource.Fuel, Is.EqualTo(result.Resource.MaxFuel));
    }

    [Test]
    public async Task LoadOrCreateAsync_DoesNotSave_WhenCurrentDataNeedsNoChanges()
    {
        UserDataRoot data = CreateValidData();
        FakeUserDataRepository repository = new()
        {
            LoadResult = UserDataLoadResult.Found(data),
        };
        UserDataLoader loader = new(repository);

        UserDataRoot result = await loader.LoadOrCreateAsync(UserId);

        Assert.That(result, Is.SameAs(data));
        Assert.That(repository.CreateCallCount, Is.Zero);
        Assert.That(repository.SaveAllCallCount, Is.Zero);
        Assert.That(repository.SaveResourcesCallCount, Is.Zero);
    }

    [Test]
    public async Task LoadOrCreateAsync_SavesAll_WhenSchemaMigrationRuns()
    {
        UserDataRoot data = CreateValidData();
        data.SchemaVersion = 0;
        FakeUserDataRepository repository = new()
        {
            LoadResult = UserDataLoadResult.Found(data),
        };
        UserDataLoader loader = new(repository);

        UserDataRoot result = await loader.LoadOrCreateAsync(UserId);

        Assert.That(result.SchemaVersion, Is.EqualTo(UserDataSchema.CurrentVersion));
        Assert.That(repository.SaveAllCallCount, Is.EqualTo(1));
        Assert.That(repository.SaveResourcesCallCount, Is.Zero);
    }

    [Test]
    public async Task LoadOrCreateAsync_SavesOnlyResources_WhenFuelRecovers()
    {
        UserDataRoot data = CreateValidData();
        data.Resource.Fuel = 10;
        data.Resource.LastFuelUpdateTime = Timestamp.FromDateTime(DateTime.UtcNow.AddMinutes(-10));
        FakeUserDataRepository repository = new()
        {
            LoadResult = UserDataLoadResult.Found(data),
        };
        UserDataLoader loader = new(repository);

        await loader.LoadOrCreateAsync(UserId);

        Assert.That(data.Resource.Fuel, Is.GreaterThan(10));
        Assert.That(repository.SaveResourcesCallCount, Is.EqualTo(1));
        Assert.That(repository.SaveAllCallCount, Is.Zero);
    }

    [Test]
    public async Task ProfileUpdate_CommitsCopy_AfterSaveSucceeds()
    {
        UserDataRoot data = CreateValidData();
        UserProfileData originalProfile = data.Profile;
        FakeUserDataRepository repository = new();
        ProfileUpdateUseCase useCase = new(repository, UserId, data);

        bool succeeded = await useCase.UpdateNicknameAsync("NewName");

        Assert.That(succeeded, Is.True);
        Assert.That(repository.SaveProfileCallCount, Is.EqualTo(1));
        Assert.That(data.Profile, Is.Not.SameAs(originalProfile));
        Assert.That(data.Profile.Nickname, Is.EqualTo("NewName"));
        Assert.That(originalProfile.Nickname, Is.EqualTo("OldName"));
    }

    [Test]
    public async Task ProfileUpdate_KeepsOriginalData_WhenSaveFails()
    {
        UserDataRoot data = CreateValidData();
        UserProfileData originalProfile = data.Profile;
        FakeUserDataRepository repository = new()
        {
            ThrowOnSaveProfile = true,
        };
        ProfileUpdateUseCase useCase = new(repository, UserId, data);

        bool succeeded = await useCase.UpdateNicknameAsync("NewName");

        Assert.That(succeeded, Is.False);
        Assert.That(data.Profile, Is.SameAs(originalProfile));
        Assert.That(data.Profile.Nickname, Is.EqualTo("OldName"));
    }

    private static UserDataRoot CreateValidData()
    {
        return new UserDataRoot
        {
            SchemaVersion = UserDataSchema.CurrentVersion,
            Profile = new UserProfileData
            {
                UserId = UserId,
                Nickname = "OldName",
                Level = 1,
                Exp = 0,
                IconId = "unit_knight",
            },
            Resource = new UserResourceData
            {
                Gold = 100,
                Gem = 10,
                Fuel = 100,
                MaxFuel = 100,
                LastFuelUpdateTime = Timestamp.GetCurrentTimestamp(),
            },
            Roster = new UserRosterData(),
            Progress = new UserProgressData(),
            Inventory = new UserInventoryData(),
            Gacha = new UserGachaData(),
            Ad = new UserAdData(),
        };
    }

    private sealed class FakeUserDataRepository : IUserDataRepository
    {
        public UserDataLoadResult LoadResult { get; set; } = UserDataLoadResult.NotFound();
        public bool ThrowOnSaveProfile { get; set; }
        public int CreateCallCount { get; private set; }
        public int SaveAllCallCount { get; private set; }
        public int SaveResourcesCallCount { get; private set; }
        public int SaveProfileCallCount { get; private set; }
        public UserDataRoot CreatedData { get; private set; }

        public Task<UserDataLoadResult> LoadAsync(string userId) => Task.FromResult(LoadResult);

        public Task CreateAsync(string userId, UserDataRoot data)
        {
            CreateCallCount++;
            CreatedData = data;
            return Task.CompletedTask;
        }

        public Task SaveAllAsync(string userId, UserDataRoot data)
        {
            SaveAllCallCount++;
            return Task.CompletedTask;
        }

        public Task SaveProfileAsync(string userId, UserProfileData profile)
        {
            SaveProfileCallCount++;

            if (ThrowOnSaveProfile)
                throw new InvalidOperationException("Simulated save failure.");

            return Task.CompletedTask;
        }

        public Task SaveResourcesAsync(string userId, UserResourceData resources)
        {
            SaveResourcesCallCount++;
            return Task.CompletedTask;
        }

        public Task SaveProgressAsync(string userId, UserProgressData progress) => Task.CompletedTask;
        public Task SaveRosterAsync(string userId, UserRosterData roster) => Task.CompletedTask;
        public Task SaveInventoryAsync(string userId, UserInventoryData inventory) => Task.CompletedTask;
        public Task SaveGachaAsync(string userId, UserGachaData gacha) => Task.CompletedTask;
        public Task SaveAdAsync(string userId, UserAdData ad) => Task.CompletedTask;
        public Task SaveSectionsAsync(string userId, UserDataUpdate update) => Task.CompletedTask;
    }
}
