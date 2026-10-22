using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

public static class BossBalanceAudit
{
    [MenuItem("Tools/Game/Bosses/Print Boss Balance Report")]
    public static void PrintReport()
    {
        StageDataSO[] stages = AssetDatabase.FindAssets("t:StageDataSO", new[] { "Assets/Resources/GameData/Stages" })
            .Select(AssetDatabase.GUIDToAssetPath)
            .Select(AssetDatabase.LoadAssetAtPath<StageDataSO>)
            .Where(stage => stage != null)
            .OrderBy(stage => stage.sector)
            .ThenBy(stage => stage.stage)
            .ToArray();

        StringBuilder report = new StringBuilder("[Boss Balance] Basic attacks only; AoE and summon pressure are excluded.\n");
        for (int progressionIndex = 0; progressionIndex < stages.Length; progressionIndex++)
        {
            StageDataSO stage = stages[progressionIndex];
            if (stage.waves == null)
                continue;

            foreach (WaveData wave in stage.waves.Where(candidate => candidate?.waveType == WaveType.Boss))
            {
                foreach (MonsterSpawnEntry entry in FindBossEntries(wave))
                {
                    if (!BossEncounterBalanceCalculator.TryCalculate(
                            stage,
                            progressionIndex,
                            wave,
                            entry,
                            out BossEncounterBalanceSnapshot snapshot))
                        continue;

                    BossDefinitionSO definition = entry.data.BossDefinition;
                    report.AppendLine(
                        $"{stage.StageKey} {entry.data.displayName}: " +
                        $"HP {snapshot.RawHp:0.#} + Shield {snapshot.ShieldHp:0.#} = EHP {snapshot.EffectiveHp:0.#}, " +
                        $"DPS {snapshot.NormalAttackDps:0.##} -> Enraged {snapshot.EnragedAttackDps:0.##}, " +
                        $"phases {snapshot.NormalPhaseDuration:0.#}s/{snapshot.EnragePhaseDuration:0.#}s, " +
                        $"pre-enrage clear DPS {snapshot.RequiredDpsToClearBeforeEnrage:0.##}, " +
                        $"AoE yes, Summon {(definition.summon != null && definition.summon.enabled ? "yes" : "no")}");
                }
            }
        }

        Debug.Log(report.ToString());
    }

    private static System.Collections.Generic.IEnumerable<MonsterSpawnEntry> FindBossEntries(WaveData wave)
    {
        return wave.subWaves == null
            ? Enumerable.Empty<MonsterSpawnEntry>()
            : wave.subWaves
                .Where(subWave => subWave?.spawnEntries != null)
                .SelectMany(subWave => subWave.spawnEntries)
                .Where(entry => entry?.data?.Classification == MonsterClassification.Boss);
    }
}
