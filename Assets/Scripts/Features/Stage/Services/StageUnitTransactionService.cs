public enum StageUnitTransactionFailure
{
    None,
    NotAvailable,
    InsufficientMinerals,
    PopulationFull,
    PopulationLimitReached,
    UnitCreationFailed,
    LastUnitRequired,
}

public sealed class StageUnitTransactionService
{
    private readonly UnitSummoner unitSummoner;
    private readonly UnitRoster unitRoster;
    private readonly PopulationManager populationManager;
    private readonly EconomyManager economyManager;
    private readonly StageRerollAllowance rerollAllowance;

    public StageUnitTransactionService(
        UnitSummoner unitSummoner,
        UnitRoster unitRoster,
        PopulationManager populationManager,
        EconomyManager economyManager,
        StageRerollAllowance rerollAllowance)
    {
        this.unitSummoner = unitSummoner;
        this.unitRoster = unitRoster;
        this.populationManager = populationManager;
        this.economyManager = economyManager;
        this.rerollAllowance = rerollAllowance;
    }

    public bool IsConfigured => unitSummoner != null && unitRoster != null &&
                                populationManager != null && economyManager != null;

    public bool TrySummonUnit()
    {
        return TrySummonUnit(out _);
    }

    public bool TrySummonUnit(out StageUnitTransactionFailure failure)
    {
        if (!IsConfigured)
        {
            failure = StageUnitTransactionFailure.NotAvailable;
            return false;
        }

        if (!populationManager.CanSummon())
        {
            failure = StageUnitTransactionFailure.PopulationFull;
            return false;
        }

        int summonCost = economyManager.GetSummonCost();
        if (summonCost < 0)
        {
            failure = StageUnitTransactionFailure.NotAvailable;
            return false;
        }

        if (!economyManager.TrySummonUnit())
        {
            failure = economyManager.CurrentGold < summonCost
                ? StageUnitTransactionFailure.InsufficientMinerals
                : StageUnitTransactionFailure.NotAvailable;
            return false;
        }

        if (!unitSummoner.TryCreateRandomUnit(out UnitController unit))
        {
            economyManager.RefundGold(summonCost);
            failure = StageUnitTransactionFailure.UnitCreationFailed;
            return false;
        }

        unitSummoner.CommitSummonedUnit(unit);
        failure = StageUnitTransactionFailure.None;
        return true;
    }

    public bool TryIncreasePopulation()
    {
        return TryIncreasePopulation(out _);
    }

    public bool TryIncreasePopulation(out StageUnitTransactionFailure failure)
    {
        if (!IsConfigured)
        {
            failure = StageUnitTransactionFailure.NotAvailable;
            return false;
        }

        if (!populationManager.CanIncreaseMax())
        {
            failure = StageUnitTransactionFailure.PopulationLimitReached;
            return false;
        }

        int cost = populationManager.GetNextIncreaseCost();
        if (cost < 0)
        {
            failure = StageUnitTransactionFailure.NotAvailable;
            return false;
        }

        if (economyManager.CurrentGold < cost)
        {
            failure = StageUnitTransactionFailure.InsufficientMinerals;
            return false;
        }

        if (!populationManager.TryIncreaseMax())
        {
            failure = StageUnitTransactionFailure.NotAvailable;
            return false;
        }

        failure = StageUnitTransactionFailure.None;
        return true;
    }

    public bool TrySellUnit(UnitController unit)
    {
        return TrySellUnit(unit, out _);
    }

    public bool TrySellUnit(UnitController unit, out StageUnitTransactionFailure failure)
    {
        if (!IsConfigured || unit == null)
        {
            failure = StageUnitTransactionFailure.NotAvailable;
            return false;
        }

        if (unitRoster.RegisteredCount <= 1)
        {
            failure = StageUnitTransactionFailure.LastUnitRequired;
            return false;
        }

        int star = unit.Star;
        if (economyManager.GetSellCost(star) < 0 ||
            !unit.TryBeginRemoval(UnitRemovalReason.Sold))
        {
            failure = StageUnitTransactionFailure.NotAvailable;
            return false;
        }

        unitRoster.Unregister(unit);
        if (!economyManager.SellUnit(star))
            UnityEngine.Debug.LogError("Failed to grant unit sell gold after removal.");
        unit.ReturnToPool();
        failure = StageUnitTransactionFailure.None;
        return true;
    }

    public bool TryRerollUnit(UnitController unit, UnityEngine.Vector3? replacementPosition = null)
    {
        if (!IsConfigured || unit == null || unit.Star != 1)
            return false;

        bool useFreeReroll = rerollAllowance?.HasFreeReroll == true;
        if (!useFreeReroll && !economyManager.TryReroll())
            return false;

        UnityEngine.Vector3 spawnPosition = replacementPosition ?? unit.transform.position;
        if (!unitSummoner.TryCreateRandomUnit(out UnitController replacement, requestedPosition: spawnPosition))
        {
            if (!useFreeReroll)
                economyManager.RefundGold(economyManager.GetRerollCost());
            return false;
        }

        if (!unit.TryBeginRemoval(UnitRemovalReason.Rerolled))
        {
            unitSummoner.DiscardCreatedUnit(replacement);
            if (!useFreeReroll)
                economyManager.RefundGold(economyManager.GetRerollCost());
            return false;
        }

        unitRoster.Unregister(unit);
        unit.ReturnToPool();
        unitSummoner.CommitSummonedUnit(replacement);
        if (useFreeReroll)
            rerollAllowance.TryConsume();
        return true;
    }
}
