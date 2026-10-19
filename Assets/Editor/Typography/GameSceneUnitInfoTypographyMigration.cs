using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class GameSceneUnitInfoTypographyMigration
{
    private const string ScenePath = "Assets/Scenes/GameScene.unity";

    private static readonly string[] HeaderPaths =
    {
        "Stage UI Root/UI_StageUnitInfoPanel/Panel/Content/Active/Header",
        "Stage UI Root/UI_StageUnitInfoPanel/Panel/Content/Passive/Header",
    };

    private static readonly string[] DescriptionPaths =
    {
        "Stage UI Root/UI_StageUnitInfoPanel/Panel/Content/Active/Description",
        "Stage UI Root/UI_StageUnitInfoPanel/Panel/Content/Passive/Description",
    };

    [MenuItem("Tools/Defenders/Typography/Apply Game Scene Unit Info Typography")]
    public static void Apply()
    {
        LocalizationProjectSetup.Setup();
        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        foreach (string path in HeaderPaths)
        {
            TMP_Text text = FindText(scene, path);
            Undo.RecordObject(text, "Apply unit info header typography");
            text.enableAutoSizing = false;
            text.fontSize = 23f;
            text.textWrappingMode = TextWrappingModes.NoWrap;
            text.overflowMode = TextOverflowModes.Overflow;
            EditorUtility.SetDirty(text);
        }

        foreach (string path in DescriptionPaths)
        {
            TMP_Text text = FindText(scene, path);
            Undo.RecordObject(text, "Apply unit info description typography");
            text.enableAutoSizing = false;
            text.textWrappingMode = TextWrappingModes.Normal;
            text.overflowMode = TextOverflowModes.Overflow;
            EditorUtility.SetDirty(text);
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[GameSceneUnitInfoTypographyMigration] Updated unit info headers and descriptions.");
    }

    private static TMP_Text FindText(Scene scene, string path)
    {
        int separator = path.IndexOf('/');
        string rootName = path.Substring(0, separator);
        string relativePath = path.Substring(separator + 1);
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            if (root.name != rootName)
                continue;

            Transform target = root.transform.Find(relativePath);
            TMP_Text text = target != null ? target.GetComponent<TMP_Text>() : null;
            if (text != null)
                return text;
        }

        throw new MissingReferenceException(path);
    }
}
