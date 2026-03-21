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

    public MonsterSpawner MonsterSpawner => waveController.MonsterSpawner;
    public PopulationManager PopulationManager => preparationService.PopulationManager;
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
        bootstrapper.Initialize(currentStageData);

        stageUI.Initialize(this);
        stageUI.RefreshWaveUI(currentStageData.waves, CurrentWaveIndex);
        // stageUI.CreateWaveUI(currentStageData.waves);

        EnterPreparePhase();
    }

    public void EnterPreparePhase()
    {
        CurrentState = StageState.Preparing;
        stageUI.SetPhase(CurrentState);

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
        // stageUI.UpdateCurrentWave(CurrentWaveIndex);
    }

    private void OnWaveWin()
    {
        rewardService.GiveWaveReward(CurrentWave);

        CurrentWaveIndex++;

        stageUI.RefreshWaveUI(currentStageData.waves, CurrentWaveIndex);

        if (CurrentWave == null)
            HandleStageClear();
        else
            EnterPreparePhase();
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

    public void RequestSellUnit(PlayerCharacter unit)
    {
        preparationService.TrySellUnit(unit);
    }

    public void RequestRerollUnit(PlayerCharacter unit)
    {
        preparationService.TryRerollUnit(unit);
    }
}