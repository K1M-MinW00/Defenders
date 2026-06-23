using System.Collections.Generic;
using UnityEngine;

public static class UnitDatabase
{
    private static Dictionary<string, UnitDataSO> unitDict;
    private const string UnitDataPath = "UnitData";

    public static void Initialize()
    {
        if (unitDict != null)
            return;

        unitDict = new();

        UnitDataSO[] units = Resources.LoadAll<UnitDataSO>(UnitDataPath);

        if (units == null || units.Length == 0)
            return;

        foreach (UnitDataSO unit in units)
        {
            if(unit == null) 
                continue;
            
            if (string.IsNullOrEmpty(unit.unitId))
            {
                Debug.LogError($"UnitId is Empty : {unit.name}");
                continue;
            }

            if (unitDict.ContainsKey(unit.unitId))
            {
                Debug.LogError($"Duplicate UnitId : {unit.unitId}");
                continue;
            }

            unitDict.Add(unit.unitId, unit);
        }

        Debug.Log($"UnitDatabase Loaded : {unitDict.Count}");
    }

    public static UnitDataSO Get(string unitId)
    {
        if (string.IsNullOrEmpty(unitId))
            return null;

        unitDict.TryGetValue(unitId, out UnitDataSO unit);

        return unit;
    }

    public static Sprite GetIcon(string unitId)
    {
        UnitDataSO unit = Get(unitId);

        return unit != null ? unit.icon : null;
    }

    public static IReadOnlyCollection<UnitDataSO> GetAll()
    {
        return unitDict.Values;
    }
}