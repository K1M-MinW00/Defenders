using System;
using System.Linq;
using UnityEngine;

public class EconomyManager : MonoBehaviour
{
    public static EconomyManager Instance { get; private set; }

    private EconomyConfig config;
    private int populationIndex;
    
    [SerializeField] private int[] populationCostCurve;

    public int CurrentGold { get; private set; }

    public event Action<int> OnGoldChanged;

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
        this.config = config;

        CurrentGold = config.initialGold;
        populationIndex = 0;

        OnGoldChanged?.Invoke(CurrentGold);
    }

    public void ApplyWaveClearReward(WaveType waveType)
    {
        int before = CurrentGold;

        int bonus = config.CalculateBonus(before);

        if (bonus > 0)
            AddGold(bonus);

        int reward = config.GetWaveReward(waveType);

        if (reward > 0)
            AddGold(reward);
    }

    private void AddGold(int amount)
    {
        if (amount <= 0)
            return;

        int before = CurrentGold;
        CurrentGold += amount;

        // UI 업데이트
        OnGoldChanged?.Invoke(CurrentGold);
    }

    public bool TrySpendGold(int cost)
    {
        if (cost <= 0)
            return true;

        if (CurrentGold < cost)
            return false;

        int before = CurrentGold;
        CurrentGold -= cost;


        // UI 업데이트 
        OnGoldChanged?.Invoke(CurrentGold);

        return true;
    }

    public bool TrySummonUnit() => TrySpendGold(config.summonUnit);
    public bool TryReroll() => TrySpendGold(config.reRollUnit);

    public void SellUnit(int star)
    {
        int price = config.CalculateSellUnit(star);

        AddGold(price);
    }
}
