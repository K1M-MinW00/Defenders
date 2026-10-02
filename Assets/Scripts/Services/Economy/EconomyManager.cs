using System;
using UnityEngine;

public class EconomyManager : MonoBehaviour
{
    [SerializeField] private EconomyConfig config;

    public int CurrentGold { get; private set; }
    public int CurrentInterestBonus =>
        IsInitialized ? config.CalculateInterestBonus(CurrentGold) : 0;

    public event Action<int> OnGoldChanged;
    public bool IsInitialized { get; private set; }

    public bool Init(EconomyConfig config)
    {
        IsInitialized = false;

        if (!IsValidConfig(config, out string error))
        {
            Debug.LogError($"Economy initialization failed: {error}");
            return false;
        }

        this.config = config;
        CurrentGold = config.initialGold;
        IsInitialized = true;

        NotifyGoldChanged();
        return true;
    }

    public void ResetRuntime()
    {
        config = null;
        CurrentGold = 0;
        IsInitialized = false;
    }

    public bool ApplyWaveReward(WaveType waveType)
    {
        if (!IsInitialized)
            return false;

        int bonus = CurrentInterestBonus;
        int waveReward = config.GetWaveReward(waveType);

        return TryAddGold(bonus + waveReward);
    }

    public bool TryAddGold(int amount)
    {
        if (!IsInitialized || amount < 0)
            return false;
        if (amount == 0)
            return true;

        long nextGold = (long)CurrentGold + amount;
        if (nextGold > int.MaxValue)
            return false;

        CurrentGold = (int)nextGold;
        NotifyGoldChanged();
        return true;
    }

    public bool RefundGold(int amount)
    {
        return TryAddGold(amount);
    }

    public bool TrySpendGold(int cost)
    {
        if (!IsInitialized || cost < 0)
            return false;
        if (cost == 0)
            return true;

        if (CurrentGold < cost)
            return false;

        CurrentGold -= cost;
        NotifyGoldChanged();

        return true;
    }

    public bool TrySummonUnit() => IsInitialized && TrySpendGold(config.summonUnit);
    public bool TryReroll() => IsInitialized && TrySpendGold(config.reRollUnit);
    

    public int GetSummonCost() => IsInitialized ? config.summonUnit : -1;
    public int GetRerollCost() => IsInitialized ? config.reRollUnit : -1;
    public int GetSellCost(int star) =>
        IsInitialized && star >= 1 && star <= 4 ? config.CalculateSellUnit(star) : -1;

    public bool SellUnit(int star)
    {
        int price = GetSellCost(star);
        return price >= 0 && TryAddGold(price);
    }

    private void NotifyGoldChanged()
    {
        OnGoldChanged?.Invoke(CurrentGold);
    }

    private static bool IsValidConfig(EconomyConfig value, out string error)
    {
        if (value == null)
        {
            error = "Config is null.";
            return false;
        }

        if (value.initialGold < 0 || value.normalReward < 0 ||
            value.eliteReward < 0 || value.bossReward < 0 ||
            value.bonusPer10 < 0 || value.bonusCap < 0 ||
            value.summonUnit < 0 || value.reRollUnit < 0)
        {
            error = "Gold, rewards, bonuses, and costs cannot be negative.";
            return false;
        }

        if (value.sellUnit == null || value.sellUnit.Length < 4)
        {
            error = "Sell costs for stars 1 through 4 are required.";
            return false;
        }

        for (int i = 0; i < value.sellUnit.Length; i++)
        {
            if (value.sellUnit[i] < 0)
            {
                error = $"Sell cost at index {i} cannot be negative.";
                return false;
            }
        }

        error = string.Empty;
        return true;
    }
}
