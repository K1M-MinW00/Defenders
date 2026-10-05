using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class StageBalanceDataMigration
{
    private const string ConfigFolder = "Assets/Resources/GameData/Configs";
    private const string ConfigPath = ConfigFolder + "/StageBalanceConfig.asset";

    [MenuItem("Tools/Defenders/Stage/Initialize Stage Balance Data")]
    public static void Run()
    {
        EnsureFolder("Assets/Resources", "GameData");
        EnsureFolder("Assets/Resources/GameData", "Configs");
        StageBalanceConfigSO config = AssetDatabase.LoadAssetAtPath<StageBalanceConfigSO>(ConfigPath);
        if (config == null)
        {
            config = ScriptableObject.CreateInstance<StageBalanceConfigSO>();
            config.presets = new List<StageThreatPreset>
            {
                Preset(StageThreatType.Balanced, 1f, 1f, 1f),
                Preset(StageThreatType.Swarm, 0.82f, 0.9f, 1.3f),
                Preset(StageThreatType.Durable, 1.3f, 0.9f, 0.9f),
                Preset(StageThreatType.Aggressive, 0.9f, 1.22f, 1.05f),
                Preset(StageThreatType.Ranged, 0.95f, 1.05f, 1f),
                Preset(StageThreatType.Elite, 1.25f, 1.15f, 0.8f)
            };
            AssetDatabase.CreateAsset(config, ConfigPath);
        }

        foreach (StageDataSO stage in FindAssets<StageDataSO>())
        {
            Undo.RecordObject(stage, "Initialize stage balance data");
            stage.hpMultiplier = Positive(stage.hpMultiplier);
            stage.attackMultiplier = Positive(stage.attackMultiplier);
            stage.pressureMultiplier = Positive(stage.pressureMultiplier);
            foreach (WaveData wave in stage.waves ?? new List<WaveData>())
            {
                if (wave == null) continue;
                wave.hpMultiplier = Positive(wave.hpMultiplier);
                wave.attackMultiplier = Positive(wave.attackMultiplier);
                wave.pressureMultiplier = Positive(wave.pressureMultiplier);
                foreach (SubWaveData subWave in wave.subWaves ?? new List<SubWaveData>())
                foreach (MonsterSpawnEntry entry in subWave?.spawnEntries ?? new List<MonsterSpawnEntry>())
                {
                    if (entry == null) continue;
                    entry.hpMultiplier = Positive(entry.hpMultiplier);
                    entry.attackMultiplier = Positive(entry.attackMultiplier);
                }
            }
            EditorUtility.SetDirty(stage);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Stage balance configuration and default multipliers are ready.");
    }

    private static StageThreatPreset Preset(StageThreatType type, float hp, float attack, float pressure) => new() { type = type, hpMultiplier = hp, attackMultiplier = attack, pressureMultiplier = pressure };
    private static float Positive(float value) => value > 0f ? value : 1f;
    private static IEnumerable<T> FindAssets<T>() where T : Object => AssetDatabase.FindAssets($"t:{typeof(T).Name}").Select(guid => AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid))).Where(asset => asset != null);
    private static void EnsureFolder(string parent, string child) { string path = $"{parent}/{child}"; if (!AssetDatabase.IsValidFolder(path)) AssetDatabase.CreateFolder(parent, child); }
}
