using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class GameScenePrepareTypographyMigration
{
    private const string ScenePath = "Assets/Scenes/GameScene.unity";

    private static readonly string[] Paths =
    {
        "Stage UI Root/Stage Prepare Canvas/Prepare Action View/Default Buttons Group/Summon Button/Summon_Text",
        "Stage UI Root/Stage Prepare Canvas/Prepare Action View/Default Buttons Group/Increase Button/IncreasePop_Text",
        "Stage UI Root/Stage Prepare Canvas/Prepare Action View/Unit Action Buttons Group/Reroll Zone/Reroll_Text",
    };

    [MenuItem("Tools/Defenders/Typography/Apply Game Scene Prepare Typography")]
    public static void Apply()
    {
        LocalizationProjectSetup.Setup();
        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        foreach (string path in Paths)
        {
            GameObject target = Find(scene, path);
            TMP_Text text = target != null ? target.GetComponent<TMP_Text>() : null;
            if (text == null) throw new MissingReferenceException(path);

            Undo.RecordObject(text, "Apply prepare button typography");
            text.enableAutoSizing = false;
            text.fontSize = 34f;
            text.enableWordWrapping = false;
            text.overflowMode = TextOverflowModes.Overflow;
            EditorUtility.SetDirty(text);
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log($"[GameScenePrepareTypographyMigration] Updated {Paths.Length} prepare labels.");
    }

    private static GameObject Find(Scene scene, string path)
    {
        int separator = path.IndexOf('/');
        string rootName = separator < 0 ? path : path.Substring(0, separator);
        string relativePath = separator < 0 ? string.Empty : path.Substring(separator + 1);
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            if (root.name != rootName) continue;
            Transform target = string.IsNullOrEmpty(relativePath) ? root.transform : root.transform.Find(relativePath);
            return target != null ? target.gameObject : null;
        }
        return null;
    }
}
