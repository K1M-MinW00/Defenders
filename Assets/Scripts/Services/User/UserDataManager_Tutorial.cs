using System.Threading.Tasks;

public partial class UserDataManager
{
    public bool ShouldRunFirstBattleTutorial =>
        UserData?.Tutorial?.ShouldRun == true;

    public Task<TutorialProgressResult> AdvanceTutorialAsync(string expectedStepId) =>
        RunTutorialMutationAsync(() => TutorialProgressUseCase.AdvanceAsync(expectedStepId));

    public Task<TutorialProgressResult> CompleteTutorialAsync() =>
        RunTutorialMutationAsync(TutorialProgressUseCase.CompleteAsync);

    public Task<TutorialProgressResult> SkipTutorialAsync() =>
        RunTutorialMutationAsync(TutorialProgressUseCase.SkipAsync);

    public async Task<bool> CompleteLobbyUnitGuideAsync()
    {
        if (!IsLoaded || repository == null || UserData?.Tutorial == null)
            return false;

        return await RunSerializedMutationAsync(async () =>
        {
            UserTutorialData next = UserDataCloner.Copy(UserData.Tutorial);
            next.LobbyUnitGuideCompleted = true;
            try
            {
                await repository.SaveSectionsAsync(CurrentUserId, new UserDataUpdate { Tutorial = next });
                UserData.Tutorial = next;
                OnTutorialUpdated?.Invoke();
                return true;
            }
            catch (System.Exception exception)
            {
                UnityEngine.Debug.LogError($"[UserDataManager] Lobby unit guide save failed: {exception}");
                return false;
            }
        });
    }

    public async Task<bool> CompleteLaboratoryUnlockGuideAsync()
    {
        if (!IsLoaded || repository == null || UserData?.Tutorial == null)
            return false;

        return await RunSerializedMutationAsync(async () =>
        {
            UserTutorialData next = UserDataCloner.Copy(UserData.Tutorial);
            next.LaboratoryUnlockGuideCompleted = true;
            try
            {
                await repository.SaveSectionsAsync(CurrentUserId, new UserDataUpdate { Tutorial = next });
                UserData.Tutorial = next;
                OnTutorialUpdated?.Invoke();
                return true;
            }
            catch (System.Exception exception)
            {
                UnityEngine.Debug.LogError($"[UserDataManager] Laboratory unlock guide save failed: {exception}");
                return false;
            }
        });
    }

    private async Task<TutorialProgressResult> RunTutorialMutationAsync(
        System.Func<Task<TutorialProgressResult>> operation)
    {
        if (!IsLoaded || TutorialProgressUseCase == null || operation == null)
            return TutorialProgressResult.Fail(TutorialProgressFailure.InvalidState);

        TutorialProgressResult result = await RunSerializedMutationAsync(operation);
        if (result.Succeeded)
            OnTutorialUpdated?.Invoke();

        return result;
    }

#if UNITY_EDITOR
    public async Task<bool> SetTutorialStepForDebugAsync(string stepId)
    {
        if (!IsLoaded || repository == null || !FirstBattleTutorialSequence.Contains(stepId))
            return false;

        UserTutorialData next = UserDataFactory.CreateDefaultTutorial();
        next.NextStepId = stepId;

        try
        {
            await RunSerializedMutationAsync(async () =>
            {
                await repository.SaveSectionsAsync(
                    CurrentUserId,
                    new UserDataUpdate { Tutorial = next });
                UserData.Tutorial = next;
                return true;
            });
            OnTutorialUpdated?.Invoke();
            return true;
        }
        catch (System.Exception exception)
        {
            UnityEngine.Debug.LogError($"[UserDataManager] Tutorial debug reset failed: {exception}");
            return false;
        }
    }
#endif
}
