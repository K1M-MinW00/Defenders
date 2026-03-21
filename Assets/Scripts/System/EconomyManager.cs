using System;
using UnityEngine;

public class EconomyManager : MonoBehaviour
{
    public static EconomyManager Instance { get; private set; }

    private EconomyConfig config;

    public int CurrentGold { get; private set; }

    public event Action<int> OnGoldChanged;
    public bool IsInitialized => config != null;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

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

    public void SellUnit(int star)
    {
        int price = config.CalculateSellUnit(star);
        AddGold(price);
    }

    private void NotifyGoldChanged()
    {
        OnGoldChanged?.Invoke(CurrentGold);
    }
}
