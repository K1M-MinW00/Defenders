using System;
using UnityEngine;

public class PopulationManager : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private UnitRoster unitRoster;
    [SerializeField] private EconomyManager economyManager;

    [Header("Limits")]
    [SerializeField] private int initialMax = 5;
    [SerializeField] private int hardMax = 10;

    [Header("Increase Costs (Max 5->6, 6->7, ... 9->10)")]
    [SerializeField] private int[] increaseCosts = { 5, 10, 15, 20, 25 };

    public int MaxPopulation { get; private set; }
    public int CurrentPopulation => unitRoster?.RegisteredCount ?? 0;
    public bool IsInitialized { get; private set; }

    public event Action<int, int> OnPopulationChanged;

    private void Awake()
    {
        initialMax = Mathf.Max(1, initialMax);
        hardMax = Mathf.Max(initialMax, hardMax);
        MaxPopulation = initialMax;
        IsInitialized = unitRoster != null && economyManager != null;
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
        Notify();
    }

    private void OnDisable()
    {
        if (unitRoster != null)
            unitRoster.OnRosterChanged -= Notify;
    }

    public bool CanSummon()
    {
        return IsInitialized && CurrentPopulation < MaxPopulation;
    }

    public bool CanIncreaseMax()
    {
        return IsInitialized && MaxPopulation < hardMax;
    }

    public int GetNextIncreaseCost()
    {
        if (!CanIncreaseMax())
            return -1;

        if (increaseCosts == null || increaseCosts.Length == 0) 
            return -1;

        int index = MaxPopulation - initialMax;
        if (index < 0 || index >= increaseCosts.Length || increaseCosts[index] < 0)
            return -1;

        return increaseCosts[index];
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

    private void Notify()
    {
        OnPopulationChanged?.Invoke(CurrentPopulation, MaxPopulation);
    }
}
