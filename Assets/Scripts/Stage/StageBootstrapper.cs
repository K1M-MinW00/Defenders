using System.Collections.Generic;
using UnityEngine;

public class StageBootstrapper : MonoBehaviour
{
    [SerializeField] private EconomyConfig economyConfig;
    [SerializeField] private ObjectPool objectPool;
    [SerializeField] private MonsterSpawner monsterSpawner;
    [SerializeField] private PopulationManager populationManager;
    [SerializeField] private UnitRoster unitRoster;

    public void Initialize()
    {
        EconomyManager.Instance.Init(economyConfig);
        populationManager.Init();
        monsterSpawner.Init(objectPool,unitRoster);
        DamageUIService.Instance.Init(objectPool);
    }
}