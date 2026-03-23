using UnityEngine;

public class StagePreparationService : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private UnitSummoner unitSummoner;
    [SerializeField] private UnitRoster unitRoster;
    [SerializeField] private PopulationManager populationManager;
    [SerializeField] private TilemapPlacementArea placementArea;
    [SerializeField] private PlacementController placementController;

    public PopulationManager PopulationManager => populationManager;
    public UnitRoster UnitRoster => unitRoster;
    private bool isPrepareMode;

    public void SetPrepareMode(bool enabled)
    {
        isPrepareMode = enabled;

        placementController?.EnablePlacement(enabled);
        placementArea?.SetVisible(enabled);
    }

    public bool TrySummonUnit()
    {
        if (!isPrepareMode)
            return false;

        if (populationManager != null && !populationManager.CanSummon())
            return false;

        if (!EconomyManager.Instance.TrySummonUnit())
            return false;

        unitSummoner.SummonRandomUnit();
        populationManager?.Notify();
        return true;
    }

    public bool TryIncreasePopulation()
    {
        if (!isPrepareMode || populationManager == null)
            return false;

        if (!populationManager.TryIncreaseMax())
            return false;

        populationManager.Notify();
        return true;
    }

    public bool TrySellUnit(PlayerCharacter unit)
    {
        if (!isPrepareMode || unit == null)
            return false;

        var runtime = unit.GetComponent<UnitRuntime>();

        if (runtime == null)
            return false;

        unitRoster?.Unregister(runtime);
        EconomyManager.Instance.SellUnit(runtime.Star);
        Destroy(unit.gameObject);

        return true;
    }

    public bool TryRerollUnit(PlayerCharacter unit)
    {
        if (!isPrepareMode || unit == null)
            return false;

        var runtime = unit.GetComponent<UnitRuntime>();
        if (runtime == null)
            return false;

        if (!EconomyManager.Instance.TryReroll())
            return false;

        unitRoster?.Unregister(runtime);
        Destroy(unit.gameObject);

        unitSummoner.SummonRandomUnit();
        return true;
    }
}