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
        if(placementController != null)
        {
            placementController.OnSellRequested -= HandleSellRequested;
            placementController.OnRerollRequested -= HandleRerollRequested;
        }
    }

    public void EnterPrepareMode()
    {
        isPrepareMode = true;

        placementController.EnablePlacement(true);

        unitResetService.RestoreAll(unitRoster);
        SetUnitsCombatPhase(false);
    }

    public void ExitPrepareMode()
    {
        isPrepareMode = false;

        placementController.EnablePlacement(false);

        unitResetService.CapturePreWavePositions(unitRoster);
        SetUnitsCombatPhase(true);
    }

    public bool TrySummonUnit()
    {
        if (!isPrepareMode)
            return false;

        if (!populationManager.CanSummon())
            return false;

        if (!economyManager.TrySummonUnit())
            return false;

        bool success = unitSummoner.SummonRandomUnit();

        if (!success)
            return false;

        return true;
    }

    public bool TryIncreasePopulation()
    {
        if (!isPrepareMode)
            return false;

        if (!populationManager.TryIncreaseMax())
            return false;

        return true;
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
        if (!isPrepareMode || unit == null)
            return false;

        unitRoster.Unregister(unit);

        economyManager.SellUnit(unit.Star);

        Destroy(unit.gameObject);

        return true;
    }

    public bool TryRerollUnit(UnitController unit)
    {
        if (!isPrepareMode)
            return false;

        if (unit == null || unit.Star != 1)
            return false;

        if (!economyManager.TryReroll())
            return false;

        unitRoster.Unregister(unit);
        Destroy(unit.gameObject);


        bool success = unitSummoner.SummonRandomUnit();
        if (!success)
            return false;

        return true;
    }

    private void SetUnitsCombatPhase(bool isCombat)
    {
        if (unitRoster == null)
            return;

        foreach (UnitController unit in unitRoster.Units)
        {
            if (unit == null)
                continue;

            unit.SetCombatPhase(isCombat);
        }
    }
}