using System;
using System.Threading.Tasks;

public enum TutorialProgressFailure
{
    None,
    InvalidState,
    UnexpectedStep,
    SaveFailed,
}

public sealed class TutorialProgressResult
{
    public bool Succeeded { get; }
    public TutorialProgressFailure Failure { get; }
    public UserTutorialData Tutorial { get; }

    private TutorialProgressResult(
        bool succeeded,
        TutorialProgressFailure failure,
        UserTutorialData tutorial)
    {
        Succeeded = succeeded;
        Failure = failure;
        Tutorial = tutorial;
    }

    public static TutorialProgressResult Success(UserTutorialData tutorial) =>
        new(true, TutorialProgressFailure.None, tutorial);

    public static TutorialProgressResult Fail(TutorialProgressFailure failure) =>
        new(false, failure, null);
}

public sealed class TutorialProgressUseCase
{
    private readonly IUserDataRepository repository;
    private readonly string userId;
    private readonly UserDataRoot userData;

    public TutorialProgressUseCase(
        IUserDataRepository repository,
        string userId,
        UserDataRoot userData)
    {
        this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
        this.userId = string.IsNullOrWhiteSpace(userId)
            ? throw new ArgumentException("User ID is required.", nameof(userId))
            : userId;
        this.userData = userData ?? throw new ArgumentNullException(nameof(userData));
    }

    public Task<TutorialProgressResult> AdvanceAsync(string expectedStepId)
    {
        UserTutorialData current = userData.Tutorial;
        if (!CanMutate(current))
            return Task.FromResult(TutorialProgressResult.Fail(TutorialProgressFailure.InvalidState));

        if (!string.Equals(current.NextStepId, expectedStepId, StringComparison.Ordinal) ||
            !FirstBattleTutorialSequence.TryGetNext(expectedStepId, out string nextStepId))
        {
            return Task.FromResult(TutorialProgressResult.Fail(TutorialProgressFailure.UnexpectedStep));
        }

        UserTutorialData next = UserDataCloner.Copy(current);
        next.NextStepId = nextStepId ?? string.Empty;
        return SaveAsync(next);
    }

    public Task<TutorialProgressResult> CompleteAsync()
    {
        UserTutorialData current = userData.Tutorial;
        if (!CanMutate(current))
        {
            return Task.FromResult(TutorialProgressResult.Fail(TutorialProgressFailure.InvalidState));
        }

        UserTutorialData next = UserDataCloner.Copy(current);
        next.NextStepId = string.Empty;
        next.Completed = true;
        return SaveAsync(next);
    }

    public Task<TutorialProgressResult> SkipAsync()
    {
        UserTutorialData current = userData.Tutorial;
        if (!CanMutate(current))
            return Task.FromResult(TutorialProgressResult.Fail(TutorialProgressFailure.InvalidState));

        UserTutorialData next = UserDataCloner.Copy(current);
        next.NextStepId = string.Empty;
        next.Skipped = true;
        return SaveAsync(next);
    }

    private static bool CanMutate(UserTutorialData tutorial) =>
        tutorial != null && tutorial.ShouldRun &&
        tutorial.Version == FirstBattleTutorialSequence.Version &&
        string.Equals(
            tutorial.SequenceId,
            UserTutorialData.FirstBattleSequenceId,
            StringComparison.Ordinal);

    private async Task<TutorialProgressResult> SaveAsync(UserTutorialData next)
    {
        try
        {
            await repository.SaveSectionsAsync(
                userId,
                new UserDataUpdate { Tutorial = next });
            userData.Tutorial = next;
            return TutorialProgressResult.Success(next);
        }
        catch (Exception)
        {
            return TutorialProgressResult.Fail(TutorialProgressFailure.SaveFailed);
        }
    }
}
