using System;
using System.Threading.Tasks;
using UnityEngine;

public class StageSessionController : MonoBehaviour
{
    [Header("Controllers")]
    [SerializeField] private StagePrepareTimerController prepareTimerController;
    [SerializeField] private WaveController waveController;
    [SerializeField] private StageRewardService rewardService;
    [SerializeField] private StagePreparationService preparationService;
    [SerializeField] private StageBootstrapper bootstrapper;
    [SerializeField] private StageUIController stageUI;
    [SerializeField] private MonsterSpawner monsterSpawner;
    [SerializeField] private MonsterPrewarmService monsterPrewarmService;
    [SerializeField] private StageTimeController stageTimeController;
    [SerializeField] private StageProgressService progressService;

    [Header("Runtime")]
    [SerializeField] private StageDataSO currentStageData;
    private StageEnterData enterData;
    private StageDataProvider stageDataProvider;
    private readonly StagePhaseMachine phaseMachine = new();
    private bool stageClearRewardGranted;
    private bool stageClearSavePending;
    private bool stageClearSaveInProgress;

    public StageState CurrentState => phaseMachine.Current;
    public StageDataSO CurrentStageData => currentStageData;
    public int CurrentWaveIndex { get; private set; }

    public event Action<StageState, StageState> PhaseChanged
    {
        add => phaseMachine.Changed += value;
        remove => phaseMachine.Changed -= value;
    }

    public WaveData CurrentWave =>
        currentStageData != null && CurrentWaveIndex < currentStageData.waves.Count
            ? currentStageData.waves[CurrentWaveIndex]
            : null;

    private void Start()
    {
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

        StartStage(stageData,enterData);
    }

    private void StartStage(StageDataSO stageData, StageEnterData enterData)
    {
        currentStageData = stageData;
        CurrentWaveIndex = 0;
        stageClearRewardGranted = false;
        stageClearSavePending = false;
        stageClearSaveInProgress = false;

        bootstrapper.InitializeStage(stageData,enterData);

        stageUI.Initialize();
        stageUI.RefreshWaveUI(CurrentWaveIndex);

        EnterPreparePhase();
    }
    public void EnterPreparePhase()
    {
        if (!TransitionTo(StageState.Preparing))
            return;

        stageUI.RefreshWaveUI(CurrentWaveIndex);

        monsterPrewarmService.PrewarmForWave(CurrentWave);
        monsterSpawner.WaveHpTracker.PrepareWave(CurrentWave);

        preparationService.EnterPrepareMode();
        stageTimeController.ExitCombatPhase();
        prepareTimerController.StartPreparePhase(OnPrepareFinished);
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

        preparationService.ExitPrepareMode();
        stageTimeController.EnterCombatPhase();
        waveController.StartWave(CurrentWave, OnWaveWin, OnWaveLose);
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
        stageTimeController.ExitCombatPhase();

        int clearedWaveCount = CurrentWaveIndex + 1;
        bool isLastWave = currentStageData == null || clearedWaveCount >= currentStageData.waves.Count;

        if (isLastWave)
        {
            await HandleStageClear();
            return;
        }

        bool saved = await progressService.RecordWaveClearAsync(currentStageData, clearedWaveCount);
        if (!saved)
            Debug.LogWarning($"Wave progress save failed: {currentStageData.StageKey}, cleared waves: {clearedWaveCount}");

        CurrentWaveIndex = clearedWaveCount;
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
        stageTimeController.ExitCombatPhase();
        if (!TransitionTo(StageState.StageFail))
            return;

        stageUI.ShowStageFail();

        bool saved = await progressService.ApplyStageFailAsync(currentStageData, CurrentWaveIndex);
        if (!saved)
            Debug.LogWarning($"Stage failure progress save failed: {currentStageData?.StageKey}");
    }

    private async Task HandleStageClear()
    {
        if (!stageClearRewardGranted)
        {
            rewardService.GiveStageClearReward(currentStageData);
            stageClearRewardGranted = true;
        }

        stageUI.ShowStageClear();
        await TrySaveStageClearAsync();
    }

    private async Task<bool> TrySaveStageClearAsync()
    {
        if (stageClearSaveInProgress)
            return false;

        stageClearSaveInProgress = true;
        stageClearSavePending = true;
        try
        {
            bool saved = await progressService.ApplyStageClearAsync(currentStageData);
            if (!saved)
            {
                Debug.LogWarning($"Stage clear progress save failed. Lobby exit is blocked until retry succeeds: {currentStageData?.StageKey}");
                return false;
            }

            stageClearSavePending = false;
            return CurrentState == StageState.StageClear || TransitionTo(StageState.StageClear);
        }
        finally
        {
            stageClearSaveInProgress = false;
        }
    }

    public async Task<bool> TryPrepareExitAsync()
    {
        if (stageClearSavePending && !await TrySaveStageClearAsync())
            return false;

        if (CurrentState != StageState.StageClear && CurrentState != StageState.StageFail)
            return false;

        return true;
    }

    private void StopCurrentPhase()
    {
        prepareTimerController.StopPreparePhase();
        waveController.StopWave();
        preparationService.ExitPrepareMode();
    }

    public void RequestStageFail()
    {
        stageTimeController.Resume();
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
