using System.Collections.Generic;
using UnityEngine;

public class StageUIController : MonoBehaviour
{
    [Header("Sub UIs")]
    [SerializeField] private StagePhaseUIView phaseUIView;
    [SerializeField] private StageHudPresenter hudPresenter;
    [SerializeField] private StagePrepareActionUI prepareActionUI;
    [SerializeField] private UnitDragActionUI unitDragActionUI;
    [SerializeField] private StageWaveTrackUI waveTrackUI;
    [SerializeField] private StageHpSummaryUI hpSummaryUI;
    [SerializeField] private StageResultUI resultUI;
    [SerializeField] private StageRelicUI relicUI;
    [SerializeField] private StageUnitInfoPanel unitInfoPanel;

    [Header("Time UI")]
    [SerializeField] private StageTimeController timeController;
    [SerializeField] private StageTopControlUI topControlUI;
    [SerializeField] private StagePauseUI pausePanelUI;

    [Header("References")]
    [SerializeField] private StageSessionController session;
    [SerializeField] private StagePrepareTimerController flowController;
    [SerializeField] private EconomyManager economy;
    [SerializeField] private PopulationManager population;
    [SerializeField] private MonsterSpawner monsterSpawner;
    [SerializeField] private UnitRosterHpTracker unitHpTracker;
    [SerializeField] private MonsterWaveHpTracker monsterHpTracker;
    [SerializeField] private StagePreparationService preparationService;
    private bool isInitialized;
    public StageRelicUI RelicUI => relicUI;

    public void Initialize()
    {
        Dispose();

        if (session == null || flowController == null)
        {
            Debug.LogError("StageUIController Initialize failed.");
            return;
        }

        session.PhaseChanged -= HandlePhaseChanged;
        session.PhaseChanged += HandlePhaseChanged;

        hudPresenter?.Initialize(
            session.CurrentStageData,
            session.CurrentState,
            economy,
            population,
            flowController,
            monsterSpawner,
            preparationService
        );

        prepareActionUI?.Initialize(preparationService, flowController);
        unitDragActionUI?.Initialize(economy, preparationService);

        hpSummaryUI?.Initialize(monsterHpTracker, unitHpTracker);
        resultUI?.Initialize(session);
        relicUI?.Initialize();
        waveTrackUI?.Initialize(session.CurrentStageData);

        timeController?.Initialize();

        topControlUI?.Initialize(timeController);
        pausePanelUI?.Initialize(timeController, session);

        isInitialized = true;
        SetPhase(session.CurrentState);
    }

    private void OnDestroy()
    {
        Dispose();
    }

    public void Dispose()
    {
        if (!isInitialized)
            return;

        if (session != null)
            session.PhaseChanged -= HandlePhaseChanged;

        hudPresenter?.Dispose();
        prepareActionUI?.Dispose();
        hpSummaryUI?.Dispose();

        topControlUI?.Dispose();
        pausePanelUI?.Dispose();
        isInitialized = false;
    }

    public void SetPhase(StageState state)
    {
        if (state != StageState.Preparing)
            HideUnitInfo();

        phaseUIView?.SetPhase(state);
        hudPresenter?.SetPhase(state);

        bool isResult = state == StageState.StageClear || state == StageState.StageFail;
        relicUI?.SetOwnedHudVisible(!isResult);

        if (state == StageState.Preparing)
            SetUnitDragMode(false);

        if (isResult)
            return;

        if (monsterSpawner != null && session?.CurrentWave != null)
            hudPresenter?.RefreshMonsterCount(session.CurrentWave.TotalMonsterCount);
    }

    private void HandlePhaseChanged(StageState previous, StageState current)
    {
        SetPhase(current);
    }

    public void RefreshWaveUI(int currentWaveIndex)
    {
        if (session?.CurrentStageData == null)
            return;

        waveTrackUI?.Refresh(session.CurrentStageData.waves, currentWaveIndex);
    }

    public void SetUnitDragMode(bool isDraggingUnit, bool canReroll = true, int star = 1)
    {
        unitDragActionUI?.SetDragMode(isDraggingUnit, canReroll, star);
    }

    public void ShowUnitInfo(UnitController unit)
    {
        if (unit == null || session == null)
            return;

        phaseUIView?.SetTopHudHidden(true, session.CurrentState);
        unitInfoPanel?.Show(unit);
    }

    public void HideUnitInfo()
    {
        unitInfoPanel?.Hide();

        if (session != null)
            phaseUIView?.SetTopHudHidden(false, session.CurrentState);
    }

    public void ShowStageClear(
        StageDataSO stage,
        int clearedWaveCount,
        IReadOnlyList<RewardData> rewards)
    {
        resultUI?.ShowClear(stage, clearedWaveCount, rewards);
    }

    public void ShowStageFail(
        StageDataSO stage,
        int clearedWaveCount,
        IReadOnlyList<RewardData> rewards)
    {
        resultUI?.ShowFail(stage, clearedWaveCount, rewards);
    }

    public void HideAllResultPanels()
    {
        resultUI?.HideAll();
    }
}
