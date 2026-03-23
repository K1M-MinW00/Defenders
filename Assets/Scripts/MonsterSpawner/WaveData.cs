using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;

[System.Serializable]
public class WaveData
{
    public WaveType waveType;
    public List<SubWaveData> subWaves = new();
    public int rewardGold;

    public int TotalMonsterCount
    {
        get
        {
            if (subWaves == null)
                return 0;

            int total = 0;

            foreach(var subWave in subWaves)
            {
                if (subWave?.spawnGroups == null)
                    continue;

                foreach(var entry in subWave.spawnGroups)
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
    public List<MonsterGroup> spawnGroups = new();
    public float delayAfter;
}

[System.Serializable]
public class MonsterGroup
{
    public MonsterDataSO data;
    public int count;
}