using UnityEngine;

public class StageBootstrapper : MonoBehaviour
{
    [Header("Scene References")]
    [SerializeField] private PlacementController placementController;
    [SerializeField] private EconomyManager economyManager;
    [SerializeField] private GameCameraController gameCameraController;
    [SerializeField] private UnitSummoner unitSummoner;
    [SerializeField] private MonsterSpawner monsterSpawner;


    public StageMapContext InitializeStage(StageDataSO stageData, StageEnterData enterData)
    {
        if(stageData == null)
        {
            Debug.LogError("StageBootstrapper. InitializeStage Failed");
            return null;
        }

        economyManager.Init(stageData.economyConfig);
        StageMapContext mapContext = CreateMap(stageData);

        placementController.Initialize(mapContext.PlacementArea);

        unitSummoner.SetMapContext(mapContext.UnitSpawnPoint, mapContext.PlacementArea);
        unitSummoner.SetUnitPool(enterData.SelectedUnitIds);

        monsterSpawner.SetSpawnPoints(mapContext.MonsterSpawnPoints);
        gameCameraController.Initialize(mapContext.MinBound, mapContext.MaxBound);

        return mapContext;
    }

    private StageMapContext CreateMap(StageDataSO stageData)
    {
        GameObject mapPrefab = stageData.mapPrefab;
        GameObject mapInstance = Instantiate(mapPrefab);

        StageMapContext context = mapInstance.GetComponent<StageMapContext>();

        if (context == null)
            throw new System.Exception("StageMapContext is missing on map prefab.");

        return context;
    }
}