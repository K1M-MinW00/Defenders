using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class TutorialSceneSetup
{
    private const string SourceScenePath = "Assets/Scenes/GameScene.unity";
    private const string TutorialScenePath = "Assets/Scenes/TutorialScene.unity";
    private const string StartupScenePath = "Assets/Scenes/StartScene.unity";
    private const string LobbyScenePath = "Assets/Scenes/LobbyScene.unity";
    private const string SourceStagePath = "Assets/Resources/GameData/Stages/Stage_1_1.asset";
    private const string SourceEconomyPath = "Assets/GameData/Economy/Configs/BaseConfig.asset";
    private const string TutorialDataFolder = "Assets/GameData/Tutorial";
    private const string TutorialStagePath = TutorialDataFolder + "/Tutorial_FirstBattle.asset";
    private const string TutorialEconomyPath = TutorialDataFolder + "/Tutorial_Economy.asset";

    [MenuItem("Tools/Tutorial/Create or Repair Tutorial Scene")]
    public static void CreateOrRepair()
    {
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(SourceScenePath) == null)
            throw new System.InvalidOperationException($"Source scene is missing: {SourceScenePath}");

        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(TutorialScenePath) == null &&
            !AssetDatabase.CopyAsset(SourceScenePath, TutorialScenePath))
        {
            throw new System.InvalidOperationException("Failed to create TutorialScene from GameScene.");
        }

        EnsureTutorialDataFolder();
        EconomyConfig tutorialEconomy = CreateOrRepairTutorialEconomy();
        StageDataSO tutorialStage = CreateOrRepairTutorialStage(tutorialEconomy);
        SyncStageFeatures();
        ConfigureTutorialScene(tutorialStage);
        ConfigureLobbyScene();
        ConfigureStartupScene();

        List<EditorBuildSettingsScene> scenes = EditorBuildSettings.scenes.ToList();
        if (scenes.All(scene => scene.path != TutorialScenePath))
        {
            int gameSceneIndex = scenes.FindIndex(scene => scene.path == SourceScenePath);
            int insertIndex = gameSceneIndex >= 0 ? gameSceneIndex + 1 : scenes.Count;
            scenes.Insert(insertIndex, new EditorBuildSettingsScene(TutorialScenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"[{nameof(TutorialSceneSetup)}] Tutorial scene is ready: {TutorialScenePath}");
    }

    private static void SyncStageFeatures()
    {
        BossFeatureSetup.ApplyRuntimeSetup(TutorialScenePath);
        BossHudSetup.Build(TutorialScenePath);
        CombatStatisticsHudSetup.Build(TutorialScenePath);
    }

    private static EconomyConfig CreateOrRepairTutorialEconomy()
    {
        EconomyConfig economy = AssetDatabase.LoadAssetAtPath<EconomyConfig>(TutorialEconomyPath);
        if (economy == null)
        {
            if (!AssetDatabase.CopyAsset(SourceEconomyPath, TutorialEconomyPath))
                throw new System.InvalidOperationException("Failed to create the tutorial EconomyConfig.");
            economy = AssetDatabase.LoadAssetAtPath<EconomyConfig>(TutorialEconomyPath);
        }

        economy.name = "Tutorial_Economy";
        economy.initialGold = 5;
        economy.normalReward = 15;
        if (economy.populationIncreaseCosts is { Length: > 0 })
            economy.populationIncreaseCosts[0] = Mathf.Max(10, economy.populationIncreaseCosts[0]);
        EditorUtility.SetDirty(economy);
        return economy;
    }

    private static void EnsureTutorialDataFolder()
    {
        if (!AssetDatabase.IsValidFolder("Assets/GameData"))
            AssetDatabase.CreateFolder("Assets", "GameData");
        if (!AssetDatabase.IsValidFolder(TutorialDataFolder))
            AssetDatabase.CreateFolder("Assets/GameData", "Tutorial");
    }

    private static StageDataSO CreateOrRepairTutorialStage(EconomyConfig tutorialEconomy)
    {
        StageDataSO tutorialStage = AssetDatabase.LoadAssetAtPath<StageDataSO>(TutorialStagePath);
        if (tutorialStage == null)
        {
            if (!AssetDatabase.CopyAsset(SourceStagePath, TutorialStagePath))
                throw new System.InvalidOperationException("Failed to create the tutorial StageDataSO.");
            tutorialStage = AssetDatabase.LoadAssetAtPath<StageDataSO>(TutorialStagePath);
        }

        tutorialStage.name = "Tutorial_FirstBattle";
        tutorialStage.sector = 999;
        tutorialStage.stage = 1;
        tutorialStage.displayName = "Tutorial";
        // The tutorial controller freezes this timer until the highlighted Start button is used.
        tutorialStage.prepareDuration = 90f;
        tutorialStage.hpMultiplier = 0.35f;
        tutorialStage.attackMultiplier = 0.1f;
        tutorialStage.pressureMultiplier = 0.5f;
        tutorialStage.economyConfig = tutorialEconomy;
        tutorialStage.clearRewards = new List<RewardData>
        {
            new() { Type = RewardType.Gold, Amount = UnitExpTable.GetRequiredExp(1) },
            new() { Type = RewardType.Gem, Amount = 15 },
            new() { Type = RewardType.Item, Id = "material_exp_1", Amount = 20 },
            new() { Type = RewardType.Experience, Amount = 100 },
        };

        for (int waveIndex = 0; waveIndex < tutorialStage.waves.Count; waveIndex++)
        {
            WaveData wave = tutorialStage.waves[waveIndex];
            wave.hpMultiplier = waveIndex switch
            {
                0 => 0.25f,
                1 => 0.35f,
                2 => 0.45f,
                _ => 0.6f,
            };
            wave.attackMultiplier = waveIndex < 3 ? 0.15f : 0.25f;
            wave.pressureMultiplier = waveIndex < 3 ? 0.4f : 0.6f;
            foreach (SubWaveData subWave in wave.subWaves)
            {
                subWave.delayAfterSubWave = 0.75f;
                foreach (MonsterSpawnEntry entry in subWave.spawnEntries)
                {
                    entry.count = 1;
                    entry.interval = 0.25f;
                    entry.delayAfterGroup = 0.15f;
                    entry.hpMultiplier = 0.75f;
                    entry.attackMultiplier = 0.5f;
                }
            }
        }

        EditorUtility.SetDirty(tutorialStage);
        return tutorialStage;
    }

    private static void ConfigureTutorialScene(StageDataSO tutorialStage)
    {
        Scene scene = EditorSceneManager.OpenScene(TutorialScenePath, OpenSceneMode.Single);
        TutorialBattleController controller = Object.FindFirstObjectByType<TutorialBattleController>();
        if (controller == null)
        {
            GameObject root = new("Tutorial Battle");
            SceneManager.MoveGameObjectToScene(root, scene);
            controller = root.AddComponent<TutorialBattleController>();
        }

        SerializedObject serialized = new(controller);
        serialized.FindProperty("session").objectReferenceValue =
            Object.FindFirstObjectByType<StageSessionController>();
        serialized.FindProperty("preparationService").objectReferenceValue =
            Object.FindFirstObjectByType<StagePreparationService>();
        serialized.FindProperty("fusionService").objectReferenceValue =
            Object.FindFirstObjectByType<FusionService>();
        serialized.FindProperty("prepareTimer").objectReferenceValue =
            Object.FindFirstObjectByType<StagePrepareTimerController>();
        serialized.FindProperty("prepareActionUI").objectReferenceValue =
            Object.FindFirstObjectByType<StagePrepareActionUI>();
        serialized.FindProperty("unitDragActionUI").objectReferenceValue =
            Object.FindFirstObjectByType<UnitDragActionUI>(FindObjectsInactive.Include);
        serialized.FindProperty("relicUI").objectReferenceValue =
            Object.FindFirstObjectByType<StageRelicUI>();
        serialized.FindProperty("resultUI").objectReferenceValue =
            Object.FindFirstObjectByType<StageResultUI>();
        serialized.FindProperty("overlay").objectReferenceValue =
            TutorialOverlaySceneUtility.GetOrCreate();
        serialized.ApplyModifiedPropertiesWithoutUndo();

        StageSessionController session = Object.FindFirstObjectByType<StageSessionController>();
        SerializedObject sessionSerialized = new(session);
        sessionSerialized.FindProperty("tutorialStageData").objectReferenceValue = tutorialStage;
        sessionSerialized.ApplyModifiedPropertiesWithoutUndo();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    private static void ConfigureLobbyScene()
    {
        Scene scene = EditorSceneManager.OpenScene(LobbyScenePath, OpenSceneMode.Single);
        TutorialOverlayView overlay = TutorialOverlaySceneUtility.GetOrCreate();
        LobbyLevelUpView levelUpView = LobbyLevelUpSceneUtility.GetOrCreate(overlay);
        LobbyTabView[] tabViews = Object.FindObjectsByType<LobbyTabView>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        LobbyTabView mainTabs = tabViews.First(view => new SerializedObject(view).FindProperty("tabs").arraySize == 5);
        LobbyTabView unitDetailTabs = tabViews.First(view => new SerializedObject(view).FindProperty("tabs").arraySize == 4);
        LobbyUnitPanelView unitPanel = Object.FindFirstObjectByType<LobbyUnitPanelView>(FindObjectsInactive.Include);
        UnitDetailView unitDetail = Object.FindFirstObjectByType<UnitDetailView>(FindObjectsInactive.Include);

        SerializedObject mainTabsSerialized = new(mainTabs);
        SerializedProperty mainEntries = mainTabsSerialized.FindProperty("tabs");
        SerializedProperty laboratoryEntry = mainEntries.GetArrayElementAtIndex(3);
        laboratoryEntry.FindPropertyRelative("requiredAccountLevel").intValue = LobbyFeatureUnlockPolicy.LaboratoryRequiredLevel;
        UnityEngine.UI.Button laboratoryButton = laboratoryEntry.FindPropertyRelative("button").objectReferenceValue as UnityEngine.UI.Button;
        GameObject lockVisual = GetOrCreateLockVisual(laboratoryButton);
        laboratoryEntry.FindPropertyRelative("lockVisual").objectReferenceValue = lockVisual;
        mainTabsSerialized.ApplyModifiedPropertiesWithoutUndo();

        SerializedObject detailSerialized = new(unitDetail);
        detailSerialized.FindProperty("contentTabView").objectReferenceValue = unitDetailTabs;
        detailSerialized.ApplyModifiedPropertiesWithoutUndo();

        LobbyTutorialController controller = Object.FindFirstObjectByType<LobbyTutorialController>(FindObjectsInactive.Include);
        if (controller == null)
            controller = overlay.gameObject.AddComponent<LobbyTutorialController>();

        SerializedObject serialized = new(controller);
        serialized.FindProperty("overlay").objectReferenceValue = overlay;
        serialized.FindProperty("mainTabs").objectReferenceValue = mainTabs;
        serialized.FindProperty("unitPanel").objectReferenceValue = unitPanel;
        serialized.FindProperty("levelUpView").objectReferenceValue = levelUpView;
        serialized.ApplyModifiedPropertiesWithoutUndo();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    private static GameObject GetOrCreateLockVisual(UnityEngine.UI.Button button)
    {
        if (button == null)
            throw new System.InvalidOperationException("Laboratory tab button is missing.");

        Transform existing = button.transform.Find("LevelLockVisual");
        if (existing != null)
            return existing.gameObject;

        GameObject root = new("LevelLockVisual", typeof(RectTransform));
        RectTransform rootRect = (RectTransform)root.transform;
        rootRect.SetParent(button.transform, false);
        rootRect.anchorMin = rootRect.anchorMax = new Vector2(0.5f, 0.5f);
        rootRect.sizeDelta = new Vector2(42f, 52f);
        rootRect.SetAsLastSibling();

        CreateLockPart(rootRect, "ShackleTop", new Vector2(0f, 13f), new Vector2(30f, 8f));
        CreateLockPart(rootRect, "ShackleLeft", new Vector2(-11f, 5f), new Vector2(8f, 18f));
        CreateLockPart(rootRect, "ShackleRight", new Vector2(11f, 5f), new Vector2(8f, 18f));
        CreateLockPart(rootRect, "Body", new Vector2(0f, -10f), new Vector2(38f, 30f));
        root.SetActive(false);
        return root;
    }

    private static void CreateLockPart(RectTransform parent, string name, Vector2 position, Vector2 size)
    {
        GameObject part = new(name, typeof(RectTransform), typeof(UnityEngine.UI.Image));
        RectTransform rect = (RectTransform)part.transform;
        rect.SetParent(parent, false);
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        UnityEngine.UI.Image image = part.GetComponent<UnityEngine.UI.Image>();
        image.color = new Color(0.12f, 0.14f, 0.18f, 0.95f);
        image.raycastTarget = false;
    }

    private static void ConfigureStartupScene()
    {
        Scene scene = EditorSceneManager.OpenScene(StartupScenePath, OpenSceneMode.Single);
        StartupBootstrap bootstrap = Object.FindFirstObjectByType<StartupBootstrap>(FindObjectsInactive.Include);
        if (bootstrap == null)
            throw new System.InvalidOperationException("StartupBootstrap is missing from StartScene.");

        SerializedObject serialized = new(bootstrap);
        serialized.FindProperty("tutorialSceneName").stringValue = "TutorialScene";
        serialized.ApplyModifiedPropertiesWithoutUndo();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }
}
