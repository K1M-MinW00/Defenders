using UnityEngine;

public class StageSessionController : MonoBehaviour
{
    [Header("Runtime")]
    [SerializeField] private StageData currentStageData;

    [Header("Controllers")]
    [SerializeField] private StageFlowController flowController;
    [SerializeField] private WaveController waveController;
    [SerializeField] private StageRewardService rewardService;
    [SerializeField] private StagePreparationService preparationService;
    [SerializeField] private StageBootstrapper bootstrapper;
    [SerializeField] private StageUIController stageUI;
    [SerializeField] private MonsterSpawner monsterSpawner;
    [SerializeField] private MonsterPrewarmService monsterPrewarmService;

    // [SerializeField] private EconomyManager economyManager;

    //public UnitRosterHpTracker UnitRosterHpTracker => unitRosterHpTracker;
    //public MonsterSpawner MonsterSpawner => waveController.MonsterSpawner;
    //public EconomyManager EconomyManager => economyManager;
    //public PopulationManager PopulationManager => preparationService.PopulationManager;

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
        flowController.StartPreparePhase(OnPrepareFinished);
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
        waveController.StartWave(CurrentWave, OnWaveWin, OnWaveLose);
    }

    private void OnWaveWin()
    {
        rewardService.GiveWaveReward(CurrentWave);

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
}