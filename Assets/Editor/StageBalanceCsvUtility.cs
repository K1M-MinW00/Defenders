using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

public static class StageBalanceCsvUtility
{
    private const string StagesFile = "stages.csv";
    private const string WavesFile = "waves.csv";
    private const string SubWavesFile = "subwaves.csv";
    private const string SpawnsFile = "monster_spawns.csv";

    public static string Export(string folder, IReadOnlyList<StageDataSO> stages)
    {
        Directory.CreateDirectory(folder);
        var stageRows = new List<string[]> { new[] { "stage_id", "sector", "stage", "prepare_duration", "threat_type", "hp_modifier", "attack_modifier", "pressure_modifier", "map_asset", "economy_asset" } };
        var waveRows = new List<string[]> { new[] { "wave_id", "stage_id", "wave_index", "wave_type", "hp_modifier", "attack_modifier", "pressure_modifier" } };
        var subWaveRows = new List<string[]> { new[] { "subwave_id", "wave_id", "subwave_index", "delay_after" } };
        var spawnRows = new List<string[]> { new[] { "spawn_id", "subwave_id", "entry_index", "monster_id", "count", "spawn_point", "interval", "delay_after_group", "hp_modifier", "attack_modifier" } };

        foreach (StageDataSO stage in stages.Where(value => value != null).OrderBy(value => value.sector).ThenBy(value => value.stage))
        {
            string stageId = stage.StageKey;
            stageRows.Add(new[] { stageId, I(stage.sector), I(stage.stage), F(stage.prepareDuration), stage.threatType.ToString(), F(stage.hpMultiplier), F(stage.attackMultiplier), F(stage.pressureMultiplier), AssetDatabase.GetAssetPath(stage.mapPrefab), AssetDatabase.GetAssetPath(stage.economyConfig) });

            for (int waveIndex = 0; waveIndex < stage.waves.Count; waveIndex++)
            {
                WaveData wave = stage.waves[waveIndex];
                string waveId = $"{stageId}:W{waveIndex + 1}";
                waveRows.Add(new[] { waveId, stageId, I(waveIndex), wave.waveType.ToString(), F(wave.hpMultiplier), F(wave.attackMultiplier), F(wave.pressureMultiplier) });

                for (int subIndex = 0; subIndex < wave.subWaves.Count; subIndex++)
                {
                    SubWaveData subWave = wave.subWaves[subIndex];
                    string subWaveId = $"{waveId}:S{subIndex + 1}";
                    subWaveRows.Add(new[] { subWaveId, waveId, I(subIndex), F(subWave.delayAfterSubWave) });

                    for (int entryIndex = 0; entryIndex < subWave.spawnEntries.Count; entryIndex++)
                    {
                        MonsterSpawnEntry entry = subWave.spawnEntries[entryIndex];
                        spawnRows.Add(new[] { $"{subWaveId}:E{entryIndex + 1}", subWaveId, I(entryIndex), entry.data != null ? entry.data.monsterId : string.Empty, I(entry.count), I(entry.spawnPointIndex), F(entry.interval), F(entry.delayAfterGroup), F(entry.hpMultiplier), F(entry.attackMultiplier) });
                    }
                }
            }
        }

        Write(Path.Combine(folder, StagesFile), stageRows);
        Write(Path.Combine(folder, WavesFile), waveRows);
        Write(Path.Combine(folder, SubWavesFile), subWaveRows);
        Write(Path.Combine(folder, SpawnsFile), spawnRows);
        return $"Exported {stages.Count} stages to {folder}";
    }

    public static bool TryImport(string folder, IReadOnlyList<StageDataSO> existingStages, out string report)
    {
        try
        {
            CsvTable stageTable = Read(Path.Combine(folder, StagesFile));
            CsvTable waveTable = Read(Path.Combine(folder, WavesFile));
            CsvTable subTable = Read(Path.Combine(folder, SubWavesFile));
            CsvTable spawnTable = Read(Path.Combine(folder, SpawnsFile));
            Dictionary<string, StageDataSO> stages = existingStages.Where(stage => stage != null).ToDictionary(stage => stage.StageKey, StringComparer.Ordinal);
            Dictionary<string, MonsterDataSO> monsters = FindAssets<MonsterDataSO>().Where(data => !string.IsNullOrWhiteSpace(data.monsterId)).ToDictionary(data => data.monsterId, StringComparer.Ordinal);

            var stageModels = new Dictionary<string, StageModel>(StringComparer.Ordinal);
            foreach (string[] row in stageTable.Rows)
            {
                string id = stageTable.Value(row, "stage_id");
                if (!stages.ContainsKey(id))
                    throw new InvalidDataException($"Unknown stage_id: {id}. Create the StageDataSO first.");
                stageModels[id] = new StageModel
                {
                    Id = id,
                    PrepareDuration = ParseFloat(stageTable.Value(row, "prepare_duration")),
                    ThreatType = ParseEnum<StageThreatType>(stageTable.Value(row, "threat_type")),
                    Hp = ParseFloat(stageTable.Value(row, "hp_modifier")),
                    Attack = ParseFloat(stageTable.Value(row, "attack_modifier")),
                    Pressure = ParseFloat(stageTable.Value(row, "pressure_modifier"))
                };
            }

            var waves = new Dictionary<string, WaveModel>(StringComparer.Ordinal);
            foreach (string[] row in waveTable.Rows)
            {
                string id = waveTable.Value(row, "wave_id");
                string stageId = waveTable.Value(row, "stage_id");
                if (!stageModels.ContainsKey(stageId)) throw new InvalidDataException($"Unknown stage_id in waves.csv: {stageId}");
                var model = new WaveModel { Id = id, Index = ParseInt(waveTable.Value(row, "wave_index")), Type = ParseEnum<WaveType>(waveTable.Value(row, "wave_type")), Hp = ParseFloat(waveTable.Value(row, "hp_modifier")), Attack = ParseFloat(waveTable.Value(row, "attack_modifier")), Pressure = ParseFloat(waveTable.Value(row, "pressure_modifier")) };
                waves.Add(id, model);
                stageModels[stageId].Waves.Add(model);
            }

            var subWaves = new Dictionary<string, SubWaveModel>(StringComparer.Ordinal);
            foreach (string[] row in subTable.Rows)
            {
                string id = subTable.Value(row, "subwave_id");
                string waveId = subTable.Value(row, "wave_id");
                if (!waves.TryGetValue(waveId, out WaveModel wave)) throw new InvalidDataException($"Unknown wave_id in subwaves.csv: {waveId}");
                var model = new SubWaveModel { Index = ParseInt(subTable.Value(row, "subwave_index")), Delay = ParseFloat(subTable.Value(row, "delay_after")) };
                subWaves.Add(id, model);
                wave.SubWaves.Add(model);
            }

            foreach (string[] row in spawnTable.Rows)
            {
                string subWaveId = spawnTable.Value(row, "subwave_id");
                string monsterId = spawnTable.Value(row, "monster_id");
                if (!subWaves.TryGetValue(subWaveId, out SubWaveModel subWave)) throw new InvalidDataException($"Unknown subwave_id in monster_spawns.csv: {subWaveId}");
                if (!monsters.TryGetValue(monsterId, out MonsterDataSO monster)) throw new InvalidDataException($"Unknown monster_id: {monsterId}");
                subWave.Entries.Add(new SpawnModel { Index = ParseInt(spawnTable.Value(row, "entry_index")), Monster = monster, Count = ParseInt(spawnTable.Value(row, "count")), SpawnPoint = ParseInt(spawnTable.Value(row, "spawn_point")), Interval = ParseFloat(spawnTable.Value(row, "interval")), Delay = ParseFloat(spawnTable.Value(row, "delay_after_group")), Hp = ParseFloat(spawnTable.Value(row, "hp_modifier")), Attack = ParseFloat(spawnTable.Value(row, "attack_modifier")) });
            }

            foreach (StageModel model in stageModels.Values)
            {
                StageDataSO stage = stages[model.Id];
                Undo.RecordObject(stage, "Import stage balance CSV");
                stage.prepareDuration = Mathf.Max(0f, model.PrepareDuration);
                stage.threatType = model.ThreatType;
                stage.hpMultiplier = Positive(model.Hp);
                stage.attackMultiplier = Positive(model.Attack);
                stage.pressureMultiplier = Positive(model.Pressure);
                stage.waves = model.Waves.OrderBy(wave => wave.Index).Select(ToWaveData).ToList();
                EditorUtility.SetDirty(stage);
            }

            AssetDatabase.SaveAssets();
            report = $"Imported {stageModels.Count} stages, {waves.Count} waves, {subWaves.Count} sub-waves, and {spawnTable.Rows.Count} spawn entries.";
            return true;
        }
        catch (Exception exception)
        {
            report = exception.Message;
            return false;
        }
    }

    private static WaveData ToWaveData(WaveModel model) => new()
    {
        waveType = model.Type,
        hpMultiplier = Positive(model.Hp),
        attackMultiplier = Positive(model.Attack),
        pressureMultiplier = Positive(model.Pressure),
        subWaves = model.SubWaves.OrderBy(sub => sub.Index).Select(sub => new SubWaveData
        {
            delayAfterSubWave = Mathf.Max(0f, sub.Delay),
            spawnEntries = sub.Entries.OrderBy(entry => entry.Index).Select(entry => new MonsterSpawnEntry
            {
                data = entry.Monster, count = entry.Count, spawnPointIndex = entry.SpawnPoint,
                interval = entry.Interval, delayAfterGroup = entry.Delay,
                hpMultiplier = Positive(entry.Hp), attackMultiplier = Positive(entry.Attack)
            }).ToList()
        }).ToList()
    };

    private static IEnumerable<T> FindAssets<T>() where T : UnityEngine.Object => AssetDatabase.FindAssets($"t:{typeof(T).Name}").Select(guid => AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid)));
    private static string I(int value) => value.ToString(CultureInfo.InvariantCulture);
    private static string F(float value) => value.ToString("0.####", CultureInfo.InvariantCulture);
    private static int ParseInt(string value) => int.Parse(value, NumberStyles.Integer, CultureInfo.InvariantCulture);
    private static float ParseFloat(string value) => float.Parse(value, NumberStyles.Float, CultureInfo.InvariantCulture);
    private static T ParseEnum<T>(string value) where T : struct => Enum.Parse<T>(value, true);
    private static float Positive(float value) => value > 0f ? value : 1f;

    private static void Write(string path, IEnumerable<string[]> rows)
    {
        File.WriteAllLines(path, rows.Select(row => string.Join(",", row.Select(Escape))), new UTF8Encoding(true));
    }

    private static string Escape(string value)
    {
        value ??= string.Empty;
        return value.IndexOfAny(new[] { ',', '"', '\n', '\r' }) >= 0 ? $"\"{value.Replace("\"", "\"\"")}\"" : value;
    }

    private static CsvTable Read(string path)
    {
        if (!File.Exists(path)) throw new FileNotFoundException($"Required CSV is missing: {Path.GetFileName(path)}", path);
        string[][] rows = File.ReadAllLines(path).Where(line => !string.IsNullOrWhiteSpace(line)).Select(ParseLine).ToArray();
        if (rows.Length == 0) throw new InvalidDataException($"CSV is empty: {path}");
        return new CsvTable(rows[0], rows.Skip(1).ToList());
    }

    private static string[] ParseLine(string line)
    {
        var values = new List<string>();
        var value = new StringBuilder();
        bool quoted = false;
        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];
            if (c == '"' && quoted && i + 1 < line.Length && line[i + 1] == '"') { value.Append('"'); i++; }
            else if (c == '"') quoted = !quoted;
            else if (c == ',' && !quoted) { values.Add(value.ToString()); value.Clear(); }
            else value.Append(c);
        }
        values.Add(value.ToString());
        return values.ToArray();
    }

    private sealed class CsvTable
    {
        private readonly Dictionary<string, int> columns;
        public readonly List<string[]> Rows;
        public CsvTable(string[] headers, List<string[]> rows) { columns = headers.Select((name, index) => (name, index)).ToDictionary(pair => pair.name, pair => pair.index, StringComparer.OrdinalIgnoreCase); Rows = rows; }
        public string Value(string[] row, string name) { if (!columns.TryGetValue(name, out int index) || index >= row.Length) throw new InvalidDataException($"Missing CSV column: {name}"); return row[index]; }
    }

    private sealed class StageModel { public string Id; public float PrepareDuration; public StageThreatType ThreatType; public float Hp; public float Attack; public float Pressure; public readonly List<WaveModel> Waves = new(); }
    private sealed class WaveModel { public string Id; public int Index; public WaveType Type; public float Hp; public float Attack; public float Pressure; public readonly List<SubWaveModel> SubWaves = new(); }
    private sealed class SubWaveModel { public int Index; public float Delay; public readonly List<SpawnModel> Entries = new(); }
    private sealed class SpawnModel { public int Index; public MonsterDataSO Monster; public int Count; public int SpawnPoint; public float Interval; public float Delay; public float Hp; public float Attack; }
}
