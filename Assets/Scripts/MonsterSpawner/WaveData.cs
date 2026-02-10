using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Wave/WaveData")]
public class WaveData : ScriptableObject
{
    public List<SubWaveData> subWaves;
    public float delayBetweenSubWaves = 5f;
    public int totalMonsterCount;

    #if UNITY_EDITOR
    private void OnValidate()
    {
        RecalculateTotalMonsterCount();
    }
    #endif

    private void RecalculateTotalMonsterCount()
    {
        totalMonsterCount = 0;

        if (subWaves == null)
            return;

        foreach (var subWave in subWaves)
        {
            if (subWave != null)
            {
                totalMonsterCount += subWave.count;
            }
        }
    }
}

[System.Serializable]
public class SubWaveData
{
    public string monsterId;   // A, B, C, D
    public int count;
}
