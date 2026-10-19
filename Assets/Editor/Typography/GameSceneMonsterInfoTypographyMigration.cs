using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class GameSceneMonsterInfoTypographyMigration
{
    private const string ScenePath = "Assets/Scenes/GameScene.unity";
    private const string BasePath = "Stage UI Root/Stage Popup Canvas/Monster Info Popup/Panel/Scroll View/Viewport/Content/";

    [MenuItem("Tools/Defenders/Typography/Apply Game Scene Monster Info Typography")]
    public static void Apply()
    {
        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        Configure(FindText(scene, BasePath + "Empty Text"), 26f, wrap: true);
        Configure(FindText(scene, BasePath + "Monster Info Item Template/Name Background/Name"), 28f, wrap: false);
        Configure(FindText(scene, BasePath + "Monster Info Item Template/Description"), 24f, wrap: true);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[GameSceneMonsterInfoTypographyMigration] Updated empty, name, and description labels.");
    }

    private static void Configure(TMP_Text text, float fontSize, bool wrap)
    {
        Undo.RecordObject(text, "Apply monster info typography");
        text.enableAutoSizing = false;
        text.fontSize = fontSize;
        text.enableWordWrapping = wrap;
        text.overflowMode = TextOverflowModes.Overflow;
        EditorUtility.SetDirty(text);
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
