using System;
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

    public int MaxPopulation { get; private set; }

    public int CurrentPopulation => unitRoster != null ? unitRoster.Units.Count : 0;

    public event Action<int, int> OnPopulationChanged; // (current, max)

    private void Awake()
    {
        if (unitRoster == null)
            unitRoster = FindFirstObjectByType<UnitRoster>();
    }

    private void Start()
    {
        if(unitRoster != null)
        {
            unitRoster.OnUnitAdded += _ => Notify();
            unitRoster.OnUnitRemoved += _ => Notify();
        }
    }
    public void Init()
    {
        MaxPopulation = initialMax;
        Notify();
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

        // MaxPopulation이 5면 index 0 (5->6), 6이면 index 1 ...
        int idx = MaxPopulation - initialMax;
        if (idx < 0) idx = 0;
        if (increaseCosts == null || increaseCosts.Length == 0) return -1;

        // 안전: 배열 길이 넘어가면 마지막 값 유지
        if (idx >= increaseCosts.Length)
            idx = increaseCosts.Length - 1;

        return increaseCosts[idx];
    }

    public bool TryIncreaseMax()
    {
        if (!CanIncreaseMax())
            return false;

        int cost = GetNextIncreaseCost();
        if (cost < 0)
            return false;

        if (!EconomyManager.Instance.TrySpendGold(cost))
            return false;

        MaxPopulation++;
        Notify();
        return true;
    }

    public void Notify()
    {
        OnPopulationChanged?.Invoke(CurrentPopulation, MaxPopulation);
    }
}