using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class BossFeatureSetup
{
    private const string GameScenePath = "Assets/Scenes/GameScene.unity";

    [MenuItem("Tools/Game/Bosses/Apply Runtime Setup")]
    public static void ApplyRuntimeSetup()
    {
        Scene scene = EditorSceneManager.OpenScene(GameScenePath, OpenSceneMode.Single);
        StagePhaseRuntimeController phaseRuntime = Object.FindFirstObjectByType<StagePhaseRuntimeController>(
            FindObjectsInactive.Include);
        if (phaseRuntime == null)
            throw new System.InvalidOperationException("StagePhaseRuntimeController was not found in GameScene.");

        BossBattleTimerController timer = phaseRuntime.GetComponent<BossBattleTimerController>();
        if (timer == null)
            timer = Undo.AddComponent<BossBattleTimerController>(phaseRuntime.gameObject);

        SerializedObject serializedRuntime = new(phaseRuntime);
        serializedRuntime.FindProperty("bossBattleTimerController").objectReferenceValue = timer;
        serializedRuntime.ApplyModifiedPropertiesWithoutUndo();

        EditorUtility.SetDirty(phaseRuntime);
        EditorUtility.SetDirty(timer);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        Debug.Log("Boss runtime setup applied to GameScene.");
    }
}
