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
    }

    public void ExitPrepareMode()
    {
        isPrepareMode = false;

        placementController.EnablePlacement(false);

        unitResetService.CapturePreWavePositions(unitRoster);
        BeginUnitsCombat();
    }

    public void EndCurrentPhase()
    {
        isPrepareMode = false;
        placementController.EnablePlacement(false);

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
        if (!isPrepareMode)
            return false;

        if (!populationManager.CanSummon())
            return false;

        if (!economyManager.TrySummonUnit())
            return false;

        if (!unitSummoner.TryCreateRandomUnit(out UnitController unit))
        {
            economyManager.RefundGold(economyManager.GetSummonCost());
            return false;
        }

        unitSummoner.CommitSummonedUnit(unit);

        GameAudioManager.Instance?.PlaySfx(GameAudioCue.UnitSummon);
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

        if (!unit.TryBeginRemoval(UnitRemovalReason.Sold))
            return false;

        int star = unit.Star;
        unitRoster.Unregister(unit);
        economyManager.SellUnit(star);
        unit.ReturnToPool();

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

        if (!unitSummoner.TryCreateRandomUnit(out UnitController replacement))
        {
            economyManager.RefundGold(economyManager.GetRerollCost());
            return false;
        }

        if (!unit.TryBeginRemoval(UnitRemovalReason.Rerolled))
        {
            unitSummoner.DiscardCreatedUnit(replacement);
            economyManager.RefundGold(economyManager.GetRerollCost());
            return false;
        }

        unitRoster.Unregister(unit);
        unit.ReturnToPool();
        unitSummoner.CommitSummonedUnit(replacement);

        GameAudioManager.Instance?.PlaySfx(GameAudioCue.UnitSummon);
        return true;
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
}
