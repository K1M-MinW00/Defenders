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

    [Header("Runtime")]
    [SerializeField] private StageDataSO currentStageData;
    private StageEnterData enterData;
    private StageDataProvider stageDataProvider;
    private StageWaveSequence waveSequence;
    private readonly StagePhaseMachine phaseMachine = new();
    private bool outcomeSavePending;
    private bool pendingOutcomeIsClear;
    private Task<bool> outcomeSaveTask;

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
        if (!HasRequiredSceneReferences())
            return;

        if (!TransitionTo(StageState.Loading))
            return;

        if (!GameConfig.IsInitialized)
            GameConfig.Initialize();

        stageDataProvider = new StageDataProvider(GameConfig.Stages);
        enterData = StageEnterHolder.Consume();

        if(enterData == null)
        {
            Debug.LogError("StageEnterData is missing.");
            return;
        }

        StageDataSO stageData = stageDataProvider.Load(enterData.Sector, enterData.Stage);

        if(stageData == null)
        {
            Debug.LogError("StageData is missing");
            return;
        }

        if (!stageData.TryValidate(out string validationError))
        {
            Debug.LogError($"Stage data validation failed ({stageData.StageKey}): {validationError}");
            return;
        }

        StartStage(stageData,enterData);
    }

    private void StartStage(StageDataSO stageData, StageEnterData enterData)
    {
        currentStageData = stageData;
        waveSequence = new StageWaveSequence(stageData.waves);
        outcomeSavePending = false;
        pendingOutcomeIsClear = false;
        outcomeSaveTask = null;

        if (!bootstrapper.TryInitializeStage(stageData, enterData, out _, out string bootstrapError))
        {
            Debug.LogError($"Stage bootstrap failed ({stageData.StageKey}): {bootstrapError}");
            return;
        }

        stageUI.Initialize();
        stageUI.RefreshWaveUI(CurrentWaveIndex);

        EnterPreparePhase();
    }

    private bool HasRequiredSceneReferences()
    {
        bool valid = phaseRuntimeController != null && rewardService != null &&
                     bootstrapper != null && stageUI != null && progressService != null;

        if (!valid)
            Debug.LogError($"[{nameof(StageSessionController)}] Required scene references are missing.", this);

        return valid;
    }
    public void EnterPreparePhase()
    {
        if (!TransitionTo(StageState.Preparing))
            return;

        stageUI.RefreshWaveUI(CurrentWaveIndex);

        phaseRuntimeController.BeginPreparation(CurrentWave, OnPrepareFinished);
    }

    private void OnPrepareFinished()
    {
        EnterCombatPhase();
    }

    public void EnterCombatPhase()
    {
        if (!TransitionTo(StageState.Combat))
            return;

        stageUI.RefreshWaveUI(CurrentWaveIndex);

        phaseRuntimeController.BeginCombat(CurrentWave, OnWaveWin, OnWaveLose);
    }

    private void OnWaveWin()
    {
        _ = HandleWaveWinAsync();
    }

    private async Task HandleWaveWinAsync()
    {
        if (CurrentState != StageState.Combat)
            return;

        if (!TransitionTo(StageState.WaveCleared))
            return;

        GameAudioManager.Instance?.PlaySfx(GameAudioCue.WaveClear);
        rewardService.GiveWaveReward(CurrentWave);
        phaseRuntimeController.CompleteCombat();

        int clearedWaveCount = waveSequence.ClearedWaveCount;

        if (waveSequence.IsFinalWave)
        {
            await HandleStageClear();
            return;
        }

        bool saved = await progressService.RecordWaveClearAsync(currentStageData, clearedWaveCount);
        if (!saved)
            Debug.LogWarning($"Wave progress save failed: {currentStageData.StageKey}, cleared waves: {clearedWaveCount}");

        if (!waveSequence.TryMoveNext())
        {
            Debug.LogError("Failed to advance to the next wave.");
            return;
        }

        EnterPreparePhase();
    }

    private void OnWaveLose()
    {
        _ = HandleWaveLoseAsync(stopCurrentPhase: false);
    }

    private async Task HandleWaveLoseAsync(bool stopCurrentPhase)
    {
        bool canFail = CurrentState == StageState.Combat ||
                       (stopCurrentPhase && CurrentState == StageState.Preparing);

        if (!canFail)
            return;

        if (stopCurrentPhase)
            StopCurrentPhase();

        GameAudioManager.Instance?.PlaySfx(GameAudioCue.WaveFail);
        phaseRuntimeController.CompleteCombat();
        if (!TransitionTo(StageState.StageFail))
            return;

        stageUI.ShowStageFail(
            currentStageData,
            CurrentWaveIndex,
            currentStageData.failureRewards);
        await TrySaveStageOutcomeAsync(isClear: false);
    }

    private async Task HandleStageClear()
    {
        stageUI.ShowStageClear(
            currentStageData,
            waveSequence.ClearedWaveCount,
            currentStageData.clearRewards);
        await TrySaveStageOutcomeAsync(isClear: true);
    }

    private async Task<bool> TrySaveStageOutcomeAsync(bool isClear)
    {
        if (outcomeSaveTask != null)
            return await outcomeSaveTask;

        outcomeSavePending = true;
        pendingOutcomeIsClear = isClear;
        outcomeSaveTask = SaveStageOutcomeCoreAsync(isClear);

        try
        {
            return await outcomeSaveTask;
        }
        finally
        {
            outcomeSaveTask = null;
        }
    }

    private async Task<bool> SaveStageOutcomeCoreAsync(bool isClear)
    {
        StageOutcomeResult result = isClear
            ? await rewardService.GiveStageClearRewardAsync(currentStageData)
            : await rewardService.GiveStageFailRewardAsync(currentStageData, CurrentWaveIndex);

        if (!result.Succeeded)
        {
            Debug.LogWarning(
                $"Stage outcome save failed ({result.Failure}). " +
                $"Lobby exit is blocked until retry succeeds: {currentStageData?.StageKey}");
            return false;
        }

        outcomeSavePending = false;
        return !isClear || CurrentState == StageState.StageClear || TransitionTo(StageState.StageClear);
    }

    public async Task<bool> TryPrepareExitAsync()
    {
        if (outcomeSavePending && !await TrySaveStageOutcomeAsync(pendingOutcomeIsClear))
            return false;

        if (CurrentState != StageState.StageClear && CurrentState != StageState.StageFail)
            return false;

        return true;
    }

    private void StopCurrentPhase()
    {
        phaseRuntimeController.StopCurrentPhase();
    }

    public void RequestStageFail()
    {
        phaseRuntimeController.ResumeTime();
        _ = HandleWaveLoseAsync(stopCurrentPhase: true);
    }

    private bool TransitionTo(StageState next)
    {
        if (phaseMachine.TryTransition(next))
            return true;

        Debug.LogError($"Invalid stage phase transition: {CurrentState} -> {next}");
        return false;
    }
}
