using UnityEngine;

public class StageUIController_ : MonoBehaviour
{
    [Header("Sub UIs")]
    [SerializeField] private StagePhaseUIView phaseUIView;
    [SerializeField] private StageHudPresenter hudPresenter;
    [SerializeField] private StagePrepareActionUI prepareActionUI;
    [SerializeField] private UnitDragActionUI unitDragActionUI;
    [SerializeField] private StageWaveTrackUI waveTrackUI;
    [SerializeField] private StageHpSummaryUI hpSummaryUI;
    [SerializeField] private StageResultUI resultUI;

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

    public void Initialize()
    {
        if (session == null || flowController == null)
        {
            Debug.LogError("StageUIController Initialize failed.");
            return;
        }

        phaseUIView?.SetPhase(session.CurrentState);

        hudPresenter?.Initialize(
            session.CurrentStageData,
            session.CurrentState,
            economy,
            population,
            flowController,
            monsterSpawner
        );

        prepareActionUI?.Initialize(preparationService, flowController);
        unitDragActionUI?.Initialize(economy);

        hpSummaryUI?.Initialize(monsterHpTracker, unitHpTracker);
        resultUI?.Initialize();
        waveTrackUI?.Initialize(session.CurrentStageData);

        timeController?.Initialize();

        topControlUI?.Initialize(timeController);
        pausePanelUI?.Initialize(timeController, session);

        SetPhase(session.CurrentState);
    }

    private void OnDestroy()
    {
        hudPresenter?.Dispose();
        prepareActionUI?.Dispose();

        topControlUI?.Dispose();
        pausePanelUI?.Dispose();
    }

    public void SetPhase(StageState state)
    {
        phaseUIView?.SetPhase(state);
        hudPresenter?.SetPhase(state);

        if (state == StageState.Preparing)
            SetUnitDragMode(false);

        if (state == StageState.StageClear || state == StageState.StageFail)
            return;

        if (monsterSpawner != null && session?.CurrentWave != null)
            hudPresenter?.RefreshMonsterCount(session.CurrentWave.TotalMonsterCount);
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

    public void ShowStageClear()
    {
        resultUI?.ShowClear();
    }

    public void ShowStageFail()
    {
        resultUI?.ShowFail();
    }

    public void HideAllResultPanels()
    {
        resultUI?.HideAll();
    }
}