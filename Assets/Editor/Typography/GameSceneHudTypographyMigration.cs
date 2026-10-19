using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class GameSceneHudTypographyMigration
{
    private const string ScenePath = "Assets/Scenes/GameScene.unity";
    private const string MonsterCountPath = "Stage UI Root/Stage HUD Canvas/HP Summary View/Monster_Slider/Monster Count View";
    private const string StageInfoPath = "Stage UI Root/Stage HUD Canvas/Top HUD/Stage Info View";

    [MenuItem("Tools/Defenders/Typography/Apply Game Scene HUD Typography")]
    public static void Apply()
    {
        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        Configure(FindText(scene, MonsterCountPath), 22f);
        Configure(FindText(scene, StageInfoPath), 26f);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[GameSceneHudTypographyMigration] Updated monster count and stage labels.");
    }

    private static void Configure(TMP_Text text, float fontSize)
    {
        Undo.RecordObject(text, "Apply HUD typography");
        text.enableAutoSizing = false;
        text.fontSize = fontSize;
        text.enableWordWrapping = false;
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
