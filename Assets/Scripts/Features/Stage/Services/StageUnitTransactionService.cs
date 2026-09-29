public sealed class StageUnitTransactionService
{
    private readonly UnitSummoner unitSummoner;
    private readonly UnitRoster unitRoster;
    private readonly PopulationManager populationManager;
    private readonly EconomyManager economyManager;

    public StageUnitTransactionService(
        UnitSummoner unitSummoner,
        UnitRoster unitRoster,
        PopulationManager populationManager,
        EconomyManager economyManager)
    {
        this.unitSummoner = unitSummoner;
        this.unitRoster = unitRoster;
        this.populationManager = populationManager;
        this.economyManager = economyManager;
    }

    public bool IsConfigured => unitSummoner != null && unitRoster != null &&
                                populationManager != null && economyManager != null;

    public bool TrySummonUnit()
    {
        if (!IsConfigured || !populationManager.CanSummon())
            return false;

        if (!economyManager.TrySummonUnit())
            return false;

        if (!unitSummoner.TryCreateRandomUnit(out UnitController unit))
        {
            economyManager.RefundGold(economyManager.GetSummonCost());
            return false;
        }

        unitSummoner.CommitSummonedUnit(unit);
        return true;
    }

    public bool TryIncreasePopulation()
    {
        return IsConfigured && populationManager.TryIncreaseMax();
    }

    public bool TrySellUnit(UnitController unit)
    {
        if (!IsConfigured || unit == null || !unit.TryBeginRemoval(UnitRemovalReason.Sold))
            return false;

        int star = unit.Star;
        unitRoster.Unregister(unit);
        economyManager.SellUnit(star);
        unit.ReturnToPool();
        return true;
    }

    public bool TryRerollUnit(UnitController unit)
    {
        if (!IsConfigured || unit == null || unit.Star != 1)
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
        return true;
    }
}
