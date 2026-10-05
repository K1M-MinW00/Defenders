using System;
using System.Collections.Generic;
using UnityEngine;

public enum StageThreatType
{
    Balanced,
    Swarm,
    Durable,
    Aggressive,
    Ranged,
    Elite
}

[Serializable]
public sealed class StageThreatPreset
{
    public StageThreatType type;
    [Min(0.01f)] public float hpMultiplier = 1f;
    [Min(0.01f)] public float attackMultiplier = 1f;
    [Min(0.01f)] public float pressureMultiplier = 1f;
}

[CreateAssetMenu(menuName = "Game/Stage/Stage Balance Config")]
public sealed class StageBalanceConfigSO : ScriptableObject
{
    [Header("Automatic progression growth")]
    [Min(0f)] public float hpGrowthPerStage = 0.12f;
    [Min(0f)] public float attackGrowthPerStage = 0.065f;

    [Header("Editor warnings")]
    [Min(1f)] public float threatSpikeWarningRatio = 1.6f;
    [Range(0f, 1f)] public float rangedRatioWarning = 0.6f;

    [Header("Stage presets")]
    public List<StageThreatPreset> presets = new();

    public StageThreatPreset GetPreset(StageThreatType type)
    {
        StageThreatPreset preset = presets?.Find(candidate => candidate != null && candidate.type == type);
        return preset ?? new StageThreatPreset { type = type };
    }
}

public static class StageBalanceConfigDatabase
{
    private const string ResourcePath = "GameData/Configs/StageBalanceConfig";
    private static StageBalanceConfigSO cached;

    public static StageBalanceConfigSO Get()
    {
        if (cached == null)
            cached = Resources.Load<StageBalanceConfigSO>(ResourcePath);

        if (cached == null)
            cached = CreateRuntimeDefault();

        return cached;
    }

    private static StageBalanceConfigSO CreateRuntimeDefault()
    {
        StageBalanceConfigSO config = ScriptableObject.CreateInstance<StageBalanceConfigSO>();
        config.hideFlags = HideFlags.HideAndDontSave;
        config.presets = new List<StageThreatPreset>
        {
            Preset(StageThreatType.Balanced, 1f, 1f, 1f),
            Preset(StageThreatType.Swarm, 0.82f, 0.9f, 1.3f),
            Preset(StageThreatType.Durable, 1.3f, 0.9f, 0.9f),
            Preset(StageThreatType.Aggressive, 0.9f, 1.22f, 1.05f),
            Preset(StageThreatType.Ranged, 0.95f, 1.05f, 1f),
            Preset(StageThreatType.Elite, 1.25f, 1.15f, 0.8f)
        };
        return config;
    }

    private static StageThreatPreset Preset(
        StageThreatType type,
        float hp,
        float attack,
        float pressure)
    {
        return new StageThreatPreset
        {
            type = type,
            hpMultiplier = hp,
            attackMultiplier = attack,
            pressureMultiplier = pressure
        };
    }
}
