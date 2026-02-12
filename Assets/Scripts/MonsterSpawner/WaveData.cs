using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class WaveData
{
    public WaveType waveType;

    public List<SubWaveData> subWaves;
    public int totalMonsterCount;
    public int rewardGold;
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
    public List<MonsterGroup> spawnGroups;
    public float delayAfter;
}

[System.Serializable]
public class MonsterGroup
{
    public string monsterId;
    public int count;
}