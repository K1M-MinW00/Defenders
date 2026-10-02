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
    private StageUnitInitData[] runtimeUnitPool;

    [Header("Spawn Settings")]
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private TilemapPlacementArea placementArea;
    [SerializeField] private float spawnRadius = 2.5f;


    public void SetMapContext(Transform unitSpawnPoint, TilemapPlacementArea placementArea)
    {
        this.spawnPoint = unitSpawnPoint;
        this.placementArea = placementArea;
    }

    public void ClearStageContext()
    {
        spawnPoint = null;
        placementArea = null;
        unitPool = System.Array.Empty<UnitDataSO>();
        runtimeUnitPool = System.Array.Empty<StageUnitInitData>();
    }

    public bool SetUnitPool(IReadOnlyList<StageUnitInitData> definitions)
    {
        if (definitions == null || definitions.Count == 0)
        {
            unitPool = System.Array.Empty<UnitDataSO>();
            runtimeUnitPool = System.Array.Empty<StageUnitInitData>();
            return false;
        }

        List<UnitDataSO> dataList = new(definitions.Count);
        List<StageUnitInitData> runtimeList = new(definitions.Count);
        foreach (StageUnitInitData definition in definitions)
        {
            if (definition?.UnitData == null || definition.UserData == null)
                continue;

            dataList.Add(definition.UnitData);
            runtimeList.Add(definition);
        }

        unitPool = dataList.ToArray();
        runtimeUnitPool = runtimeList.ToArray();
        return runtimeUnitPool.Length > 0;
    }

    public bool SummonRandomUnit()
    {
        if (!TryCreateRandomUnit(out UnitController unit))
            return false;

        CommitSummonedUnit(unit);
        return true;
    }

    public bool TryCreateRandomUnit(
        out UnitController unit,
        int initialStar = 1,
        Vector3? requestedPosition = null)
    {
        unit = null;

        if (runtimeUnitPool == null || runtimeUnitPool.Length == 0)
        {
            Debug.LogWarning("Summon blocked: unitPool is empty.");
            return false;
        }

        StageUnitInitData definition = runtimeUnitPool[Random.Range(0, runtimeUnitPool.Length)];
        UnitDataSO data = definition.UnitData;

        if (data == null || data.unitPrefab == null)
            return false;

        Vector3 pos = requestedPosition ?? ResolveSpawnPosition();
        Poolable spawned = poolManager.Spawn(
            data.unitPrefab,
            pos,
            Quaternion.identity,
            PoolCategory.Unit,
            unitsRoot);

        if (spawned == null || !spawned.TryGetComponent(out unit))
        {
            Debug.LogError($"Unit spawn failed: UnitController is missing on {data.unitPrefab.name}.");

            if (spawned != null)
                poolManager.Despawn(spawned);

            return false;
        }

        StageUnitInitData initData = new StageUnitInitData(data, definition.UserData, initialStar);

        unit.BindCombatContext(monsterSpawner, unitRoster, poolManager);
        if (!unit.Initialize(initData))
        {
            if (unit.TryBeginRemoval(UnitRemovalReason.Rerolled))
                unit.ReturnToPool();
            else
                poolManager.Despawn(spawned);

            return false;
        }

        unit.GetComponent<DropSpawnView>()?.Replay();

        return true;
    }

    public FusionResult CommitSummonedUnit(UnitController unit)
    {
        if (unit == null)
            return FusionResult.None;

        unitRoster?.Register(unit);
        fusionService?.BeginAutoFuse(unit);
        return FusionResult.WithoutFusion(unit);
    }

    public void DiscardCreatedUnit(UnitController unit)
    {
        if (unit == null)
            return;

        if (unit.TryBeginRemoval(UnitRemovalReason.Rerolled))
            unit.ReturnToPool();
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
