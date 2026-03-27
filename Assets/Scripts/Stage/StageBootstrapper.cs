using UnityEngine;

public class StageBootstrapper : MonoBehaviour
{
    [SerializeField] private EconomyConfig economyConfig;

    [SerializeField] private EconomyManager economyManager;
    [SerializeField] private PopulationManager populationManager;
    [SerializeField] private UnitRoster unitRoster;
    [SerializeField] private UnitRosterHpTracker unitHpTracker;
    [SerializeField] private ObjectPool pool;
    [SerializeField] private MonsterSpawner monsterSpawner;

    public void Initialize()
    {
        economyManager.Init(economyConfig);
        populationManager.Init(unitRoster,economyManager);
        unitHpTracker.Init(unitRoster);
        monsterSpawner.Init(pool,unitRoster);
        DamageUIService.Instance.Init(pool);
    }
}