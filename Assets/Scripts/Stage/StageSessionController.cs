using UnityEngine;

public class StageSessionController : MonoBehaviour
{
    [Header("Runtime")]
    [SerializeField] private StageData currentStageData;

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

   
    public StageState CurrentState { get; private set; } = StageState.None;
    public StageData CurrentStageData => currentStageData;
    public int CurrentWaveIndex { get; private set; }

    public WaveData CurrentWave =>
        currentStageData != null && CurrentWaveIndex < currentStageData.waves.Count
            ? currentStageData.waves[CurrentWaveIndex]
            : null;

    private void Start()
    {
        // currentStageData = StageContext.SelectedStageData; // 로비에서 전달된 데이터
        bootstrapper.Initialize();

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

    private void HandleStageClear()
    {
        CurrentState = StageState.StageClear;
        rewardService.GiveStageClearReward(currentStageData);
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