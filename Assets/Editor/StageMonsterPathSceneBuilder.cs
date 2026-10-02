#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class StageMonsterPathSceneBuilder
{
    private const string ScenePath = "Assets/Scenes/GameScene.unity";
    private const string PreviewObjectName = "Monster Path Preview";

    [MenuItem("Tools/Defenders/Build Stage Monster Path Preview")]
    public static void Build()
    {
        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        StagePhaseRuntimeController phaseController = Object.FindFirstObjectByType<StagePhaseRuntimeController>();
        if (phaseController == null)
            throw new System.InvalidOperationException("StagePhaseRuntimeController was not found.");

        GameObject previewObject = GameObject.Find(PreviewObjectName);
        if (previewObject == null)
            previewObject = new GameObject(PreviewObjectName);

        previewObject.transform.SetParent(null);
        previewObject.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        MonsterPathPreview preview = previewObject.GetComponent<MonsterPathPreview>();
        if (preview == null)
            preview = previewObject.AddComponent<MonsterPathPreview>();

        SerializedObject serializedController = new(phaseController);
        serializedController.FindProperty("pathPreview").objectReferenceValue = preview;
        serializedController.ApplyModifiedPropertiesWithoutUndo();

        EditorUtility.SetDirty(previewObject);
        EditorUtility.SetDirty(phaseController);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[StageMonsterPathSceneBuilder] Monster Path Preview created as a scene root and linked.");
    }
}
#endif
