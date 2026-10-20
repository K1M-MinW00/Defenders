using UnityEngine;
public class StagePreparationService : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private UnitSummoner unitSummoner;
    [SerializeField] private UnitRoster unitRoster;
    [SerializeField] private UnitResetService unitResetService;
    [SerializeField] private PopulationManager populationManager;
    [SerializeField] private EconomyManager economyManager;
    [SerializeField] private PlacementController placementController;

    private bool isPrepareMode;
    private StageUnitTransactionService unitTransactions;
    private readonly StageRerollAllowance rerollAllowance = new();
    private StagePreparationAction allowedActions = StagePreparationAction.All;

    public bool IsPrepareMode => isPrepareMode;
    public bool HasAnyUnit => unitRoster != null && unitRoster.RegisteredCount > 0;
    public UnitRoster UnitRoster => unitRoster;
    public UnitSummoner UnitSummoner => unitSummoner;
    public EconomyManager EconomyManager => economyManager;
    public int FreeRerollsRemaining => rerollAllowance.Remaining;
    public StagePreparationAction AllowedActions => allowedActions;
    public bool CanStartWave => IsActionAllowed(StagePreparationAction.StartWave);
    public event System.Action<string> UnitSummoned;
    public event System.Action<string, int> UnitSold;
    public event System.Action<string> UnitRerolled;
    public event System.Action<string> UnitMoved;
    public event System.Action PopulationIncreased;
    public event System.Action<StagePreparationAction> AllowedActionsChanged;
    public event System.Action<int> OnFreeRerollsChanged
    {
        add => rerollAllowance.Changed += value;
        remove => rerollAllowance.Changed -= value;
    }

    public event System.Action OnUnitRosterChanged
    {
        add
        {
            if (unitRoster != null)
                unitRoster.OnRosterChanged += value;
        }
        remove
        {
            if (unitRoster != null)
                unitRoster.OnRosterChanged -= value;
        }
    }

    private void Awake()
    {
        unitTransactions = new StageUnitTransactionService(
            unitSummoner,
            unitRoster,
            populationManager,
            economyManager,
            rerollAllowance);

        if (!unitTransactions.IsConfigured)
            Debug.LogError($"[{nameof(StagePreparationService)}] Unit transaction dependencies are missing.", this);

        unitTransactions.UnitSummoned += HandleUnitSummoned;
        unitTransactions.UnitSold += HandleUnitSold;
        unitTransactions.UnitRerolled += HandleUnitRerolled;
    }

    private void OnEnable()
    {
        if (placementController != null)
        {
            placementController.OnSellRequested += HandleSellRequested;
            placementController.OnRerollRequested += HandleRerollRequested;
            placementController.OnUnitMoved += HandleUnitMoved;
        }
    }

    private void OnDisable()
    {
        SetAllowedActions(StagePreparationAction.All);
        ClosePreparationInput();

        if(placementController != null)
        {
            placementController.OnSellRequested -= HandleSellRequested;
            placementController.OnRerollRequested -= HandleRerollRequested;
            placementController.OnUnitMoved -= HandleUnitMoved;
        }
    }

    private void OnDestroy()
    {
        if (unitTransactions == null)
            return;

        unitTransactions.UnitSummoned -= HandleUnitSummoned;
        unitTransactions.UnitSold -= HandleUnitSold;
        unitTransactions.UnitRerolled -= HandleUnitRerolled;
    }

    public bool EnterPrepareMode()
    {
        if (isPrepareMode)
            return true;

        if (placementController == null || unitResetService == null || unitRoster == null)
        {
            Debug.LogError($"[{nameof(StagePreparationService)}] Preparation dependencies are missing.", this);
            return false;
        }

        if (!placementController.SetInputEnabled(true))
            return false;

        isPrepareMode = true;
        unitResetService.RestoreAll(unitRoster);
        return true;
    }

    public void ExitPrepareMode()
    {
        if (!isPrepareMode)
            return;

        ClosePreparationInput();
        unitResetService.CapturePreWavePositions(unitRoster);
        BeginUnitsCombat();
    }

    public void EndCurrentPhase()
    {
        ClosePreparationInput();

        if (unitRoster == null)
            return;

        foreach (UnitController unit in unitRoster.Units)
        {
            if (unit != null)
                unit.CompleteWave();
        }
    }

    public bool TrySummonUnit()
    {
        return TrySummonUnit(out _);
    }

    public bool TrySummonUnit(out StageUnitTransactionFailure failure)
    {
        if (!isPrepareMode || unitTransactions == null || !IsActionAllowed(StagePreparationAction.Summon))
        {
            failure = StageUnitTransactionFailure.NotAvailable;
            return false;
        }

        bool succeeded = unitTransactions.TrySummonUnit(out failure);
        if (succeeded)
            GameAudioManager.Instance?.PlaySfx(GameAudioCue.UnitSummon);

        return succeeded;
    }

    public bool TryIncreasePopulation()
    {
        return TryIncreasePopulation(out _);
    }

    public bool TryIncreasePopulation(out StageUnitTransactionFailure failure)
    {
        if (!isPrepareMode || unitTransactions == null ||
            !IsActionAllowed(StagePreparationAction.IncreasePopulation))
        {
            failure = StageUnitTransactionFailure.NotAvailable;
            return false;
        }

        bool succeeded = unitTransactions.TryIncreasePopulation(out failure);
        if (succeeded)
            PopulationIncreased?.Invoke();
        return succeeded;
    }

    private void HandleSellRequested(UnitController unit)
    {
        if (TrySellUnit(unit, out StageUnitTransactionFailure failure))
            return;

        placementController?.RestoreDraggingUnitPosition();
        if (failure == StageUnitTransactionFailure.LastUnitRequired)
            UIFeedbackToast.Show(LocalizationService.Get(
                LocalizationTables.Stage,
                "prepare.last_unit_required"));
    }

    private void HandleRerollRequested(UnitController unit)
    {
        Vector3 replacementPosition = placementController != null
            ? placementController.DragOrigin
            : unit.transform.position;

        if (!TryRerollUnit(unit, replacementPosition))
            placementController?.RestoreDraggingUnitPosition();
    }

    public bool TrySellUnit(UnitController unit)
    {
        return TrySellUnit(unit, out _);
    }

    public bool TrySellUnit(UnitController unit, out StageUnitTransactionFailure failure)
    {
        if (!isPrepareMode || unitTransactions == null || !IsActionAllowed(StagePreparationAction.Sell))
        {
            failure = StageUnitTransactionFailure.NotAvailable;
            return false;
        }

        return unitTransactions.TrySellUnit(unit, out failure);
    }

    public bool TryRerollUnit(UnitController unit, Vector3? replacementPosition = null)
    {
        bool succeeded = isPrepareMode && unitTransactions != null &&
                         IsActionAllowed(StagePreparationAction.Reroll) &&
                         unitTransactions.TryRerollUnit(unit, replacementPosition);
        if (succeeded)
            GameAudioManager.Instance?.PlaySfx(GameAudioCue.UnitSummon);

        return succeeded;
    }

    public void GrantFreeRerolls(int count)
    {
        rerollAllowance.Grant(count);
    }

    public void SetAllowedActions(StagePreparationAction actions)
    {
        if (allowedActions == actions)
            return;

        allowedActions = actions;
        AllowedActionsChanged?.Invoke(allowedActions);
    }

    public bool IsActionAllowed(StagePreparationAction action) =>
        (allowedActions & action) == action;

    public void CancelActiveInteraction()
    {
        placementController?.CancelActiveInteraction();
    }

    private void BeginUnitsCombat()
    {
        if (unitRoster == null)
            return;

        foreach (UnitController unit in unitRoster.Units)
        {
            if (unit == null)
                continue;

            unit.BeginCombat();
        }
    }

    private void ClosePreparationInput()
    {
        isPrepareMode = false;
        placementController?.SetInputEnabled(false);
    }

    private void HandleUnitSummoned(string unitId) => UnitSummoned?.Invoke(unitId);
    private void HandleUnitSold(string unitId, int star) => UnitSold?.Invoke(unitId, star);
    private void HandleUnitRerolled(string unitId) => UnitRerolled?.Invoke(unitId);
    private void HandleUnitMoved(UnitController unit) => UnitMoved?.Invoke(unit?.UnitId);
}
