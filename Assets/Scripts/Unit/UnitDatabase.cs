using System.Collections.Generic;
using UnityEngine;

public static class UnitDatabase
{
    private static Dictionary<string, UnitDataSO> unitDict;

    public static void Initialize()
    {
        if (unitDict != null)
            return;

        unitDict = new();

        UnitDataSO[] units = Resources.LoadAll<UnitDataSO>("UnitData");

        foreach (UnitDataSO unit in units)
        {
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

    public static UnitDataSO GetUnit(string unitId)
    {
        if (string.IsNullOrEmpty(unitId))
            return null;

        unitDict.TryGetValue(unitId, out UnitDataSO unit);

        return unit;
    }
}