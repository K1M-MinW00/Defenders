using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Stage/Stage Data")]
public class StageDataSO : ScriptableObject
{
    [Header("Stage ID")]
    public int sector;
    public int stage;

    [Header("Map")]
    public GameObject mapPrefab;

    [Header("Wave")]
    public List<WaveData> waves = new();

    [Header("Economy")]
    public EconomyConfig economyConfig;

    public string StageKey => $"{sector}-{stage}";
}