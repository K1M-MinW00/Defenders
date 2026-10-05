using System.Collections.Generic;
using System.Threading.Tasks;
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
    [SerializeField] private StageMonsterInfoUI monsterInfoUI;
    [SerializeField] private StageAnnouncementUI announcementUI;

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
    private StageOverlayController overlayController;
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
        monsterInfoUI?.Initialize(session);
        waveTrackUI?.Initialize(session.CurrentStageData);

        timeController?.Initialize();

        topControlUI?.Initialize(timeController);
        pausePanelUI?.Initialize(
            timeController,
            session,
            preparationService?.UnitSummoner?.RuntimeCombatFormation);

        overlayController = new StageOverlayController(
            FindFirstObjectByType<GameCameraController>(),
            preparationService,
            phaseUIView,
            unitInfoPanel,
            monsterInfoUI,
            relicUI,
            timeController,
            session.CurrentState);

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
        overlayController?.Dispose();
        overlayController = null;
        monsterInfoUI?.Dispose();
        isInitialized = false;
    }

    public void SetPhase(StageState state)
    {
        monsterInfoUI?.Close();

        if (state != StageState.Preparing)
            HideUnitInfo();

        phaseUIView?.SetPhase(state);
        hudPresenter?.SetPhase(state);
        overlayController?.SetPhase(state);

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

        overlayController?.ShowUnitInfo(unit);
    }

    public void HideUnitInfo()
    {
        overlayController?.HideUnitInfo();
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

    public Task ShowStageIntroAsync(StageDataSO stage)
    {
        return announcementUI != null
            ? announcementUI.ShowStageIntroAsync(stage)
            : Task.CompletedTask;
    }

    public Task ShowBossWaveAsync()
    {
        return announcementUI != null
            ? announcementUI.ShowBossWaveAsync()
            : Task.CompletedTask;
    }

    public void SetRewardPresentationVisible(bool visible)
    {
        phaseUIView?.SetRewardPresentationVisible(visible);
    }
}
