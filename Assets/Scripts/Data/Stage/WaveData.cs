using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class WaveData
{
    public WaveType waveType;
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

    [Tooltip("이 SubWave가 모두 생성된 뒤, 다음 SubWave까지 대기 시간")]
    public float delayAfterSubWave = 1f;
}

[System.Serializable]
public class MonsterSpawnEntry
{
    public MonsterDataSO data;

    [Min(0)]
    public int count = 1;

    [Tooltip("StageMapContext의 SpawnPoints 배열 인덱스")]
    [Min(0)]
    public int spawnPointIndex = 0;

    [Tooltip("같은 그룹 내 몬스터 간 생성 간격")]
    public float interval = 0.1f;

    [Tooltip("이 그룹 생성이 끝난 뒤 다음 그룹까지 대기 시간")]
    public float delayAfterGroup = 0f;
}