using System;
using UnityEngine;

public class PopulationManager : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private UnitRoster unitRoster;
    [SerializeField] private EconomyManager economyManager;

    private EconomyConfig config;

    public int MaxPopulation { get; private set; }
    public int CurrentPopulation => unitRoster?.RegisteredCount ?? 0;
    public bool IsInitialized { get; private set; }

    public event Action<int, int> OnPopulationChanged;

    private void Awake()
    {
        MaxPopulation = 0;
        IsInitialized = false;
    }
    private void OnEnable()
    {
        if(unitRoster == null)
        {
            Debug.LogError("Population Manager : Unit Roster is null");
            return;
        }

        if(economyManager == null)
        {
            Debug.LogError("Population Manager : EconomyManager is null");
            return;
        }

        unitRoster.OnRosterChanged += Notify;
        economyManager.OnInitialized += Initialize;
        if (economyManager.IsInitialized)
            Initialize(economyManager.Config);
        Notify();
    }

    private void OnDisable()
    {
        if (unitRoster != null)
            unitRoster.OnRosterChanged -= Notify;
        if (economyManager != null)
            economyManager.OnInitialized -= Initialize;
    }

    public bool CanSummon()
    {
        return IsInitialized && CurrentPopulation < MaxPopulation;
    }

    public bool CanIncreaseMax()
    {
        return IsInitialized && MaxPopulation < config.maximumPopulationLimit;
    }

    public int GetNextIncreaseCost()
    {
        if (!CanIncreaseMax())
            return -1;

        return config.GetPopulationIncreaseCost(MaxPopulation);
    }

    public bool TryIncreaseMax()
    {
        if (!CanIncreaseMax() || !economyManager.IsInitialized)
            return false;

        int cost = GetNextIncreaseCost();
        if (cost < 0)
            return false;

        if (!economyManager.TrySpendGold(cost))
            return false;

        MaxPopulation++;
        Notify();
        return true;
    }

    private void Initialize(EconomyConfig economyConfig)
    {
        config = economyConfig;
        IsInitialized = config != null && unitRoster != null && economyManager != null;
        MaxPopulation = IsInitialized ? config.initialPopulationLimit : 0;
        Notify();
    }

    private void Notify()
    {
        OnPopulationChanged?.Invoke(CurrentPopulation, MaxPopulation);
    }
}
