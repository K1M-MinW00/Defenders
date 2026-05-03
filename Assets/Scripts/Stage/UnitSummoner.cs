using UnityEngine;

public class UnitSummoner : MonoBehaviour
{
    [Header("Unit Pool (Inspector)")]
    [SerializeField] private UnitDataSO[] unitPool;

    [Header("Spawn Settings")]
    [SerializeField] private Transform unitsRoot;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private float spawnRadius = 2.5f;
    [SerializeField] private TilemapPlacementArea placementArea;

    private StagePoolManager poolManager;
    private UnitRoster unitRoster;
    private FusionService fusionService;
    private MonsterSpawner monsterSpawner;

    public void Init(StagePoolManager poolManager, UnitRoster unitRoster, FusionService fusionService, MonsterSpawner monsterSpawner)
    {
        this.poolManager = poolManager;
        this.unitRoster = unitRoster;
        this.fusionService = fusionService;
        this.monsterSpawner = monsterSpawner;
    }

    public bool SummonRandomUnit()
    {
        if (unitPool == null || unitPool.Length == 0)
        {
            Debug.LogWarning("Summon blocked: unitPool is empty.");
            return false;
        }

        UnitDataSO data = unitPool[Random.Range(0, unitPool.Length)];

        if (data == null || data.unitPrefab == null)
            return false;

        Vector3 pos = ResolveSpawnPosition();
        GameObject go = Instantiate(data.unitPrefab, pos, Quaternion.identity, unitsRoot);

        UnitController unit = go.GetComponent<UnitController>();

        // TODO : UserUnitData 정보 받아와 유닛 기본 스탯 계산 및 생성
        UserUnitData userData = new UserUnitData(data.unitCode);
        StageUnitInitData initData = new StageUnitInitData(data, userData, 1);

        unit.BindCombatContext(monsterSpawner, unitRoster, poolManager);
        unit.Initialize(initData);
        unit.SetCombatPhase(false);

        unitRoster?.Register(unit);
        fusionService?.TryAutoFuse(unit);

        return true;
    }

    private Vector3 ResolveSpawnPosition()
    {
        Vector2 offset = Random.insideUnitCircle * spawnRadius;
        Vector3 pos = spawnPoint.transform.position + (Vector3)offset;
        return pos;
    }
}
