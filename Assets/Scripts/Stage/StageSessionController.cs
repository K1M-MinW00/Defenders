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
    [SerializeField] private MonsterPrewarmService monsterPrewarmService;

    public MonsterSpawner MonsterSpawner => waveController.MonsterSpawner;
    public PopulationManager PopulationManager => preparationService.PopulationManager;
    public UnitRoster UnitRoster => preparationService.UnitRoster;
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

        stageUI.Initialize(this);
        stageUI.RefreshWaveUI(currentStageData.waves, CurrentWaveIndex);

        EnterPreparePhase();
    }

    public void EnterPreparePhase()
    {
        CurrentState = StageState.Preparing;
        stageUI.SetPhase(CurrentState);

        monsterPrewarmService.PrewarmForWave(CurrentWave);
        MonsterSpawner.WaveHpTracker.PrepareWave(CurrentWave);
        flowController.StartPreparePhase(OnPrepareFinished);
        
        preparationService.SetPrepareMode(true);
    }

    private void OnPrepareFinished()
    {
        EnterCombatPhase();
    }

    public void EnterCombatPhase()
    {
        if (CurrentWave == null)
        {
            HandleStageClear();
            return;
        }

        CurrentState = StageState.Combat;

        stageUI.SetPhase(CurrentState);
        stageUI.RefreshWaveUI(CurrentStageData.waves, CurrentWaveIndex);

        preparationService.SetPrepareMode(false);

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
        stageUI.RefreshWaveUI(currentStageData.waves, CurrentWaveIndex);
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

    public void StartBattleEarly()
    {
        if (CurrentState != StageState.Preparing)
            return;

        flowController.ForceFinishPrepare();
    }

    public void RequestSummonUnit()
    {
        preparationService.TrySummonUnit();
    }

    public void RequestIncreasePopulation()
    {
        preparationService.TryIncreasePopulation();
    }

    public void RequestSellUnit(UnitController unit)
    {
        preparationService.TrySellUnit(unit);
    }

    public void RequestRerollUnit(UnitController unit)
    {
        preparationService.TryRerollUnit(unit);
    }
}