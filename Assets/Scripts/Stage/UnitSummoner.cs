using UnityEngine;

public class UnitSummoner : MonoBehaviour
{
    [Header("Unit Pool (Inspector)")]
    [SerializeField] private UnitDataSO[] unitPool;

    [Header("Spawn Settings")]
    [SerializeField] private Transform unitsRoot;
    [SerializeField] private MonsterSpawner monsterSpawner;
    [SerializeField] private UnitRoster roster;
    [SerializeField] private FusionService fusionService;
    
    [SerializeField] private TilemapPlacementArea placementArea;
    [SerializeField] private Transform spawnPoint;

    public bool SummonRandomUnit()
    {
        if (unitPool == null || unitPool.Length == 0)
        {
            Debug.LogWarning("Summon blocked: unitPool is empty.");
            return false;
        }

        UnitDataSO data = unitPool[Random.Range(0, unitPool.Length)];

        if (data == null || data.UnitPrefab == null)
            return false;

        Vector3 pos = ResolveSpawnPosition();
        GameObject go = Instantiate(data.UnitPrefab, pos, Quaternion.identity, unitsRoot);

        UnitController unit = go.GetComponent<UnitController>();

        // TODO : UserUnitData 정보 받아오기
        UserUnitData userData = new UserUnitData(data.UnitCode);
        StageUnitInitData initData = new StageUnitInitData(data, userData, 1);

        unit.BindCombatContext(monsterSpawner);
        unit.Initialize(initData);
        unit.SetCombatPhase(false);
        
        roster?.Register(unit);
        fusionService?.TryAutoFuse(unit);

        return true;
    }

    private Vector3 ResolveSpawnPosition()
    {
        if (spawnPoint != null)
            return spawnPoint.position;

        if (placementArea != null)
            return placementArea.transform.position;

        return transform.position;
    }
}
