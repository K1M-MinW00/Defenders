using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public sealed class StageBalanceEditorWindow : EditorWindow
{
    private readonly List<StageDataSO> stages = new();
    private StageDataSO selected;
    private Vector2 stageScroll;
    private Vector2 detailScroll;
    private string status;

    [MenuItem("Tools/Defenders/Stage/Stage Balance Editor")]
    public static void Open() => GetWindow<StageBalanceEditorWindow>("Stage Balance");

    private void OnEnable() => Reload();

    private void OnGUI()
    {
        DrawToolbar();
        EditorGUILayout.BeginHorizontal();
        DrawStageList();
        DrawDetails();
        EditorGUILayout.EndHorizontal();

        if (!string.IsNullOrWhiteSpace(status))
            EditorGUILayout.HelpBox(status, MessageType.Info);
    }

    private void DrawToolbar()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        if (GUILayout.Button("Reload", EditorStyles.toolbarButton)) Reload();
        if (GUILayout.Button("Validate All", EditorStyles.toolbarButton)) ValidateAll();
        if (GUILayout.Button("Export CSV", EditorStyles.toolbarButton)) ExportCsv();
        if (GUILayout.Button("Import CSV", EditorStyles.toolbarButton)) ImportCsv();
        GUILayout.FlexibleSpace();
        EditorGUILayout.LabelField("SO is runtime source · CSV is batch-edit format", EditorStyles.miniLabel, GUILayout.Width(260f));
        EditorGUILayout.EndHorizontal();
    }

    private void DrawStageList()
    {
        EditorGUILayout.BeginVertical(GUILayout.Width(170f));
        EditorGUILayout.LabelField("Stages", EditorStyles.boldLabel);
        stageScroll = EditorGUILayout.BeginScrollView(stageScroll);
        int sector = -1;
        foreach (StageDataSO stage in stages)
        {
            if (stage.sector != sector)
            {
                sector = stage.sector;
                EditorGUILayout.Space(4f);
                EditorGUILayout.LabelField($"Sector {sector}", EditorStyles.miniBoldLabel);
            }

            GUIStyle style = selected == stage ? EditorStyles.miniButtonMid : EditorStyles.miniButton;
            if (GUILayout.Button($"{stage.StageKey}  ({stage.waves?.Count ?? 0} waves)", style))
            {
                selected = stage;
                Selection.activeObject = stage;
            }
        }
        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
    }

    private void DrawDetails()
    {
        EditorGUILayout.BeginVertical();
        if (selected == null)
        {
            EditorGUILayout.HelpBox("Select a stage to edit.", MessageType.Info);
            EditorGUILayout.EndVertical();
            return;
        }

        detailScroll = EditorGUILayout.BeginScrollView(detailScroll);
        SerializedObject serialized = new(selected);
        serialized.Update();

        EditorGUILayout.LabelField($"Stage {selected.StageKey}", EditorStyles.largeLabel);
        DrawProperty(serialized, "mapPrefab");
        DrawProperty(serialized, "economyConfig");
        DrawProperty(serialized, "prepareDuration");
        EditorGUILayout.Space(6f);
        EditorGUILayout.LabelField("Difficulty", EditorStyles.boldLabel);
        DrawProperty(serialized, "threatType");
        DrawProperty(serialized, "hpMultiplier");
        DrawProperty(serialized, "attackMultiplier");
        DrawProperty(serialized, "pressureMultiplier");

        int progression = StageBalanceCalculator.GetProgressionIndex(selected, stages);
        StageBalanceMultipliers baseMultipliers = StageBalanceCalculator.Calculate(selected, progression);
        EditorGUILayout.HelpBox(
            $"Progression index: {progression}\nRuntime base multiplier  HP ×{baseMultipliers.Hp:0.00}  ATK ×{baseMultipliers.Attack:0.00}  Pressure ×{baseMultipliers.Pressure:0.00}\nUser level and XP are not used.",
            MessageType.None);

        EditorGUILayout.Space(6f);
        EditorGUILayout.LabelField("Waves", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serialized.FindProperty("waves"), true);
        DrawProperty(serialized, "clearRewards");
        DrawProperty(serialized, "failureRewards");

        if (serialized.ApplyModifiedProperties())
        {
            EditorUtility.SetDirty(selected);
            status = $"Edited {selected.StageKey}. Save Assets to persist changes.";
        }

        DrawAnalytics(selected, progression);
        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
    }

    private static void DrawProperty(SerializedObject serialized, string name)
    {
        SerializedProperty property = serialized.FindProperty(name);
        if (property != null)
            EditorGUILayout.PropertyField(property, true);
    }

    private void DrawAnalytics(StageDataSO stage, int progression)
    {
        EditorGUILayout.Space(10f);
        EditorGUILayout.LabelField("Wave Analytics", EditorStyles.boldLabel);
        StageBalanceConfigSO config = StageBalanceConfigDatabase.Get();
        float previousThreat = 0f;

        for (int waveIndex = 0; waveIndex < stage.waves.Count; waveIndex++)
        {
            WaveData wave = stage.waves[waveIndex];
            if (wave == null) continue;

            float threat = 0f;
            float totalHp = 0f;
            float totalDps = 0f;
            int totalCount = 0;
            int rangedCount = 0;
            foreach (SubWaveData subWave in wave.subWaves ?? new List<SubWaveData>())
            {
                foreach (MonsterSpawnEntry entry in subWave?.spawnEntries ?? new List<MonsterSpawnEntry>())
                {
                    if (entry?.data == null) continue;
                    StageBalanceMultipliers multipliers = StageBalanceCalculator.Calculate(stage, progression, wave, entry, config);
                    threat += StageBalanceCalculator.CalculateThreat(entry, multipliers);
                    totalHp += entry.data.BaseMaxHp * multipliers.Hp * entry.count;
                    totalDps += entry.data.BaseAttackDamage * entry.data.BaseAttackPerSecond * multipliers.Attack * entry.count;
                    totalCount += entry.count;
                    if (entry.data.BaseAttackRange >= 4f) rangedCount += entry.count;
                }
            }

            float rangedRatio = totalCount > 0 ? (float)rangedCount / totalCount : 0f;
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField($"Wave {waveIndex + 1} · {wave.waveType}", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Count {totalCount}   Total HP {totalHp:0}   Total DPS {totalDps:0.0}   Threat {threat:0}");
            EditorGUILayout.LabelField($"Ranged ratio {rangedRatio:P0}");

            if (totalCount == 0)
                EditorGUILayout.HelpBox("No monsters are configured.", MessageType.Error);
            if (previousThreat > 0f && threat / previousThreat >= (config != null ? config.threatSpikeWarningRatio : 1.6f))
                EditorGUILayout.HelpBox($"Threat jumps to {threat / previousThreat:0.00}× the previous wave.", MessageType.Warning);
            if (rangedRatio >= (config != null ? config.rangedRatioWarning : 0.6f))
                EditorGUILayout.HelpBox("The ranged monster ratio is high.", MessageType.Warning);
            EditorGUILayout.EndVertical();
            previousThreat = threat;
        }
    }

    private void Reload()
    {
        stages.Clear();
        stages.AddRange(AssetDatabase.FindAssets("t:StageDataSO")
            .Select(guid => AssetDatabase.LoadAssetAtPath<StageDataSO>(AssetDatabase.GUIDToAssetPath(guid)))
            .Where(stage => stage != null)
            .OrderBy(stage => stage.sector)
            .ThenBy(stage => stage.stage));
        if (selected == null || !stages.Contains(selected)) selected = stages.FirstOrDefault();
        status = $"Loaded {stages.Count} stage assets.";
    }

    private void ValidateAll()
    {
        var errors = new List<string>();
        foreach (StageDataSO stage in stages)
            if (!stage.TryValidate(out string error)) errors.Add($"{stage.StageKey}: {error}");
        status = errors.Count == 0 ? $"All {stages.Count} stages are valid." : string.Join("\n", errors);
        if (errors.Count > 0) Debug.LogError(status);
    }

    private void ExportCsv()
    {
        string folder = EditorUtility.OpenFolderPanel("Export stage balance CSV", Application.dataPath, "StageBalanceCsv");
        if (!string.IsNullOrWhiteSpace(folder)) status = StageBalanceCsvUtility.Export(folder, stages);
    }

    private void ImportCsv()
    {
        string folder = EditorUtility.OpenFolderPanel("Import stage balance CSV", Application.dataPath, string.Empty);
        if (string.IsNullOrWhiteSpace(folder)) return;
        if (!EditorUtility.DisplayDialog("Import stage balance", "Existing wave and spawn data for matching stages will be replaced. Continue?", "Import", "Cancel")) return;
        bool success = StageBalanceCsvUtility.TryImport(folder, stages, out status);
        if (!success) EditorUtility.DisplayDialog("CSV import failed", status, "OK");
        Reload();
    }
}
