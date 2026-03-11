using System.Collections.Generic;
using UnityEngine;

public class UnitMasterDataManager : MonoBehaviour
{
    public static UnitMasterDataManager Instance { get; private set; }

    [SerializeField] private Dictionary<UnitCode, UnitDataSO> unitDataMap = new();

    [SerializeField] private List<UnitCode> starterUnits = new();
    [SerializeField] private List<UnitCode> defaultOwnedUnits = new();
    public IReadOnlyList<UnitCode> StarterUnits => starterUnits;
    public IReadOnlyList<UnitCode> DefaultOwnedUnits => defaultOwnedUnits;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadAllUnitData();
    }

    private void LoadAllUnitData()
    {
        unitDataMap.Clear();

        UnitDataSO[] unitDataArr = Resources.LoadAll<UnitDataSO>("UnitData");

        foreach (var unitData in unitDataArr)
        {
            if (unitData == null)
                continue;

            if (unitDataMap.ContainsKey(unitData.UnitCode))
            {
                Debug.LogError($"[UnitMasterDataManager] Duplicate UnitCode : {unitData.UnitCode}");
                continue;
            }

            unitDataMap.Add(unitData.UnitCode, unitData);
        }
    }

    public UnitDataSO GetUnitData(UnitCode unitCode)
    {
        unitDataMap.TryGetValue(unitCode, out UnitDataSO unitData);
        return unitData;
    }

    public IReadOnlyCollection<UnitDataSO> GetAllUnitData()
    {
        return unitDataMap.Values;
    }

    public bool Contains(UnitCode unitCode)
    {
        return unitDataMap.ContainsKey(unitCode);
    }
}

