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
    private readonly StageDataProvider stageDataProvider = new();


    public StageState CurrentState { get; private set; } = StageState.None;
    public StageDataSO CurrentStageData => currentStageData;
    public int CurrentWaveIndex { get; private set; }

    public WaveData CurrentWave =>
        currentStageData != null && CurrentWaveIndex < currentStageData.waves.Count
            ? currentStageData.waves[CurrentWaveIndex]
            : null;

    private void Start()
    {
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
        CurrentState = StageState.None;

        bootstrapper.InitializeStage(stageData,enterData);

        stageUI.Initialize();
        stageUI.RefreshWaveUI(CurrentWaveIndex);

        EnterPreparePhase();
    }
    public void EnterPreparePhase()
    {
        CurrentState = StageState.Preparing;

        stageUI.SetPhase(CurrentState);
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
        CurrentState = StageState.Combat;
        stageUI.SetPhase(CurrentState);
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

        CurrentState = StageState.Reward;
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
        CurrentState = StageState.StageFail;
        stageUI.SetPhase(CurrentState);
        stageUI.ShowStageFail();

        bool saved = await progressService.ApplyStageFailAsync(currentStageData, CurrentWaveIndex);
        if (!saved)
            Debug.LogWarning($"Stage failure progress save failed: {currentStageData?.StageKey}");
    }

    private async Task HandleStageClear()
    {
        CurrentState = StageState.StageClear;
        rewardService.GiveStageClearReward(currentStageData);
        bool saved = await progressService.ApplyStageClearAsync(currentStageData);
        if (!saved)
            Debug.LogWarning($"Stage clear progress save failed: {currentStageData?.StageKey}");

        stageUI.SetPhase(CurrentState);
        stageUI.ShowStageClear();
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
}
