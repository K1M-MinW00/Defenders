using System;
using Unity.VisualScripting;
using UnityEngine;

public class PopulationManager : MonoBehaviour
{
    [Header("Limits")]
    [SerializeField] private int initialMax = 5;
    [SerializeField] private int hardMax = 10;

    [Header("Increase Costs (Max 5->6, 6->7, ... 9->10)")]
    [SerializeField] private int[] increaseCosts = { 5, 10, 15, 20, 25 };

    [Header("Reference")]
    [SerializeField] private UnitRoster unitRoster;
    [SerializeField] private EconomyManager economyManager;

    public int MaxPopulation { get; private set; }
    public int CurrentPopulation => unitRoster.Units.Count;

    public event Action<int, int> OnPopulationChanged;

    public void Init(UnitRoster unitRoster, EconomyManager economyManager)
    {
        this.unitRoster = unitRoster;
        this.economyManager = economyManager;

        unitRoster.OnRosterChanged += Notify;
        MaxPopulation = Mathf.Clamp(initialMax,0,hardMax);
        Notify();
    }

    private void OnDisable()
    {
        if (unitRoster != null)
            unitRoster.OnRosterChanged -= Notify;
    }

    public bool CanSummon()
    {
        return CurrentPopulation < MaxPopulation;
    }

    public bool CanIncreaseMax()
    {
        return MaxPopulation < hardMax;
    }

    public int GetNextIncreaseCost()
    {
        if (!CanIncreaseMax())
            return -1;

        if (increaseCosts == null || increaseCosts.Length == 0) 
            return -1;

        int index = MaxPopulation - initialMax;
        index = Mathf.Clamp(index, 0, increaseCosts.Length - 1);
        
        return increaseCosts[index];
    }

    public bool TryIncreaseMax()
    {
        if (!CanIncreaseMax())
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