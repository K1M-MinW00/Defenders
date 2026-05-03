using UnityEngine;

public class StageBootstrapper : MonoBehaviour
{
    [SerializeField] private EconomyConfig economyConfig;

    [SerializeField] private EconomyManager economyManager;
    [SerializeField] private PopulationManager populationManager;
    [SerializeField] private StagePoolManager poolManager;

    [SerializeField] private UnitSummoner unitSummoner;
    [SerializeField] private UnitRoster unitRoster;
    [SerializeField] private UnitRosterHpTracker unitHpTracker;
    [SerializeField] private FusionService fusionService;
    
    [SerializeField] private MonsterSpawner monsterSpawner;
    [SerializeField] private MonsterWaveHpTracker monsterHpTracker;
    [SerializeField] private DamageUIService damageUIService;

    public void Initialize()
    {
        economyManager.Init(economyConfig);
        monsterSpawner.Init(poolManager, unitRoster, monsterHpTracker, damageUIService);
        unitSummoner.Init(poolManager, unitRoster, fusionService, monsterSpawner);
        populationManager.Init(unitRoster,economyManager);
        unitHpTracker.Init(unitRoster);
        
    }
}