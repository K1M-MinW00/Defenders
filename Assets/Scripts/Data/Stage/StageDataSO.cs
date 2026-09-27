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

    [Header("Clear Rewards")]
    public List<RewardData> clearRewards = new();

    [Header("Failure Rewards")]
    public List<RewardData> failureRewards = new();

    public string StageKey => $"{sector}-{stage}";

    public bool TryValidate(out string error)
    {
        if (sector < 1)
            return Fail("Sector must be at least 1.", out error);
        if (stage < 1 || stage > StageProgressRules.StagesPerSector)
            return Fail($"Stage must be between 1 and {StageProgressRules.StagesPerSector}.", out error);
        if (mapPrefab == null)
            return Fail("Map prefab is missing.", out error);
        if (!mapPrefab.TryGetComponent(out StageMapContext mapContext))
            return Fail("Map prefab does not contain StageMapContext on its root.", out error);
        if (mapContext.MinBound == null || mapContext.MaxBound == null)
            return Fail("Map context camera bounds are missing.", out error);
        if (mapContext.UnitSpawnPoint == null)
            return Fail("Map context unit spawn point is missing.", out error);
        if (mapContext.PlacementArea == null)
            return Fail("Map context placement area is missing.", out error);
        if (economyConfig == null)
            return Fail("Economy config is missing.", out error);
        if (clearRewards == null || clearRewards.Count == 0)
            return Fail("Stage clear rewards are missing.", out error);
        if (!TryValidateRewards(clearRewards, "Clear", out error))
            return false;
        if (failureRewards == null || failureRewards.Count == 0)
            return Fail("Stage failure rewards are missing.", out error);
        if (!TryValidateRewards(failureRewards, "Failure", out error))
            return false;
        if (waves == null || (waves.Count != 3 && waves.Count != 5 && waves.Count != 10))
            return Fail("Wave count must be 3, 5, or 10.", out error);
        if (waves[^1] == null || waves[^1].waveType != WaveType.Boss)
            return Fail("The final wave must be a boss wave.", out error);

        int spawnPointCount = mapContext.MonsterSpawnPoints?.Length ?? 0;
        if (spawnPointCount == 0)
            return Fail("Map context has no monster spawn points.", out error);

        for (int waveIndex = 0; waveIndex < waves.Count; waveIndex++)
        {
            WaveData wave = waves[waveIndex];
            if (wave == null)
                return Fail($"Wave {waveIndex + 1} is null.", out error);
            if (waveIndex < waves.Count - 1 && wave.waveType == WaveType.Boss)
                return Fail($"Wave {waveIndex + 1} is a boss wave before the final wave.", out error);
            if (wave.subWaves == null || wave.subWaves.Count == 0)
                return Fail($"Wave {waveIndex + 1} has no sub-waves.", out error);

            for (int subWaveIndex = 0; subWaveIndex < wave.subWaves.Count; subWaveIndex++)
            {
                SubWaveData subWave = wave.subWaves[subWaveIndex];
                if (subWave == null)
                    return Fail($"Wave {waveIndex + 1}, sub-wave {subWaveIndex + 1} is null.", out error);
                if (subWave.delayAfterSubWave < 0f)
                    return Fail($"Wave {waveIndex + 1}, sub-wave {subWaveIndex + 1} has a negative delay.", out error);
                if (subWave.spawnEntries == null || subWave.spawnEntries.Count == 0)
                    return Fail($"Wave {waveIndex + 1}, sub-wave {subWaveIndex + 1} has no spawn entries.", out error);

                for (int entryIndex = 0; entryIndex < subWave.spawnEntries.Count; entryIndex++)
                {
                    MonsterSpawnEntry entry = subWave.spawnEntries[entryIndex];
                    string location = $"Wave {waveIndex + 1}, sub-wave {subWaveIndex + 1}, entry {entryIndex + 1}";
                    if (entry == null)
                        return Fail($"{location} is null.", out error);
                    if (entry.data == null)
                        return Fail($"{location} has no monster data.", out error);
                    if (entry.count <= 0)
                        return Fail($"{location} must spawn at least one monster.", out error);
                    if (entry.spawnPointIndex < 0 || entry.spawnPointIndex >= spawnPointCount)
                        return Fail($"{location} uses invalid spawn point {entry.spawnPointIndex}.", out error);
                    if (entry.interval < 0f || entry.delayAfterGroup < 0f)
                        return Fail($"{location} has a negative interval or delay.", out error);
                }
            }
        }

        error = string.Empty;
        return true;
    }

    private static bool TryValidateRewards(List<RewardData> rewards, string label, out string error)
    {
        for (int rewardIndex = 0; rewardIndex < rewards.Count; rewardIndex++)
        {
            RewardData reward = rewards[rewardIndex];
            if (reward == null || reward.Amount <= 0)
                return Fail($"{label} reward {rewardIndex + 1} is invalid.", out error);
            if ((reward.Type is RewardType.Item or RewardType.Equipment or RewardType.Unit) &&
                string.IsNullOrWhiteSpace(reward.Id))
                return Fail($"{label} reward {rewardIndex + 1} requires an ID.", out error);
        }

        error = string.Empty;
        return true;
    }

    private static bool Fail(string message, out string error)
    {
        error = message;
        return false;
    }
}
