using System.Collections.Generic;
using UnityEngine;

public class UnitSummoner : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private StagePoolManager poolManager;
    [SerializeField] private UnitRoster unitRoster;
    [SerializeField] private FusionService fusionService;
    [SerializeField] private MonsterSpawner monsterSpawner;
    [SerializeField] private Transform unitsRoot;
    
    [Header("Unit Pool (Inspector)")]
    [SerializeField] private UnitDataSO[] unitPool;

    [Header("Spawn Settings")]
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private TilemapPlacementArea placementArea;
    [SerializeField] private float spawnRadius = 2.5f;


    public void SetMapContext(Transform unitSpawnPoint, TilemapPlacementArea placementArea)
    {
        this.spawnPoint = unitSpawnPoint;
        this.placementArea = placementArea;
    }

    public void SetUnitPool(IReadOnlyList<string> selectedUnitIds)
    {
        List<UnitDataSO> result = new();

        foreach (string unitId in selectedUnitIds)
        {
            UnitDataSO data = GameConfig.Units.Get(unitId);

            if (data != null)
                result.Add(data);
        }

        unitPool = result.ToArray();
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
        Poolable spawned = poolManager.Spawn(
            data.unitPrefab,
            pos,
            Quaternion.identity,
            PoolCategory.Unit,
            unitsRoot);

        if (spawned == null || !spawned.TryGetComponent(out UnitController unit))
        {
            Debug.LogError($"Unit spawn failed: UnitController is missing on {data.unitPrefab.name}.");

            if (spawned != null)
                poolManager.Despawn(spawned);

            return false;
        }

        UserUnitData userData = FindUserUnitData(data);
        StageUnitInitData initData = new StageUnitInitData(data, userData, 1);

        unit.BindCombatContext(monsterSpawner, unitRoster, poolManager);
        if (!unit.Initialize(initData))
        {
            if (unit.TryBeginRemoval(UnitRemovalReason.Rerolled))
                unit.ReturnToPool();
            else
                poolManager.Despawn(spawned);

            return false;
        }

        unitRoster?.Register(unit);
        fusionService?.TryAutoFuse(unit);

        return true;
    }

    private UserUnitData FindUserUnitData(UnitDataSO data)
    {
        UserDataRoot userDataRoot = UserDataManager.Instance.UserData;

        return UserDataManager.Instance.RosterService.GetUnit(data.unitId);
    }

    private Vector3 ResolveSpawnPosition()
    {
        const int maxAttempts = 20;

        for (int i = 0; i < maxAttempts; i++)
        {
            Vector2 offset = Random.insideUnitCircle * spawnRadius;
            Vector3 pos = spawnPoint.position + (Vector3)offset;

            if (placementArea == null || placementArea.CanPlace(pos))
                return pos;
        }

        return spawnPoint.position;
    }
}
