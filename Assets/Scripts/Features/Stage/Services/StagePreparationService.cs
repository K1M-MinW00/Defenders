using UnityEngine;
public class StagePreparationService : MonoBehaviour
{
    private const string LastUnitRequiredMessage = "유닛을 1명 이상 배치해야 전투를 시작할 수 있습니다";

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

    public bool IsPrepareMode => isPrepareMode;
    public bool HasAnyUnit => unitRoster != null && unitRoster.RegisteredCount > 0;
    public UnitRoster UnitRoster => unitRoster;
    public UnitSummoner UnitSummoner => unitSummoner;
    public EconomyManager EconomyManager => economyManager;
    public int FreeRerollsRemaining => rerollAllowance.Remaining;
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
    }

    private void OnEnable()
    {
        if (placementController != null)
        {
            placementController.OnSellRequested += HandleSellRequested;
            placementController.OnRerollRequested += HandleRerollRequested;
        }
    }

    private void OnDisable()
    {
        ClosePreparationInput();

        if(placementController != null)
        {
            placementController.OnSellRequested -= HandleSellRequested;
            placementController.OnRerollRequested -= HandleRerollRequested;
        }
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
        if (!isPrepareMode || unitTransactions == null)
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
        if (!isPrepareMode || unitTransactions == null)
        {
            failure = StageUnitTransactionFailure.NotAvailable;
            return false;
        }

        return unitTransactions.TryIncreasePopulation(out failure);
    }

    private void HandleSellRequested(UnitController unit)
    {
        if (TrySellUnit(unit, out StageUnitTransactionFailure failure))
            return;

        placementController?.RestoreDraggingUnitPosition();
        if (failure == StageUnitTransactionFailure.LastUnitRequired)
            UIFeedbackToast.Show(LastUnitRequiredMessage);
    }

    private void HandleRerollRequested(UnitController unit)
    {
        TryRerollUnit(unit);
    }

    public bool TrySellUnit(UnitController unit)
    {
        return TrySellUnit(unit, out _);
    }

    public bool TrySellUnit(UnitController unit, out StageUnitTransactionFailure failure)
    {
        if (!isPrepareMode || unitTransactions == null)
        {
            failure = StageUnitTransactionFailure.NotAvailable;
            return false;
        }

        return unitTransactions.TrySellUnit(unit, out failure);
    }

    public bool TryRerollUnit(UnitController unit)
    {
        bool succeeded = isPrepareMode && unitTransactions != null && unitTransactions.TryRerollUnit(unit);
        if (succeeded)
            GameAudioManager.Instance?.PlaySfx(GameAudioCue.UnitSummon);

        return succeeded;
    }

    public void GrantFreeRerolls(int count)
    {
        rerollAllowance.Grant(count);
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
}
