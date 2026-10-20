using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;

public sealed class TutorialProgressTests
{
    [SetUp]
    public void SetUp() => GameConfig.Initialize();

    [Test]
    public void NewUser_StartsFirstBattleTutorial()
    {
        UserDataRoot data = UserDataFactory.CreateDefault("tutorial-new-user");

        Assert.That(data.Tutorial.ShouldRun, Is.True);
        Assert.That(data.Tutorial.NextStepId, Is.EqualTo(FirstBattleTutorialSequence.SummonWave1Unit));
        Assert.That(data.SchemaVersion, Is.EqualTo(UserDataSchema.CurrentVersion));
    }

    [Test]
    public void Laboratory_UnlocksAtAccountLevelFive()
    {
        Assert.That(LobbyFeatureUnlockPolicy.IsUnlocked(4, LobbyFeatureUnlockPolicy.LaboratoryRequiredLevel), Is.False);
        Assert.That(LobbyFeatureUnlockPolicy.IsUnlocked(5, LobbyFeatureUnlockPolicy.LaboratoryRequiredLevel), Is.True);
    }

    [Test]
    public void TutorialClone_PreservesLobbyGuideCompletion()
    {
        UserTutorialData source = UserDataFactory.CreateDefaultTutorial(completed: true);
        source.LobbyUnitGuideCompleted = true;
        source.LaboratoryUnlockGuideCompleted = true;

        UserTutorialData clone = UserDataCloner.Copy(source);

        Assert.That(clone.LobbyUnitGuideCompleted, Is.True);
        Assert.That(clone.LaboratoryUnlockGuideCompleted, Is.True);
    }

    [Test]
    public void Sequence_SeparatesBattleAndLobbyRecoverySteps()
    {
        Assert.That(FirstBattleTutorialSequence.IsBattleStep(
            FirstBattleTutorialSequence.SummonWave1Unit), Is.True);
        Assert.That(FirstBattleTutorialSequence.IsBattleStep(
            FirstBattleTutorialSequence.FreePlay), Is.True);
        Assert.That(FirstBattleTutorialSequence.IsBattleStep(
            FirstBattleTutorialSequence.ClearBattle), Is.False);
        Assert.That(FirstBattleTutorialSequence.IsBattleStep(
            FirstBattleTutorialSequence.ReturnToLobby), Is.False);
    }

    [Test]
    public void Sequence_UsesRequestedSummonRerollAndFusionOrder()
    {
        Assert.That(FirstBattleTutorialSequence.Steps.ToArray(), Is.EqualTo(new[]
        {
            FirstBattleTutorialSequence.SummonWave1Unit,
            FirstBattleTutorialSequence.MoveWave1Unit,
            FirstBattleTutorialSequence.StartWave1,
            FirstBattleTutorialSequence.WaitWave1Clear,
            FirstBattleTutorialSequence.SummonWave2Unit,
            FirstBattleTutorialSequence.WaitWave2Fusion,
            FirstBattleTutorialSequence.StartWave2,
            FirstBattleTutorialSequence.WaitWave2Clear,
            FirstBattleTutorialSequence.SummonWave3Unit,
            FirstBattleTutorialSequence.SummonPostRelicB,
            FirstBattleTutorialSequence.RerollBToA,
            FirstBattleTutorialSequence.WaitThreeStarFusion,
            FirstBattleTutorialSequence.IncreasePopulation,
            FirstBattleTutorialSequence.StartWave3,
            FirstBattleTutorialSequence.WaitEliteClear,
            FirstBattleTutorialSequence.ChooseRelic,
            FirstBattleTutorialSequence.FreePlay,
            FirstBattleTutorialSequence.ClearBattle,
            FirstBattleTutorialSequence.ReturnToLobby,
            FirstBattleTutorialSequence.Finish,
        }));
    }

    [Test]
    public void Migration_DoesNotForceTutorialOnExistingUser()
    {
        UserDataRoot data = new()
        {
            SchemaVersion = 6,
            Tutorial = UserDataFactory.CreateDefaultTutorial(),
        };

        bool changed = UserDataMigrator.MigrateToCurrent(data, "existing-user");

        Assert.That(changed, Is.True);
        Assert.That(data.Tutorial.Completed, Is.True);
        Assert.That(data.Tutorial.ShouldRun, Is.False);
        Assert.That(data.Tutorial.NextStepId, Is.Empty);
    }

    [Test]
    public async Task Advance_RejectsOutOfOrderStep()
    {
        UserDataRoot data = CreateTutorialUser();
        RecordingRepository repository = new();
        TutorialProgressUseCase useCase = new(repository, "tutorial-user", data);

        TutorialProgressResult result =
            await useCase.AdvanceAsync(FirstBattleTutorialSequence.StartWave2);

        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.Failure, Is.EqualTo(TutorialProgressFailure.UnexpectedStep));
        Assert.That(repository.SaveCount, Is.Zero);
        Assert.That(data.Tutorial.NextStepId, Is.EqualTo(FirstBattleTutorialSequence.SummonWave1Unit));
    }

    [Test]
    public async Task Advance_CommitsNextStepOnlyAfterSave()
    {
        UserDataRoot data = CreateTutorialUser();
        RecordingRepository repository = new();
        TutorialProgressUseCase useCase = new(repository, "tutorial-user", data);

        TutorialProgressResult result =
            await useCase.AdvanceAsync(FirstBattleTutorialSequence.SummonWave1Unit);

        Assert.That(result.Succeeded, Is.True);
        Assert.That(repository.SaveCount, Is.EqualTo(1));
        Assert.That(repository.LastUpdate.Tutorial.NextStepId,
            Is.EqualTo(FirstBattleTutorialSequence.MoveWave1Unit));
        Assert.That(data.Tutorial.NextStepId,
            Is.EqualTo(FirstBattleTutorialSequence.MoveWave1Unit));
    }

    [Test]
    public async Task Advance_KeepsCurrentStepWhenSaveFails()
    {
        UserDataRoot data = CreateTutorialUser();
        RecordingRepository repository = new() { ThrowOnSave = true };
        TutorialProgressUseCase useCase = new(repository, "tutorial-user", data);

        TutorialProgressResult result =
            await useCase.AdvanceAsync(FirstBattleTutorialSequence.SummonWave1Unit);

        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.Failure, Is.EqualTo(TutorialProgressFailure.SaveFailed));
        Assert.That(data.Tutorial.NextStepId,
            Is.EqualTo(FirstBattleTutorialSequence.SummonWave1Unit));
    }

    [Test]
    public void RuntimeSession_AdvancesWithoutPersistenceAndRejectsWrongEvent()
    {
        FirstBattleTutorialSession session = new();

        Assert.That(session.TryAdvance(FirstBattleTutorialSequence.StartWave1), Is.False);
        Assert.That(session.CurrentStep, Is.EqualTo(FirstBattleTutorialSequence.SummonWave1Unit));

        Assert.That(session.TryAdvance(FirstBattleTutorialSequence.SummonWave1Unit), Is.True);
        Assert.That(session.CurrentStep, Is.EqualTo(FirstBattleTutorialSequence.MoveWave1Unit));
    }

    [Test]
    public async Task Complete_PersistsOnlyFinalCompletionFromInitialServerStep()
    {
        UserDataRoot data = CreateTutorialUser();
        RecordingRepository repository = new();
        TutorialProgressUseCase useCase = new(repository, "tutorial-user", data);

        TutorialProgressResult result = await useCase.CompleteAsync();

        Assert.That(result.Succeeded, Is.True);
        Assert.That(repository.SaveCount, Is.EqualTo(1));
        Assert.That(data.Tutorial.Completed, Is.True);
        Assert.That(data.Tutorial.NextStepId, Is.Empty);
    }

    private static UserDataRoot CreateTutorialUser() => new()
    {
        SchemaVersion = UserDataSchema.CurrentVersion,
        Tutorial = UserDataFactory.CreateDefaultTutorial(),
    };

    private sealed class RecordingRepository : IUserDataRepository
    {
        public int SaveCount { get; private set; }
        public UserDataUpdate LastUpdate { get; private set; }
        public bool ThrowOnSave { get; set; }

        public Task<UserDataLoadResult> LoadAsync(string userId) =>
            Task.FromResult(UserDataLoadResult.NotFound());

        public Task CreateAsync(string userId, UserDataRoot data) => Task.CompletedTask;
        public Task SaveAllAsync(string userId, UserDataRoot data) => Task.CompletedTask;
        public Task SaveProfileAsync(string userId, UserProfileData profile) => Task.CompletedTask;
        public Task SaveResourcesAsync(string userId, UserResourceData resources) => Task.CompletedTask;
        public Task SaveProgressAsync(string userId, UserProgressData progress) => Task.CompletedTask;
        public Task SaveRosterAsync(string userId, UserRosterData roster) => Task.CompletedTask;

        public Task SaveSectionsAsync(string userId, UserDataUpdate update)
        {
            if (ThrowOnSave)
                throw new InvalidOperationException("Expected test failure.");

            SaveCount++;
            LastUpdate = update;
            return Task.CompletedTask;
        }
    }
}
