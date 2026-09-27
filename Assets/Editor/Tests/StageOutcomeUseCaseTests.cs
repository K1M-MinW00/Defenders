using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;

public sealed class StageOutcomeUseCaseTests
{
    [Test]
    public async Task Complete_GrantsRewardsAndAdvancesProgressTogether()
    {
        UserDataRoot user = CreateUser();
        RecordingRepository repository = new();
        StageDataSO stage = CreateStage(new RewardData { Type = RewardType.Gold, Amount = 100 });

        try
        {
            StageOutcomeUseCase useCase = new(repository, "user", user);
            StageOutcomeResult result = await useCase.CompleteAsync(stage);

            Assert.That(result.Succeeded, Is.True);
            Assert.That(user.Resource.Gold, Is.EqualTo(100));
            Assert.That(user.Progress.CurrentStage, Is.EqualTo(2));
            Assert.That(user.Progress.BestWaveCleared, Is.Zero);
            Assert.That(repository.LastUpdate.Resources.Gold, Is.EqualTo(100));
            Assert.That(repository.LastUpdate.Progress.CurrentStage, Is.EqualTo(2));
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(stage);
        }
    }

    [Test]
    public async Task Complete_GrantsExperienceAndLevelsUpProfile()
    {
        GameConfig.Initialize();
        UserDataRoot user = CreateUser();
        user.Profile.Exp = 80;
        RecordingRepository repository = new();
        StageDataSO stage = CreateStage(new RewardData { Type = RewardType.Experience, Amount = 50 });

        try
        {
            StageOutcomeUseCase useCase = new(repository, "user", user);
            StageOutcomeResult result = await useCase.CompleteAsync(stage);

            Assert.That(result.Succeeded, Is.True);
            Assert.That(user.Profile.Level, Is.EqualTo(2));
            Assert.That(user.Profile.Exp, Is.EqualTo(30));
            Assert.That(repository.LastUpdate.Profile.Level, Is.EqualTo(2));
            Assert.That(repository.LastUpdate.Profile.Exp, Is.EqualTo(30));
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(stage);
        }
    }

    [Test]
    public async Task Fail_RefundsFuelAndRecordsClearedWaves()
    {
        UserDataRoot user = CreateUser();
        user.Resource.Fuel = 40;
        RecordingRepository repository = new();
        StageDataSO stage = CreateStage(new RewardData { Type = RewardType.Gold, Amount = 100 });
        RewardData fuel = new() { Type = RewardType.Fuel, Amount = 5 };

        try
        {
            StageOutcomeUseCase useCase = new(repository, "user", user);
            stage.failureRewards = new List<RewardData> { fuel };
            StageOutcomeResult result = await useCase.FailAsync(stage, 3);

            Assert.That(result.Succeeded, Is.True);
            Assert.That(user.Resource.Fuel, Is.EqualTo(45));
            Assert.That(user.Progress.CurrentStage, Is.EqualTo(1));
            Assert.That(user.Progress.BestWaveCleared, Is.EqualTo(3));
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(stage);
        }
    }

    [Test]
    public async Task SaveFailure_DoesNotMutateUserData()
    {
        UserDataRoot user = CreateUser();
        RecordingRepository repository = new() { ThrowOnSave = true };
        StageDataSO stage = CreateStage(new RewardData { Type = RewardType.Gold, Amount = 100 });

        try
        {
            StageOutcomeUseCase useCase = new(repository, "user", user);
            StageOutcomeResult result = await useCase.CompleteAsync(stage);

            Assert.That(result.Failure, Is.EqualTo(StageOutcomeFailure.SaveFailed));
            Assert.That(user.Resource.Gold, Is.Zero);
            Assert.That(user.Progress.CurrentStage, Is.EqualTo(1));
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(stage);
        }
    }

    private static UserDataRoot CreateUser()
    {
        return new UserDataRoot
        {
            Profile = new UserProfileData { Level = 1 },
            Resource = new UserResourceData { MaxFuel = 100 },
            Inventory = new UserInventoryData(),
            Roster = new UserRosterData(),
            Progress = new UserProgressData
            {
                CurrentSector = 1,
                CurrentStage = 1,
                BestWaveCleared = 0,
            },
        };
    }

    private static StageDataSO CreateStage(params RewardData[] rewards)
    {
        StageDataSO stage = ScriptableObject.CreateInstance<StageDataSO>();
        stage.sector = 1;
        stage.stage = 1;
        stage.clearRewards = new List<RewardData>(rewards);
        return stage;
    }

    private sealed class RecordingRepository : IUserDataRepository
    {
        public bool ThrowOnSave { get; set; }
        public UserDataUpdate LastUpdate { get; private set; }

        public Task SaveSectionsAsync(string userId, UserDataUpdate update)
        {
            if (ThrowOnSave)
                throw new InvalidOperationException("save failed");

            LastUpdate = update;
            return Task.CompletedTask;
        }

        public Task<UserDataLoadResult> LoadAsync(string userId) => throw new NotSupportedException();
        public Task CreateAsync(string userId, UserDataRoot data) => throw new NotSupportedException();
        public Task SaveAllAsync(string userId, UserDataRoot data) => throw new NotSupportedException();
        public Task SaveProfileAsync(string userId, UserProfileData profile) => throw new NotSupportedException();
        public Task SaveResourcesAsync(string userId, UserResourceData resources) => throw new NotSupportedException();
        public Task SaveProgressAsync(string userId, UserProgressData progress) => throw new NotSupportedException();
        public Task SaveRosterAsync(string userId, UserRosterData roster) => throw new NotSupportedException();
    }
}
