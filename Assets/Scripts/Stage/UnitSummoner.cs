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
            UnitDataSO data = UnitMasterDataManager.Instance.GetUnitData(unitId);

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
        GameObject go = Instantiate(data.unitPrefab, pos, Quaternion.identity, unitsRoot);

        UnitController unit = go.GetComponent<UnitController>();

        UserUnitData userData = FindUserUnitData(data);
        StageUnitInitData initData = new StageUnitInitData(data, userData, 1);

        unit.BindCombatContext(monsterSpawner, unitRoster, poolManager);
        unit.Initialize(initData);
        unit.SetCombatPhase(false);

        unitRoster?.Register(unit);
        fusionService?.TryAutoFuse(unit);

        return true;
    }

    private UserUnitData FindUserUnitData(UnitDataSO data)
    {
        UserDataRoot userDataRoot = UserDataManager.Instance.Data;

        if (userDataRoot == null || userDataRoot.Roster == null)
        {
            Debug.LogWarning("UserDataRoot or Roster is null. Temporary UserUnitData will be used.");
            return new UserUnitData(data.unitId);
        }

        foreach (UserUnitData userUnit in userDataRoot.Roster.OwnedUnits)
        {
            if (userUnit.UnitId == data.unitId)
                return userUnit;
        }

        Debug.LogWarning($"UserUnitData not found. UnitId: {data.unitId}");
        return new UserUnitData(data.unitId);
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
