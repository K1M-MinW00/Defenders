using System;
using System.Threading.Tasks;
using UnityEngine;

public class StageSessionController : MonoBehaviour
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
    private StageEnterData enterData;
    private StageDataProvider stageDataProvider;
    private StageWaveSequence waveSequence;
    private readonly StagePhaseMachine phaseMachine = new();
    private StageOutcomeCoordinator outcomeCoordinator;
    private bool isHandlingInitializationFailure;
    private bool isDisposed;

    public StageState CurrentState => phaseMachine.Current;
    public StageDataSO CurrentStageData => currentStageData;
    public int CurrentWaveIndex => waveSequence?.CurrentIndex ?? 0;

    public event Action<StageState, StageState> PhaseChanged
    {
        add => phaseMachine.Changed += value;
        remove => phaseMachine.Changed -= value;
    }

    public WaveData CurrentWave => waveSequence?.Current;

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

            if (!GameConfig.IsInitialized)
                GameConfig.Initialize();

            stageDataProvider = new StageDataProvider(GameConfig.Stages);
            StageDataSO stageData = stageDataProvider.Load(enterData.Sector, enterData.Stage);

            if (stageData == null)
            {
                await HandleInitializationFailureAsync($"Stage data is missing: {enterData.StageKey}");
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
        phaseRuntimeController.Configure(stageData);

        if (!bootstrapper.TryInitializeStage(stageData, enterData, out _, out string bootstrapError))
        {
            error = $"Stage bootstrap failed ({stageData.StageKey}): {bootstrapError}";
            return false;
        }

        stageUI.Initialize();
        relicService.Initialize(
            phaseRuntimeController.PreparationService,
            phaseRuntimeController.MonsterSpawner,
            stageUI.RelicUI);
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
            UserDataManager manager = UserDataManager.Instance;
            refundSucceeded = manager != null &&
                              await manager.RefundStageEntryFuelAsync(refundAmount);
            if (!refundSucceeded)
                Debug.LogError($"[{nameof(StageSessionController)}] Failed to refund {refundAmount} entry fuel.", this);
        }

        if (isDisposed)
            return;

        SceneTransitionResult result = await SceneFlowService.Shared.LoadAsync(lobbySceneName);
        if (isDisposed)
            return;

        if (result == SceneTransitionResult.Succeeded)
        {
            string message = refundAmount > 0
                ? refundSucceeded
                    ? "스테이지를 불러오지 못해 사용한 연료를 반환했습니다."
                    : "스테이지를 불러오지 못했으며 연료 반환에도 실패했습니다."
                : "스테이지를 불러오지 못해 로비로 돌아왔습니다.";
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

        bool saved = await progressService.RecordWaveClearAsync(currentStageData, clearedWaveCount);
        if (isDisposed)
            return;

        if (!saved)
            Debug.LogWarning($"Wave progress save failed: {currentStageData.StageKey}, cleared waves: {clearedWaveCount}");

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

        GameAudioManager.Instance?.PlaySfx(GameAudioCue.WaveFail);
        phaseRuntimeController.EndCurrentPhase(preserveCombatObjects: true);
        if (!TransitionTo(StageState.Resolving))
            return;

        await WaitForResultPresentationAsync();
        if (isDisposed || !TransitionTo(StageState.StageFail))
            return;

        stageUI.ShowStageFail(
            currentStageData,
            CurrentWaveIndex,
            currentStageData.failureRewards);
        await TrySaveStageOutcomeAsync(isClear: false);
    }

    private async Task HandleStageClear()
    {
        if (isDisposed || !TransitionTo(StageState.StageClear))
            return;

        stageUI.ShowStageClear(
            currentStageData,
            waveSequence.ClearedWaveCount,
            currentStageData.clearRewards);
        await TrySaveStageOutcomeAsync(isClear: true);
    }

    private async Task WaitForResultPresentationAsync()
    {
        if (resultPresentationDelay <= 0f)
            return;

        await Task.Delay(TimeSpan.FromSeconds(resultPresentationDelay));
    }

    private async Task<bool> TrySaveStageOutcomeAsync(bool isClear)
    {
        if (isDisposed)
            return false;

        StageOutcomeResult result = await outcomeCoordinator.SaveAsync(
            currentStageData,
            isClear,
            CurrentWaveIndex);

        if (isDisposed)
            return false;

        if (!result.Succeeded)
        {
            Debug.LogWarning(
                $"Stage outcome save failed ({result.Failure}). " +
                $"Lobby exit is blocked until retry succeeds: {currentStageData?.StageKey}");
            return false;
        }

        return !isClear || CurrentState == StageState.StageClear || TransitionTo(StageState.StageClear);
    }

    public async Task<bool> TryPrepareExitAsync()
    {
        if (isDisposed)
            return false;

        if (outcomeCoordinator?.HasPendingOutcome == true)
        {
            bool isClear = outcomeCoordinator.PendingIsClear;
            StageOutcomeResult result = await outcomeCoordinator.RetryPendingAsync();
            if (isDisposed || !result.Succeeded)
                return false;

            if (isClear && CurrentState != StageState.StageClear && !TransitionTo(StageState.StageClear))
                return false;
        }

        if (CurrentState != StageState.StageClear && CurrentState != StageState.StageFail)
            return false;

        return true;
    }

    public void RequestStageFail()
    {
        if (isDisposed)
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
