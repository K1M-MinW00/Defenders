using System;
using System.Threading.Tasks;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class StageSessionController : MonoBehaviour
{
    [Header("Controllers")]
    [SerializeField] private StagePhaseRuntimeController phaseRuntimeController;
    [SerializeField] private StageRewardService rewardService;
    [SerializeField] private StageBootstrapper bootstrapper;
    [SerializeField] private StageUIController stageUI;
    [SerializeField] private StageProgressService progressService;
    [SerializeField] private StageRelicService relicService;

    [Header("Scene Flow")]
    [SerializeField] private string lobbySceneName = "LobbyScene";
    [SerializeField, Min(0f)] private float resultPresentationDelay = 2f;

    [Header("Runtime")]
    [SerializeField] private StageDataSO currentStageData;

    [Header("Tutorial")]
    [SerializeField] private StageDataSO tutorialStageData;
    private StageEnterData enterData;
    private StageDataProvider stageDataProvider;
    private StageWaveSequence waveSequence;
    private readonly StagePhaseMachine phaseMachine = new();
    private StageOutcomeCoordinator outcomeCoordinator;
    private IStageUserDataGateway userDataGateway;
    private SceneFlowService sceneFlowService;
    private GameSettingsManager settingsManager;
    private Task<bool> exitPreparationTask;
    private bool isHandlingInitializationFailure;
    private bool isDisposed;

    public StageState CurrentState => phaseMachine.Current;
    public StageDataSO CurrentStageData => currentStageData;
    public int CurrentWaveIndex => waveSequence?.CurrentIndex ?? 0;
    public bool IsTutorial => enterData?.IsTutorial == true;

    public event Action<StageState, StageState> PhaseChanged
    {
        add => phaseMachine.Changed += value;
        remove => phaseMachine.Changed -= value;
    }

    public WaveData CurrentWave => waveSequence?.Current;

    public void Initialize(
        IStageUserDataGateway userData,
        SceneFlowService sceneFlow,
        GameSettingsManager gameSettings)
    {
        userDataGateway = userData;
        sceneFlowService = sceneFlow;
        settingsManager = gameSettings;
    }

    private void Start()
    {
        _ = InitializeStageAsync();
    }

    private async Task InitializeStageAsync()
    {
        if (isDisposed)
            return;

        if (!TransitionTo(StageState.Loading))
            return;

        enterData = StageEnterHolder.Consume();
        try
        {
            if (!TryValidateSceneReferences(out string referenceError))
            {
                await HandleInitializationFailureAsync(referenceError);
                return;
            }

            if (enterData == null)
            {
                await HandleInitializationFailureAsync("Stage entry data is missing.");
                return;
            }

            if (!TryConfigureUserDataServices(out string userDataError))
            {
                await HandleInitializationFailureAsync(userDataError);
                return;
            }

            if (!GameConfig.IsInitialized)
                GameConfig.Initialize();

            stageDataProvider = new StageDataProvider(GameConfig.Stages);
            StageDataSO stageData = enterData.IsTutorial
                ? tutorialStageData
                : stageDataProvider.Load(enterData.Sector, enterData.Stage);

            if (stageData == null)
            {
                string missingData = enterData.IsTutorial
                    ? "Tutorial stage data is not assigned to the scene."
                    : $"Stage data is missing: {enterData.StageKey}";
                await HandleInitializationFailureAsync(missingData);
                return;
            }

            if (!stageData.TryValidate(out string validationError))
            {
                await HandleInitializationFailureAsync(
                    $"Stage data validation failed ({stageData.StageKey}): {validationError}");
                return;
            }

            if (!TryStartStage(stageData, enterData, out string startError))
            {
                await HandleInitializationFailureAsync(startError);
                return;
            }

            await BeginStageSequenceAsync();
        }
        catch (Exception exception)
        {
            await HandleInitializationFailureAsync(
                $"Unexpected stage initialization error: {exception.Message}",
                exception);
        }
    }

    private bool TryStartStage(StageDataSO stageData, StageEnterData enterData, out string error)
    {
        currentStageData = stageData;
        waveSequence = new StageWaveSequence(stageData.waves);
        outcomeCoordinator = new StageOutcomeCoordinator(rewardService);

        if (!phaseRuntimeController.TryConfigure(stageData, out string runtimeError))
        {
            error = $"Stage runtime configuration failed ({stageData.StageKey}): {runtimeError}";
            return false;
        }

        if (!bootstrapper.TryInitializeStage(stageData, enterData, out _, out string bootstrapError))
        {
            error = $"Stage bootstrap failed ({stageData.StageKey}): {bootstrapError}";
            return false;
        }

        if (!stageUI.TryInitialize(settingsManager, sceneFlowService, out string uiError))
        {
            error = $"Stage UI initialization failed ({stageData.StageKey}): {uiError}";
            return false;
        }

        if (!relicService.TryInitialize(
                phaseRuntimeController.PreparationService,
                phaseRuntimeController.MonsterSpawner,
                stageUI.RelicUI,
                out string relicError))
        {
            stageUI.Dispose();
            error = $"Stage relic initialization failed ({stageData.StageKey}): {relicError}";
            return false;
        }

        stageUI.RefreshWaveUI(CurrentWaveIndex);

        error = string.Empty;
        return CurrentState == StageState.Loading;
    }

    private async Task BeginStageSequenceAsync()
    {
        await stageUI.ShowStageIntroAsync(currentStageData);
        if (isDisposed)
            return;

        EnterPreparePhase();
    }

    private bool TryValidateSceneReferences(out string error)
    {
        bool valid = phaseRuntimeController != null && rewardService != null &&
                     bootstrapper != null && stageUI != null && progressService != null &&
                     relicService != null;

        error = valid ? string.Empty : "Required StageSessionController scene references are missing.";
        return valid;
    }

    private bool TryConfigureUserDataServices(out string error)
    {
        if (userDataGateway?.UserData == null || !userDataGateway.IsLoaded)
        {
            error = "Loaded user data is unavailable.";
            return false;
        }

        if (!rewardService.Configure(userDataGateway) ||
            !progressService.Configure(userDataGateway) ||
            !bootstrapper.Configure(
                userDataGateway.RosterService,
                userDataGateway.UserData.Inventory,
                LabBonusProvider.CreateSnapshot(userDataGateway.UserData, GameConfig.Lab)))
        {
            error = "Stage data services could not be configured.";
            return false;
        }

        error = string.Empty;
        return true;
    }

    private async Task HandleInitializationFailureAsync(string reason, Exception exception = null)
    {
        if (isDisposed || isHandlingInitializationFailure)
            return;

        isHandlingInitializationFailure = true;

        if (exception == null)
            Debug.LogError($"[{nameof(StageSessionController)}] {reason}", this);
        else
            Debug.LogException(exception, this);

        if (phaseMachine.CanTransitionTo(StageState.InitializationFailed))
            TransitionTo(StageState.InitializationFailed);

        int refundAmount = enterData?.EntryFuelCost ?? 0;
        bool refundSucceeded = refundAmount <= 0;
        if (refundAmount > 0)
        {
            refundSucceeded = userDataGateway != null &&
                              await userDataGateway.RefundStageEntryFuelAsync(refundAmount);
            if (!refundSucceeded)
                Debug.LogError($"[{nameof(StageSessionController)}] Failed to refund {refundAmount} entry fuel.", this);
        }

        if (isDisposed)
            return;

        if (sceneFlowService == null)
        {
            isHandlingInitializationFailure = false;
            Debug.LogError($"[{nameof(StageSessionController)}] Scene flow service is unavailable.", this);
            return;
        }

        SceneTransitionResult result = await sceneFlowService.LoadAsync(lobbySceneName);
        if (isDisposed)
            return;

        if (result == SceneTransitionResult.Succeeded)
        {
            string message = refundAmount > 0
                ? refundSucceeded
                    ? LocalizationService.Get(LocalizationTables.Stage, "initialization.refunded")
                    : LocalizationService.Get(LocalizationTables.Stage, "initialization.refund_failed")
                : LocalizationService.Get(LocalizationTables.Stage, "initialization.returned");
            UIFeedbackToast.Show(message);
            return;
        }

        if (result != SceneTransitionResult.Succeeded && result != SceneTransitionResult.AlreadyLoading && this != null)
        {
            isHandlingInitializationFailure = false;
            Debug.LogError(
                $"[{nameof(StageSessionController)}] Failed to return to lobby after initialization failure: {result}",
                this);
        }
    }
    public void EnterPreparePhase()
    {
        if (isDisposed)
            return;

        if (!TransitionTo(StageState.Preparing))
            return;

        stageUI.RefreshWaveUI(CurrentWaveIndex);

        phaseRuntimeController.BeginPreparation(
            CurrentWave,
            waitForFirstUnit: CurrentWaveIndex == 0,
            onFinished: OnPrepareFinished);
    }

    private void OnPrepareFinished()
    {
        if (isDisposed)
            return;

        EnterCombatPhase();
    }

    public void EnterCombatPhase()
    {
        if (isDisposed)
            return;

        phaseRuntimeController.CancelActiveUnitInteraction();

        if (!TransitionTo(StageState.Combat))
            return;

        stageUI.RefreshWaveUI(CurrentWaveIndex);

        phaseRuntimeController.BeginCombat(CurrentWave, OnWaveWin, OnWaveLose);
    }

    private void OnWaveWin()
    {
        if (isDisposed)
            return;

        _ = HandleWaveWinAsync();
    }

    private async Task HandleWaveWinAsync()
    {
        if (isDisposed || CurrentState != StageState.Combat)
            return;

        if (!TransitionTo(StageState.Resolving))
            return;

        GameAudioManager.Instance?.PlaySfx(GameAudioCue.WaveClear);
        WaveData clearedWave = CurrentWave;
        phaseRuntimeController.EndCurrentPhase();

        if (waveSequence.IsFinalWave)
        {
            await WaitForResultPresentationAsync();
            if (isDisposed)
                return;

            await HandleStageClear();
            return;
        }

        if (clearedWave.waveType == WaveType.Elite && relicService != null)
            await relicService.PresentChoiceAsync();

        if (isDisposed)
            return;

        stageUI.SetRewardPresentationVisible(true);
        try
        {
            await rewardService.GiveWaveRewardAsync(clearedWave);
        }
        finally
        {
            if (!isDisposed)
                stageUI.SetRewardPresentationVisible(false);
        }

        if (isDisposed)
            return;

        int clearedWaveCount = waveSequence.ClearedWaveCount;

        if (!IsTutorial)
        {
            bool saved = await progressService.RecordWaveClearAsync(currentStageData, clearedWaveCount);
            if (isDisposed)
                return;

            if (!saved)
                Debug.LogWarning($"Wave progress save failed: {currentStageData.StageKey}, cleared waves: {clearedWaveCount}");
        }

        if (!waveSequence.TryMoveNext())
        {
            Debug.LogError("Failed to advance to the next wave.");
            return;
        }

        if (CurrentWave.waveType == WaveType.Boss)
        {
            await stageUI.ShowBossWaveAsync();
            if (isDisposed)
                return;
        }

        EnterPreparePhase();
    }

    private void OnWaveLose()
    {
        if (isDisposed)
            return;

        _ = HandleWaveLoseAsync(allowPreparingFailure: false);
    }

    private async Task HandleWaveLoseAsync(bool allowPreparingFailure)
    {
        if (isDisposed)
            return;

        bool canFail = CurrentState == StageState.Combat ||
                       (allowPreparingFailure && CurrentState == StageState.Preparing);

        if (!canFail)
            return;

        if (IsTutorial)
        {
            phaseRuntimeController.EndCurrentPhase(preserveCombatObjects: true);
            if (!TransitionTo(StageState.Resolving))
                return;

            StageEnterHolder.Set(enterData);
            SceneTransitionResult restartResult = await sceneFlowService.LoadAsync(gameObject.scene.name);
            if (restartResult != SceneTransitionResult.Succeeded && this != null)
                Debug.LogError($"Tutorial restart failed: {restartResult}", this);
            return;
        }

        GameAudioManager.Instance?.PlaySfx(GameAudioCue.WaveFail);
        phaseRuntimeController.EndCurrentPhase(preserveCombatObjects: true);
        if (!TransitionTo(StageState.Resolving))
            return;

        await WaitForResultPresentationAsync();
        if (isDisposed || !TransitionTo(StageState.StageFail))
            return;

        stageUI.ShowStageResultSettling(false, currentStageData, CurrentWaveIndex);
        StageOutcomeResult result = await TrySaveStageOutcomeAsync(isClear: false);
        if (isDisposed)
            return;
        if (result.Succeeded)
            stageUI.ShowStageResult(false, currentStageData, CurrentWaveIndex, result.GrantedRewards);
        else
            stageUI.ShowStageResultSaveFailure();
    }

    private async Task HandleStageClear()
    {
        if (isDisposed || !TransitionTo(StageState.StageClear))
            return;

        int clearedWaveCount = waveSequence.ClearedWaveCount;
        stageUI.ShowStageResultSettling(true, currentStageData, clearedWaveCount);

        if (IsTutorial)
        {
            stageUI.ShowStageResult(
                true,
                currentStageData,
                clearedWaveCount,
                System.Array.Empty<RewardData>());
            return;
        }

        StageOutcomeResult result = await TrySaveStageOutcomeAsync(isClear: true);
        if (isDisposed)
            return;
        if (result.Succeeded)
            stageUI.ShowStageResult(true, currentStageData, clearedWaveCount, result.GrantedRewards);
        else
            stageUI.ShowStageResultSaveFailure();
    }

    private async Task WaitForResultPresentationAsync()
    {
        if (resultPresentationDelay <= 0f)
            return;

        await Task.Delay(TimeSpan.FromSeconds(resultPresentationDelay));
    }

    private async Task<StageOutcomeResult> TrySaveStageOutcomeAsync(bool isClear)
    {
        if (isDisposed)
            return StageOutcomeResult.Fail(StageOutcomeFailure.InvalidRequest);

        rewardService.SetQuestSessionStats(bootstrapper.SessionFusionCount, bootstrapper.SessionFourStarFusionCount);
        StageOutcomeResult result = await outcomeCoordinator.SaveAsync(
            currentStageData,
            isClear,
            isClear ? waveSequence.ClearedWaveCount : CurrentWaveIndex,
            enterData?.EntryFuelCost ?? 0);

        if (isDisposed)
            return StageOutcomeResult.Fail(StageOutcomeFailure.InvalidRequest);

        if (!result.Succeeded)
        {
            Debug.LogWarning(
                $"Stage outcome save failed ({result.Failure}). " +
                $"Lobby exit is blocked until retry succeeds: {currentStageData?.StageKey}");
            return result;
        }

        return result;
    }

    public Task<bool> TryPrepareExitAsync()
    {
        if (isDisposed)
            return Task.FromResult(false);

        if (exitPreparationTask != null)
            return exitPreparationTask;

        exitPreparationTask = PrepareExitCoreAsync();
        return exitPreparationTask;
    }

    private async Task<bool> PrepareExitCoreAsync()
    {
        // Ensure TryPrepareExitAsync stores the shared task before this method can clear it.
        await Task.Yield();

        try
        {
            if (outcomeCoordinator?.HasPendingOutcome == true)
            {
                bool isClear = outcomeCoordinator.PendingIsClear;
                StageOutcomeResult result = await outcomeCoordinator.RetryPendingAsync();
                if (isDisposed || !result.Succeeded)
                    return false;

                if (isClear && CurrentState != StageState.StageClear && !TransitionTo(StageState.StageClear))
                    return false;
            }

            return CurrentState == StageState.StageClear || CurrentState == StageState.StageFail;
        }
        finally
        {
            exitPreparationTask = null;
        }
    }

    public void RequestStageFail()
    {
        if (isDisposed || IsTutorial)
            return;

        _ = HandleWaveLoseAsync(allowPreparingFailure: true);
    }

    private bool TransitionTo(StageState next)
    {
        if (phaseMachine.TryTransition(next))
            return true;

        Debug.LogError($"Invalid stage phase transition: {CurrentState} -> {next}");
        return false;
    }

    private void OnDestroy()
    {
        if (isDisposed)
            return;

        isDisposed = true;
        phaseRuntimeController?.Shutdown();
        stageUI?.Dispose();
    }
}
