using System.Collections.Generic;
using UnityEngine;

public class UnitMasterDataManager : MonoBehaviour
{
    public static UnitMasterDataManager Instance { get; private set; }

    private readonly Dictionary<string, UnitDataSO> unitDataMap = new();

    [Header("Default User Units")]
    [SerializeField] private List<string> defaultOwnedUnitIds = new();
    
    public IReadOnlyList<string> DefaultOwnedUnitIds => defaultOwnedUnitIds;
    public bool IsLoaded { get;private set; }

    private const string UnitDataPath = "UnitData";

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
        IsLoaded = false;
        unitDataMap.Clear();

        UnitDataSO[] unitDataArr = Resources.LoadAll<UnitDataSO>(UnitDataPath);

        if(unitDataArr == null || unitDataArr.Length == 0)
        {
            Debug.LogError($"[UnitMasterDataManager] No UnitDataSo found in Resources/{UnitDataPath}.");
            return;
        }

        foreach (var unitData in unitDataArr)
        {
            if (unitData == null)
                continue;

            if (string.IsNullOrWhiteSpace(unitData.unitId))
            {
                Debug.LogError($"[UnitMasterDataManager] UnitDataSO has empty unitId. Asset: {unitData.name}");
                continue;
            }

            if (unitDataMap.ContainsKey(unitData.unitId))
            {
                Debug.LogError($"[UnitMasterDataManager] Duplicate unitId: {unitData.unitId}");
                continue;
            }

            unitDataMap.Add(unitData.unitId, unitData);
        }
        
        IsLoaded = unitDataMap.Count > 0;

        Debug.Log($"[UnitMasterDataManager] Loaded unit count: {unitDataMap.Count}");
    }


    public UnitDataSO GetUnitData(string unitId)
    {
        if (string.IsNullOrWhiteSpace(unitId))
            return null;

        unitDataMap.TryGetValue(unitId, out UnitDataSO data);
        return data;
    }

    public IReadOnlyCollection<UnitDataSO> GetAllUnitData()
    {
        return unitDataMap.Values;
    }

    public bool Contains(string unitId)
    {
        return unitDataMap.ContainsKey(unitId);
    }
}

