using System;
using UnityEngine;

public class EconomyManager : MonoBehaviour
{
    [SerializeField] private EconomyConfig config;

    public int CurrentGold { get; private set; }

    public event Action<int> OnGoldChanged;
    public bool IsInitialized => config != null;

    public void Init(EconomyConfig config)
    {
        if (config == null)
        {
            Debug.LogError("EconomyConfig is null");
            return;
        }

        this.config = config;
        CurrentGold = config.initialGold;

        NotifyGoldChanged();
    }

    public void ApplyWaveReward(WaveType waveType)
    {
        if (config == null)
            return;

        int before = CurrentGold;
        int bonus = config.CalculateBonus(before);
        int waveReward = config.GetWaveReward(waveType);

        AddGold(bonus);
        AddGold(waveReward);
    }

    private void AddGold(int amount)
    {
        if (amount <= 0)
            return;

        CurrentGold += amount;
        NotifyGoldChanged();
    }

    public bool TrySpendGold(int cost)
    {
        if (cost <= 0)
            return true;

        if (CurrentGold < cost)
            return false;

        CurrentGold -= cost;
        NotifyGoldChanged();

        return true;
    }

    public bool TrySummonUnit() => TrySpendGold(config.summonUnit);
    public bool TryReroll() => TrySpendGold(config.reRollUnit);
    

    public int GetSummonCost() => config.summonUnit;
    public int GetRerollCost() => config.reRollUnit;
    public int GetSellCost(int star) => config.CalculateSellUnit(star);

    public void SellUnit(int star)
    {
        int price = GetSellCost(star);
        AddGold(price);
    }

    private void NotifyGoldChanged()
    {
        OnGoldChanged?.Invoke(CurrentGold);
    }
}
