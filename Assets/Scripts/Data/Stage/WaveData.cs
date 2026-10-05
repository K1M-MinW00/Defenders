using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class WaveData
{
    public WaveType waveType;
    [Min(0.01f)] public float hpMultiplier = 1f;
    [Min(0.01f)] public float attackMultiplier = 1f;
    [Min(0.01f)] public float pressureMultiplier = 1f;
    public List<SubWaveData> subWaves = new();


    public int TotalMonsterCount
    {
        get
        {
            if (subWaves == null)
                return 0;

            int total = 0;

            foreach (SubWaveData subWave in subWaves)
            {
                if (subWave?.spawnEntries == null)
                    continue;

                foreach (MonsterSpawnEntry entry in subWave.spawnEntries)
                {
                    if (entry == null)
                        continue;

                    total += Mathf.Max(0, entry.count);
                }
            }

            return total;
        }
    }
}
public enum WaveType
{
    Normal,
    Elite,
    Boss
}

[System.Serializable]
public class SubWaveData
{
    public List<MonsterSpawnEntry> spawnEntries = new();

    [Tooltip("Delay before the next sub-wave begins.")]
    public float delayAfterSubWave = 1f;
}

[System.Serializable]
public class MonsterSpawnEntry
{
    public MonsterDataSO data;

    [Header("Balance Override")]
    [Min(0.01f)] public float hpMultiplier = 1f;
    [Min(0.01f)] public float attackMultiplier = 1f;

    [Min(0)]
    public int count = 1;

    [Tooltip("Index in the StageMapContext monster spawn point array.")]
    [Min(0)]
    public int spawnPointIndex = 0;

    [Tooltip("Delay between monsters in this spawn group.")]
    public float interval = 0.1f;

    [Tooltip("Delay before the next spawn group begins.")]
    public float delayAfterGroup = 0f;
}
