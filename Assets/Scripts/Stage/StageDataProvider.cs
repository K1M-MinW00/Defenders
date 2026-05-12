using System.Collections.Generic;
using UnityEngine;

public class StageDataProvider
{
    private readonly Dictionary<string, StageDataSO> cache = new();

    public StageDataSO Load(int sector, int stage)
    {
        string key = $"{sector}-{stage}";

        if (cache.TryGetValue(key, out StageDataSO cached))
            return cached;

        string path = $"StageData/Stage_{sector}_{stage}";
        StageDataSO data = Resources.Load<StageDataSO>(path);

        if (data == null)
        {
            Debug.LogError($"StageDataSO not found. Path: {path}");
            return null;
        }

        cache[key] = data;
        return data;
    }
}