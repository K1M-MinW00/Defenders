using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class GameSceneProjectValidator
{
    private const string GameScenePath = "Assets/Scenes/GameScene.unity";

    public static void Validate(GameDataValidationReport report)
    {
        if (report == null)
            return;

        SceneAsset sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(GameScenePath);
        if (sceneAsset == null)
        {
            report.AddError(null, $"Required scene is missing: {GameScenePath}");
            return;
        }

        Scene scene = SceneManager.GetSceneByPath(GameScenePath);
        bool openedForValidation = !scene.IsValid() || !scene.isLoaded;

        try
        {
            if (openedForValidation)
                scene = EditorSceneManager.OpenScene(GameScenePath, OpenSceneMode.Additive);

            ValidateScene(scene, sceneAsset, report);
        }
        catch (System.Exception exception)
        {
            report.AddError(sceneAsset, $"Could not validate GameScene: {exception.Message}");
        }
        finally
        {
            if (openedForValidation && scene.IsValid() && scene.isLoaded)
                EditorSceneManager.CloseScene(scene, true);
        }
    }

    private static void ValidateScene(
        Scene scene,
        SceneAsset sceneAsset,
        GameDataValidationReport report)
    {
        ValidateRequiredComponent<StageSessionController>(scene, sceneAsset, report,
            "phaseRuntimeController",
            "rewardService",
            "bootstrapper",
            "stageUI",
            "progressService");

        ValidateRequiredComponent<StageBootstrapper>(scene, sceneAsset, report,
            "placementController",
            "economyManager",
            "gameCameraController",
            "unitSummoner",
            "monsterSpawner");

        ValidateRequiredComponent<StagePhaseRuntimeController>(scene, sceneAsset, report,
            "prepareTimerController",
            "waveController",
            "preparationService",
            "monsterSpawner",
            "monsterPrewarmService",
            "stageTimeController",
            "poolManager");

        ValidateRequiredComponent<StageUIController>(scene, sceneAsset, report,
            "phaseUIView",
            "hudPresenter",
            "prepareActionUI",
            "unitDragActionUI",
            "waveTrackUI",
            "hpSummaryUI",
            "resultUI",
            "timeController",
            "topControlUI",
            "pausePanelUI",
            "session",
            "flowController",
            "economy",
            "population",
            "monsterSpawner",
            "unitHpTracker",
            "monsterHpTracker",
            "preparationService");

        ValidateRequiredComponent<WaveController>(scene, sceneAsset, report,
            "monsterSpawner",
            "unitRoster");

        ValidateRequiredComponent<UnitSummoner>(scene, sceneAsset, report,
            "poolManager",
            "unitRoster",
            "fusionService",
            "monsterSpawner",
            "unitsRoot");

        ValidateRequiredComponent<MonsterSpawner>(scene, sceneAsset, report,
            "poolManager",
            "unitRoster",
            "waveHpTracker",
            "damageUIService");

        ValidateRequiredComponent<StagePreparationService>(scene, sceneAsset, report,
            "unitSummoner",
            "unitRoster",
            "unitResetService",
            "populationManager",
            "economyManager",
            "placementController");

        ValidateUiComponentOwnership(scene, sceneAsset, report);
    }

    private static void ValidateUiComponentOwnership(
        Scene scene,
        SceneAsset sceneAsset,
        GameDataValidationReport report)
    {
        ValidateComponentOwner<StageUIController>(scene, sceneAsset, report, "Stage UI Root");
        ValidateComponentOwner<StagePhaseUIView>(scene, sceneAsset, report, "Stage UI Phase Coordinator");

        ValidateComponentOwner<StageHudPresenter>(scene, sceneAsset, report, "Stage HUD Canvas");
        ValidateComponentOwner<StageWaveTrackUI>(scene, sceneAsset, report, "Stage HUD Canvas");
        ValidateComponentOwner<StageHpSummaryUI>(scene, sceneAsset, report, "Stage HUD Canvas");
        ValidateComponentOwner<StageTopControlUI>(scene, sceneAsset, report, "Stage HUD Canvas");

        ValidateComponentOwner<StagePrepareActionUI>(scene, sceneAsset, report, "Stage Prepare Canvas");
        ValidateComponentOwner<UnitDragActionUI>(scene, sceneAsset, report, "Stage Prepare Canvas");

        ValidateComponentOwner<StageResultUI>(scene, sceneAsset, report, "Stage Popup Canvas");
        ValidateComponentOwner<StagePauseUI>(scene, sceneAsset, report, "Stage Popup Canvas");
    }

    private static void ValidateComponentOwner<T>(
        Scene scene,
        SceneAsset sceneAsset,
        GameDataValidationReport report,
        string expectedObjectName) where T : Component
    {
        List<T> components = FindComponentsInScene<T>(scene);
        if (components.Count != 1)
            return;

        T component = components[0];
        if (component.gameObject.name != expectedObjectName)
        {
            report.AddError(sceneAsset,
                $"{typeof(T).Name} must be attached to '{expectedObjectName}', " +
                $"but is attached to '{component.gameObject.name}'.");
        }
    }

    private static void ValidateRequiredComponent<T>(
        Scene scene,
        SceneAsset sceneAsset,
        GameDataValidationReport report,
        params string[] requiredReferenceNames) where T : Component
    {
        List<T> components = FindComponentsInScene<T>(scene);
        if (components.Count == 0)
        {
            report.AddError(sceneAsset, $"Required component is missing: {typeof(T).Name}");
            return;
        }

        if (components.Count > 1)
        {
            report.AddError(sceneAsset,
                $"Expected one {typeof(T).Name}, but found {components.Count}.");
            return;
        }

        T component = components[0];
        SerializedObject serializedObject = new(component);
        foreach (string propertyName in requiredReferenceNames)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null)
            {
                report.AddError(sceneAsset,
                    $"{typeof(T).Name} validation field was not found: {propertyName}");
                continue;
            }

            if (property.propertyType != SerializedPropertyType.ObjectReference)
            {
                report.AddError(sceneAsset,
                    $"{typeof(T).Name}.{propertyName} is not an object reference.");
                continue;
            }

            if (property.objectReferenceValue == null)
                report.AddError(sceneAsset, $"{typeof(T).Name}.{propertyName} is not assigned.");
        }
    }

    private static List<T> FindComponentsInScene<T>(Scene scene) where T : Component
    {
        List<T> result = new();
        foreach (GameObject root in scene.GetRootGameObjects())
            result.AddRange(root.GetComponentsInChildren<T>(true));

        return result;
    }
}
