using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class GameSceneResultTypographyMigration
{
    private const string ScenePath = "Assets/Scenes/GameScene.unity";

    private static readonly string[] Paths =
    {
        "Stage UI Root/Stage Popup Canvas/StageResultOverlay/Stage Clear Panel/StageData_Panel/Survived_Text",
        "Stage UI Root/Stage Popup Canvas/StageResultOverlay/Stage Fail Panel/StageData_Panel/Survived_Text",
    };

    private static readonly string[] ValuePaths =
    {
        "Stage UI Root/Stage Popup Canvas/StageResultOverlay/Stage Clear Panel/StageData_Panel/SurvivedWave_Panel/SurvivedWave_Text",
        "Stage UI Root/Stage Popup Canvas/StageResultOverlay/Stage Fail Panel/StageData_Panel/SurvivedWave_Panel/SurvivedWave_Text",
    };

    [MenuItem("Tools/Defenders/Typography/Apply Game Scene Result Typography")]
    public static void Apply()
    {
        LocalizationProjectSetup.Setup();
        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        foreach (string path in Paths)
        {
            TMP_Text text = FindText(scene, path);
            Undo.RecordObject(text, "Apply result typography");
            text.enableAutoSizing = false;
            text.fontSize = 40f;
            text.textWrappingMode = TextWrappingModes.NoWrap;
            text.overflowMode = TextOverflowModes.Overflow;
            EditorUtility.SetDirty(text);
        }

        foreach (string path in ValuePaths)
        {
            TMP_Text text = FindText(scene, path);
            Undo.RecordObject(text, "Apply result value typography");
            text.enableAutoSizing = false;
            text.fontSize = 52f;
            text.textWrappingMode = TextWrappingModes.NoWrap;
            text.overflowMode = TextOverflowModes.Overflow;
            EditorUtility.SetDirty(text);
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log($"[GameSceneResultTypographyMigration] Updated {Paths.Length} labels and {ValuePaths.Length} values.");
    }

    private static TMP_Text FindText(Scene scene, string path)
    {
        int separator = path.IndexOf('/');
        string rootName = path.Substring(0, separator);
        string relativePath = path.Substring(separator + 1);
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            if (root.name != rootName) continue;
            Transform target = root.transform.Find(relativePath);
            TMP_Text text = target != null ? target.GetComponent<TMP_Text>() : null;
            if (text != null) return text;
        }
        throw new MissingReferenceException(path);
    }
}
