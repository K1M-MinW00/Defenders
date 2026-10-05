using System.Collections;
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
    
    private StageUnitInitData[] runtimeCombatFormation;

    public IReadOnlyList<StageUnitInitData> RuntimeCombatFormation =>
        runtimeCombatFormation ?? System.Array.Empty<StageUnitInitData>();

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
        runtimeCombatFormation = System.Array.Empty<StageUnitInitData>();
    }

    public bool SetCombatFormation(IReadOnlyList<StageUnitInitData> definitions)
    {
        if (definitions == null || definitions.Count == 0)
        {
            runtimeCombatFormation = System.Array.Empty<StageUnitInitData>();
            return false;
        }

        List<StageUnitInitData> runtimeList = new(definitions.Count);
        foreach (StageUnitInitData definition in definitions)
        {
            if (definition?.UnitData == null || definition.UserData == null)
                continue;

            runtimeList.Add(definition);
        }

        runtimeCombatFormation = runtimeList.ToArray();
        return runtimeCombatFormation.Length > 0;
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
        Vector3? requestedPosition = null,
        bool playSpawnPresentation = true)
    {
        unit = null;

        if (runtimeCombatFormation == null || runtimeCombatFormation.Length == 0)
        {
            Debug.LogWarning("Summon blocked: runtime combat formation is empty.");
            return false;
        }

        StageUnitInitData definition = runtimeCombatFormation[Random.Range(0, runtimeCombatFormation.Length)];
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

        DropSpawnView spawnView = unit.GetComponentInChildren<DropSpawnView>(true);
        if (playSpawnPresentation)
            spawnView?.Replay();
        else
            spawnView?.PrepareDeferredSpawn();

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

    public void CommitRerolledUnit(UnitController outgoing, UnitController replacement)
    {
        if (outgoing == null || replacement == null)
            return;

        replacement.SetInteractionLocked(true);
        replacement.GetComponent<UnitHUDController>()?.SetTransitionHidden(true);
        StartCoroutine(RerollPresentationRoutine(outgoing, replacement));
    }

    private IEnumerator RerollPresentationRoutine(UnitController outgoing, UnitController replacement)
    {
        bool exitComplete = false;
        DropSpawnView outgoingView = outgoing.GetComponentInChildren<DropSpawnView>(true);
        if (outgoingView != null)
        {
            outgoing.GetComponent<UnitHUDController>()?.SetTransitionHidden(true);
            outgoingView.PlayRerollExit(() => exitComplete = true);
            while (!exitComplete && outgoing != null && outgoing.gameObject.activeInHierarchy)
                yield return null;
        }

        if (outgoing != null)
            outgoing.ReturnToPool();

        if (replacement == null || !replacement.gameObject.activeInHierarchy)
            yield break;

        replacement.SetInteractionLocked(false);
        replacement.GetComponent<UnitHUDController>()?.SetTransitionHidden(false);
        replacement.GetComponentInChildren<DropSpawnView>(true)?.Replay();
        CommitSummonedUnit(replacement);
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
