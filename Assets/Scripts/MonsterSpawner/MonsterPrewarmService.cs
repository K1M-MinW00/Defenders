using System.Collections.Generic;
using UnityEngine;

public class MonsterPrewarmService : MonoBehaviour
{
    [SerializeField] private StagePoolManager poolManager;

    public void PrewarmForWave(WaveData waveData)
    {
        if (waveData == null || poolManager == null)
            return;

        Dictionary<MonsterDataSO, int> requiredCounts = CalculateRequiredCounts(waveData);

        foreach (var pair in requiredCounts)
        {
            MonsterDataSO data = pair.Key;
            int requiredCount = pair.Value;

            if(data == null || data.prefab == null)
            {
                Debug.LogError("Monster prewarm failed. MonsterDataSO or prefab is null.");
                continue;
            }

            int inactiveCount = poolManager.GetInactiveCount(data.prefab);
            int shortage = requiredCount - inactiveCount;

            if (shortage <= 0)
                continue;

            poolManager.Prewarm(data.prefab, shortage, PoolCategory.Monster);
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
                if (group == null || group.data == null)
                    continue;

                if (!counts.ContainsKey(group.data))
                    counts[group.data] = 0;

                counts[group.data] += group.count;
            }
        }

        return counts;
    }
}