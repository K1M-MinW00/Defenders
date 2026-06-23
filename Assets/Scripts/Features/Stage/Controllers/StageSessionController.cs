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
    [SerializeField] private StageUIController_ stageUI;
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
        rewardService.GiveWaveReward(CurrentWave);
        stageTimeController.ExitCombatPhase();
        CurrentWaveIndex++;

        if (CurrentWave == null)
        {
            HandleStageClear();
            return;
        }
        
        EnterPreparePhase();
        stageUI.RefreshWaveUI(CurrentWaveIndex);
    }

    private void OnWaveLose()
    {
        stageTimeController.ExitCombatPhase();
        CurrentState = StageState.StageFail;
        stageUI.SetPhase(CurrentState);
        stageUI.ShowStageFail();
    }

    private async Task HandleStageClear()
    {
        CurrentState = StageState.StageClear;
        rewardService.GiveStageClearReward(currentStageData);
        await progressService.ApplyStageClearAsync(currentStageData);

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
        if (CurrentState == StageState.StageFail || CurrentState == StageState.StageClear)
            return;

        StopCurrentPhase();

        CurrentState = StageState.StageFail;

        stageTimeController.Resume();
        stageTimeController.ExitCombatPhase();

        stageUI.SetPhase(CurrentState);
        stageUI.ShowStageFail();
    }
}