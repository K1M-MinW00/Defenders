using System;
using System.Threading.Tasks;
using Firebase.Firestore;
using NUnit.Framework;
using UnityEngine;

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

        NicknameChangeResult result = await useCase.UpdateNicknameAsync("NewName");

        Assert.That(result.Succeeded, Is.True);
        Assert.That(result.GemCost, Is.Zero);
        Assert.That(repository.SaveSectionsCallCount, Is.EqualTo(1));
        Assert.That(data.Profile, Is.Not.SameAs(originalProfile));
        Assert.That(data.Profile.Nickname, Is.EqualTo("NewName"));
        Assert.That(data.Profile.HasUsedFreeNicknameChange, Is.True);
        Assert.That(originalProfile.Nickname, Is.EqualTo("OldName"));
    }

    [Test]
    public async Task ProfileUpdate_KeepsOriginalData_WhenSaveFails()
    {
        UserDataRoot data = CreateValidData();
        UserProfileData originalProfile = data.Profile;
        FakeUserDataRepository repository = new()
        {
            ThrowOnSaveSections = true,
        };
        ProfileUpdateUseCase useCase = new(repository, UserId, data);

        NicknameChangeResult result = await useCase.UpdateNicknameAsync("NewName");

        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.Failure, Is.EqualTo(NicknameChangeFailure.SaveFailed));
        Assert.That(data.Profile, Is.SameAs(originalProfile));
        Assert.That(data.Profile.Nickname, Is.EqualTo("OldName"));
    }

    [Test]
    public async Task NicknameUpdate_ChargesGem_AfterFreeChange()
    {
        UserDataRoot data = CreateValidData();
        data.Profile.HasUsedFreeNicknameChange = true;
        data.Resource.Gem = 600;
        FakeUserDataRepository repository = new();
        ProfileUpdateUseCase useCase = new(repository, UserId, data, 500);

        NicknameChangeResult result = await useCase.UpdateNicknameAsync("PaidName");

        Assert.That(result.Succeeded, Is.True);
        Assert.That(result.GemCost, Is.EqualTo(500));
        Assert.That(data.Resource.Gem, Is.EqualTo(100));
        Assert.That(data.Profile.Nickname, Is.EqualTo("PaidName"));
        Assert.That(repository.SaveSectionsCallCount, Is.EqualTo(1));
    }

    [Test]
    public async Task NicknameUpdate_RejectsInsufficientGem()
    {
        UserDataRoot data = CreateValidData();
        data.Profile.HasUsedFreeNicknameChange = true;
        data.Resource.Gem = 499;
        FakeUserDataRepository repository = new();
        ProfileUpdateUseCase useCase = new(repository, UserId, data, 500);

        NicknameChangeResult result = await useCase.UpdateNicknameAsync("PaidName");

        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.Failure, Is.EqualTo(NicknameChangeFailure.InsufficientGem));
        Assert.That(data.Resource.Gem, Is.EqualTo(499));
        Assert.That(data.Profile.Nickname, Is.EqualTo("OldName"));
        Assert.That(repository.SaveSectionsCallCount, Is.Zero);
    }

    [Test]
    public void Normalize_AssignsDefaultProfileIcon_WhenMissing()
    {
        UserDataRoot data = CreateValidData();
        data.Profile.IconId = null;
        data.Roster.OwnedUnits.Add(new UserUnitData { UnitId = "unit_knight", Level = 1 });

        bool changed = UserDataNormalizer.Normalize(data, UserId);

        Assert.That(changed, Is.True);
        Assert.That(data.Profile.IconId, Is.EqualTo("unit_knight"));
    }

    [Test]
    public async Task ProfileUpdate_RejectsUnownedIcon()
    {
        UserDataRoot data = CreateValidData();
        data.Roster.OwnedUnits.Add(new UserUnitData { UnitId = "unit_knight", Level = 1 });
        FakeUserDataRepository repository = new();
        ProfileUpdateUseCase useCase = new(repository, UserId, data);

        bool succeeded = await useCase.UpdateIconAsync("unit_wizard");

        Assert.That(succeeded, Is.False);
        Assert.That(repository.SaveProfileCallCount, Is.Zero);
        Assert.That(data.Profile.IconId, Is.EqualTo("unit_knight"));
    }

    [Test]
    public void LobbyBattlePresenter_BuildsStateAndStageEntry()
    {
        UserDataRoot data = CreateValidData();
        data.Profile.Exp = 50;
        data.Roster.OwnedUnits.Add(new UserUnitData { UnitId = "unit_knight", Level = 1 });
        data.Roster.SelectedUnitIds.Add("unit_knight");
        UserLevelProgressionSO progression = Resources.Load<UserLevelProgressionSO>("Configs/UserLevelProgression");
        LobbyBattlePresenter presenter = new(data, progression);

        LobbyBattleViewState state = presenter.Build();
        bool succeeded = presenter.TryBuildStageEnterData(out StageEnterData enterData, out LobbyBattleStartFailure failure);

        Assert.That(state, Is.Not.Null);
        Assert.That(state.NormalizedExp, Is.EqualTo(0.5f).Within(0.001f));
        Assert.That(state.CanStartBattle, Is.True);
        Assert.That(succeeded, Is.True);
        Assert.That(failure, Is.EqualTo(LobbyBattleStartFailure.None));
        Assert.That(enterData.SelectedUnitIds, Is.EqualTo(new[] { "unit_knight" }));
    }

    [Test]
    public void LobbyBattlePresenter_RejectsInvalidFormation()
    {
        UserDataRoot data = CreateValidData();
        data.Roster.OwnedUnits.Add(new UserUnitData { UnitId = "unit_knight", Level = 1 });
        data.Roster.SelectedUnitIds.Add("unit_wizard");
        LobbyBattlePresenter presenter = new(data, null);

        bool succeeded = presenter.TryBuildStageEnterData(out StageEnterData enterData, out LobbyBattleStartFailure failure);

        Assert.That(succeeded, Is.False);
        Assert.That(enterData, Is.Null);
        Assert.That(failure, Is.EqualTo(LobbyBattleStartFailure.InvalidFormation));
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
        public bool ThrowOnSaveSections { get; set; }
        public int CreateCallCount { get; private set; }
        public int SaveAllCallCount { get; private set; }
        public int SaveResourcesCallCount { get; private set; }
        public int SaveProfileCallCount { get; private set; }
        public int SaveSectionsCallCount { get; private set; }
        public UserDataRoot CreatedData { get; private set; }

        public Task<UserDataLoadResult> LoadAsync(string userId) => Task.FromResult(LoadResult);

        public Task CreateAsync(string userId, UserDataRoot data)
        {
            CreateCallCount++;
            CreatedData = data;
            LoadResult = UserDataLoadResult.Found(data);
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
        public Task SaveSectionsAsync(string userId, UserDataUpdate update)
        {
            SaveSectionsCallCount++;

            if (ThrowOnSaveSections)
                throw new InvalidOperationException("Simulated multi-section save failure.");

            return Task.CompletedTask;
        }
    }
}
