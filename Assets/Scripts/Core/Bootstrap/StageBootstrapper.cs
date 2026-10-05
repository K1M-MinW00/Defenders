using UnityEngine;

public sealed class StageBootstrapper : MonoBehaviour
{
    [Header("Scene References")]
    [SerializeField] private PlacementController placementController;
    [SerializeField] private EconomyManager economyManager;
    [SerializeField] private GameCameraController gameCameraController;
    [SerializeField] private UnitSummoner unitSummoner;
    [SerializeField] private MonsterSpawner monsterSpawner;

    public bool IsInitialized { get; private set; }

    public bool TryInitializeStage(
        StageDataSO stageData,
        StageEnterData enterData,
        out StageMapContext mapContext,
        out string error)
    {
        mapContext = null;

        if (IsInitialized)
        {
            error = "Stage bootstrapper is already initialized.";
            return false;
        }

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

        if (!TryBuildUnitPool(
                enterData.SelectedUnitIds,
                out System.Collections.Generic.List<StageUnitInitData> combatFormation,
                out error))
            return false;

        if (!TryCreateMap(stageData, out mapContext, out error))
            return false;

        if (!economyManager.Init(stageData.economyConfig))
        {
            RollbackInitialization(ref mapContext);
            error = "Stage economy initialization failed.";
            return false;
        }

        placementController.Initialize(mapContext.PlacementArea);

        unitSummoner.SetMapContext(mapContext.UnitSpawnPoint, mapContext.PlacementArea);
        if (!unitSummoner.SetCombatFormation(combatFormation))
        {
            RollbackInitialization(ref mapContext);
            error = "Runtime unit pool is empty.";
            return false;
        }

        monsterSpawner.SetSpawnPoints(mapContext.MonsterSpawnPoints);
        int progressionIndex = StageBalanceCalculator.GetProgressionIndex(
            stageData,
            GameConfig.Stages.GetAll());
        monsterSpawner.SetStageBalanceContext(stageData, progressionIndex);
        gameCameraController.Initialize(mapContext.MinBound, mapContext.MaxBound);

        IsInitialized = true;
        error = string.Empty;
        return true;
    }

    private void RollbackInitialization(ref StageMapContext mapContext)
    {
        gameCameraController.ClearStageContext();
        monsterSpawner.ClearStageContext();
        unitSummoner.ClearStageContext();
        placementController.ClearStageContext();
        economyManager.ResetRuntime();

        if (mapContext != null)
            Destroy(mapContext.gameObject);

        mapContext = null;
        IsInitialized = false;
    }

    private static bool TryBuildUnitPool(
        System.Collections.Generic.IReadOnlyList<string> selectedUnitIds,
        out System.Collections.Generic.List<StageUnitInitData> definitions,
        out string error)
    {
        definitions = new System.Collections.Generic.List<StageUnitInitData>(selectedUnitIds.Count);
        RosterService rosterService = UserDataManager.Instance?.RosterService;
        if (rosterService == null)
        {
            error = "User roster service is not ready.";
            return false;
        }

        foreach (string unitId in selectedUnitIds)
        {
            UnitDataSO data = GameConfig.Units.Get(unitId);
            UserUnitData userData = rosterService.GetUnit(unitId);
            if (data == null || userData == null || data.unitPrefab == null)
            {
                error = $"Selected unit data is invalid: {unitId}";
                return false;
            }

            definitions.Add(new StageUnitInitData(data, userData));
        }

        error = string.Empty;
        return definitions.Count > 0;
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
        string validationError = string.Empty;

        if (context != null && context.TryValidate(out validationError))
        {
            error = string.Empty;
            return true;
        }

        Destroy(mapInstance);
        if (context == null)
            error = "StageMapContext is missing on the map prefab root.";
        else
            error = $"StageMapContext is invalid: {validationError}";

        context = null;
        return false;
    }
}
