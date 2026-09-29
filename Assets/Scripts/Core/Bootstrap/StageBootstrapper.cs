using UnityEngine;

public class StageBootstrapper : MonoBehaviour
{
    [Header("Scene References")]
    [SerializeField] private PlacementController placementController;
    [SerializeField] private EconomyManager economyManager;
    [SerializeField] private GameCameraController gameCameraController;
    [SerializeField] private UnitSummoner unitSummoner;
    [SerializeField] private MonsterSpawner monsterSpawner;


    public bool TryInitializeStage(
        StageDataSO stageData,
        StageEnterData enterData,
        out StageMapContext mapContext,
        out string error)
    {
        mapContext = null;

        if (stageData == null || enterData == null)
        {
            error = "Stage data or entry data is missing.";
            return false;
        }

        if (placementController == null || economyManager == null ||
            gameCameraController == null || unitSummoner == null || monsterSpawner == null)
        {
            error = "One or more required scene references are missing.";
            return false;
        }

        if (enterData.SelectedUnitIds == null || enterData.SelectedUnitIds.Count == 0)
        {
            error = "Selected combat units are missing.";
            return false;
        }

        if (!TryCreateMap(stageData, out mapContext, out error))
            return false;

        economyManager.Init(stageData.economyConfig);
        placementController.Initialize(mapContext.PlacementArea);

        unitSummoner.SetMapContext(mapContext.UnitSpawnPoint, mapContext.PlacementArea);
        unitSummoner.SetUnitPool(enterData.SelectedUnitIds);

        monsterSpawner.SetSpawnPoints(mapContext.MonsterSpawnPoints);
        gameCameraController.Initialize(mapContext.MinBound, mapContext.MaxBound);

        error = string.Empty;
        return true;
    }

    private static bool TryCreateMap(
        StageDataSO stageData,
        out StageMapContext context,
        out string error)
    {
        context = null;

        if (stageData.mapPrefab == null)
        {
            error = "Map prefab is missing.";
            return false;
        }

        GameObject mapInstance = Instantiate(stageData.mapPrefab);
        context = mapInstance.GetComponent<StageMapContext>();

        if (context != null)
        {
            error = string.Empty;
            return true;
        }

        Destroy(mapInstance);
        error = "StageMapContext is missing on the map prefab root.";
        return false;
    }
}
