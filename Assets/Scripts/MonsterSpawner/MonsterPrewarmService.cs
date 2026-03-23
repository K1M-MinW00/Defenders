using System.Collections.Generic;
using UnityEngine;

public class MonsterPrewarmService : MonoBehaviour
{
    [SerializeField] private ObjectPool objectPool;

    public void PrewarmForWave(WaveData waveData)
    {
        if (waveData == null || objectPool == null)
            return;

        Dictionary<MonsterDataSO, int> requiredCounts = CalculateRequiredCounts(waveData);

        foreach (var pair in requiredCounts)
        {
            MonsterDataSO data = pair.Key;
            string monsterId = data.monsterId;
            int requiredCount = pair.Value;

            int pooledInactiveCount = objectPool.GetInactiveCount(monsterId);
            int shortage = requiredCount - pooledInactiveCount;

            if (shortage <= 0)
                continue;

            GameObject prefab = data.prefab;

            if (prefab == null)
            {
                Debug.LogError($"Monster prefab load failed: {monsterId}");
                continue;
            }

            objectPool.Prewarm(monsterId, prefab, shortage);
        }
    }

    private Dictionary<MonsterDataSO, int> CalculateRequiredCounts(WaveData waveData)
    {
        Dictionary<MonsterDataSO, int> counts = new();

        foreach (SubWaveData subWave in waveData.subWaves)
        {
            if (subWave == null)
                continue;

            foreach (MonsterGroup group in subWave.spawnGroups)
            {
                if (group == null || string.IsNullOrEmpty(group.data.monsterId))
                    continue;

                if (!counts.ContainsKey(group.data))
                    counts[group.data] = 0;

                counts[group.data] += group.count;
            }
        }

        return counts;
    }
}