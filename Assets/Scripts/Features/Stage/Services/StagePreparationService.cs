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

    public bool IsPrepareMode => isPrepareMode;

    private void Awake()
    {
        unitTransactions = new StageUnitTransactionService(
            unitSummoner,
            unitRoster,
            populationManager,
            economyManager);

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
        bool succeeded = isPrepareMode && unitTransactions != null && unitTransactions.TrySummonUnit();
        if (succeeded)
            GameAudioManager.Instance?.PlaySfx(GameAudioCue.UnitSummon);

        return succeeded;
    }

    public bool TryIncreasePopulation()
    {
        return isPrepareMode && unitTransactions != null && unitTransactions.TryIncreasePopulation();
    }

    private void HandleSellRequested(UnitController unit)
    {
        TrySellUnit(unit);
    }

    private void HandleRerollRequested(UnitController unit)
    {
        TryRerollUnit(unit);
    }

    public bool TrySellUnit(UnitController unit)
    {
        return isPrepareMode && unitTransactions != null && unitTransactions.TrySellUnit(unit);
    }

    public bool TryRerollUnit(UnitController unit)
    {
        bool succeeded = isPrepareMode && unitTransactions != null && unitTransactions.TryRerollUnit(unit);
        if (succeeded)
            GameAudioManager.Instance?.PlaySfx(GameAudioCue.UnitSummon);

        return succeeded;
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
