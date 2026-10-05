using System;

/// <summary>
/// Owns the input/HUD policy shared by stage overlays. Individual views still
/// own their visuals; this class prevents each view from independently changing
/// camera, unit interaction, and top-HUD state.
/// </summary>
public sealed class StageOverlayController : IDisposable
{
    private readonly GameCameraController cameraController;
    private readonly StagePreparationService preparationService;
    private readonly StagePhaseUIView phaseView;
    private readonly StageUnitInfoPanel unitInfoPanel;
    private readonly StageMonsterInfoUI monsterInfoUI;
    private readonly StageRelicUI relicUI;
    private readonly StageTimeController timeController;

    private StageState stageState;
    private bool unitInfoOpen;
    private bool monsterInfoOpen;
    private bool relicChoiceOpen;
    private bool pauseOpen;
    private bool resultOpen;
    private bool worldInputBlocked;
    private bool disposed;

    public StageOverlayController(
        GameCameraController cameraController,
        StagePreparationService preparationService,
        StagePhaseUIView phaseView,
        StageUnitInfoPanel unitInfoPanel,
        StageMonsterInfoUI monsterInfoUI,
        StageRelicUI relicUI,
        StageTimeController timeController,
        StageState initialState)
    {
        this.cameraController = cameraController;
        this.preparationService = preparationService;
        this.phaseView = phaseView;
        this.unitInfoPanel = unitInfoPanel;
        this.monsterInfoUI = monsterInfoUI;
        this.relicUI = relicUI;
        this.timeController = timeController;
        stageState = initialState;

        if (monsterInfoUI != null)
            monsterInfoUI.VisibilityChanged += HandleMonsterInfoVisibilityChanged;
        if (relicUI != null)
            relicUI.ChoiceVisibilityChanged += HandleRelicChoiceVisibilityChanged;
        if (timeController != null)
            timeController.OnPauseChanged += HandlePauseChanged;

        ApplyPolicy();
    }

    public void SetPhase(StageState state)
    {
        stageState = state;
        bool isResult = state == StageState.StageClear || state == StageState.StageFail;

        if (state != StageState.Preparing && state != StageState.Combat)
            CloseUnitInfoVisual();

        if (state != StageState.Preparing && state != StageState.Combat)
            monsterInfoUI?.Close();

        resultOpen = isResult;
        ApplyPolicy();
    }

    public void ShowUnitInfo(UnitController unit)
    {
        if (disposed || unit == null || resultOpen || pauseOpen || relicChoiceOpen || monsterInfoOpen)
            return;

        unitInfoOpen = true;
        unitInfoPanel?.Show(unit);
        ApplyPolicy();
    }

    public void HideUnitInfo()
    {
        if (disposed)
            return;

        CloseUnitInfoVisual();
        ApplyPolicy();
    }

    public void Dispose()
    {
        if (disposed)
            return;

        disposed = true;
        if (monsterInfoUI != null)
            monsterInfoUI.VisibilityChanged -= HandleMonsterInfoVisibilityChanged;
        if (relicUI != null)
            relicUI.ChoiceVisibilityChanged -= HandleRelicChoiceVisibilityChanged;
        if (timeController != null)
            timeController.OnPauseChanged -= HandlePauseChanged;

        cameraController?.SetInputBlocked(false);
    }

    private void HandleMonsterInfoVisibilityChanged(bool visible)
    {
        monsterInfoOpen = visible;
        if (visible)
            CloseUnitInfoVisual();
        ApplyPolicy();
    }

    private void HandleRelicChoiceVisibilityChanged(bool visible)
    {
        relicChoiceOpen = visible;
        if (visible)
        {
            CloseUnitInfoVisual();
            monsterInfoUI?.Close();
        }
        ApplyPolicy();
    }

    private void HandlePauseChanged(bool paused)
    {
        pauseOpen = paused;
        if (paused)
        {
            CloseUnitInfoVisual();
            monsterInfoUI?.Close();
        }
        ApplyPolicy();
    }

    private void CloseUnitInfoVisual()
    {
        unitInfoOpen = false;
        unitInfoPanel?.Hide();
    }

    private void ApplyPolicy()
    {
        if (disposed)
            return;

        bool blocksWorldInput = monsterInfoOpen || relicChoiceOpen || pauseOpen || resultOpen;
        bool becameBlocked = blocksWorldInput && !worldInputBlocked;
        worldInputBlocked = blocksWorldInput;

        // Mark the policy as blocked before cancelling the interaction because
        // cancellation closes the unit panel and re-enters ApplyPolicy.
        if (becameBlocked)
            preparationService?.CancelActiveInteraction();

        cameraController?.SetInputBlocked(blocksWorldInput);
        phaseView?.SetTopHudHidden(unitInfoOpen, stageState);
    }
}
