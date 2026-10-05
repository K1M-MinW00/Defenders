using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class StagePauseFormationSceneBuilder
{
    private const string ScenePath = "Assets/Scenes/GameScene.unity";

    [MenuItem("Tools/Defenders/Stage/Connect Pause Formation Portraits")]
    public static void ConnectPortraits()
    {
        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        StagePauseUI pauseUI = Object.FindFirstObjectByType<StagePauseUI>(FindObjectsInactive.Include);
        Transform unitsGroup = FindTransform(scene, "Units Group");

        if (pauseUI == null || unitsGroup == null)
            throw new MissingReferenceException("StagePauseUI 또는 Units Group을 찾을 수 없습니다.");

        Image[] portraits = new Image[5];
        int portraitIndex = 0;
        for (int i = 0; i < unitsGroup.childCount && portraitIndex < portraits.Length; i++)
        {
            Transform child = unitsGroup.GetChild(i);
            if (!child.name.StartsWith("Unit Icon_Mask"))
                continue;

            Image[] childImages = child.GetComponentsInChildren<Image>(true);
            Image portrait = childImages.Length > 1 ? childImages[1] : null;
            if (portrait == null)
                continue;

            portrait.name = $"Unit Portrait {portraitIndex + 1}";
            portrait.preserveAspect = true;
            portrait.raycastTarget = false;
            portraits[portraitIndex++] = portrait;
        }

        if (portraitIndex != portraits.Length)
            throw new MissingReferenceException($"Units Group의 초상화 슬롯이 {portraitIndex}/5개만 확인되었습니다.");

        SerializedObject serialized = new(pauseUI);
        SerializedProperty property = serialized.FindProperty("unitPortraitImages");
        property.arraySize = portraits.Length;
        for (int i = 0; i < portraits.Length; i++)
            property.GetArrayElementAtIndex(i).objectReferenceValue = portraits[i];

        serialized.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(pauseUI);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();

        Debug.Log("[StagePauseFormationSceneBuilder] 전투 부대 초상화 5개를 연결했습니다.");
    }

    private static Transform FindTransform(Scene scene, string objectName)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            Transform[] transforms = root.GetComponentsInChildren<Transform>(true);
            foreach (Transform candidate in transforms)
            {
                if (candidate.name == objectName)
                    return candidate;
            }
        }

        return null;
    }
}
