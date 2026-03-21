using System.Collections.Generic;
using UnityEngine;

public class StageBootstrapper : MonoBehaviour
{
    [SerializeField] private EconomyConfig economyConfig;
    [SerializeField] private ObjectPool objectPool;
    [SerializeField] private MonsterSpawner monsterSpawner;
    [SerializeField] private PopulationManager populationManager;
    [SerializeField] private UnitRoster unitRoster;

    public void Initialize(StageData stageData)
    {
        EconomyManager.Instance.Init(economyConfig);
        populationManager.Init();
        monsterSpawner.Init(objectPool,unitRoster);
        DamageUIService.Instance.Init(objectPool);

        PrewarmStageMonsters(stageData);
    }

    private void PrewarmStageMonsters(StageData stageData)
    {
        if (stageData == null)
            return;

        HashSet<string> monsterIds = new();

        foreach (var wave in stageData.waves)
        {
            foreach (var subWave in wave.subWaves)
            {
                foreach (var group in subWave.spawnGroups)
                    monsterIds.Add(group.monsterId);
            }
        }

        foreach (var id in monsterIds)
        {
            GameObject prefab = Resources.Load<GameObject>($"Monsters/{id}");

            if (prefab == null)
            {
                Debug.LogError($"Monster Prefab not found: {id}");
                continue;
            }

            objectPool.Prewarm(id, prefab, 20);
        }
    }
}