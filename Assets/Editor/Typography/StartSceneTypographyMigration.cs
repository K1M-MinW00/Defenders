using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class StartSceneTypographyMigration
{
    private const string ScenePath = "Assets/Scenes/StartScene.unity";

    [MenuItem("Tools/Defenders/Typography/Apply Start Scene Typography")]
    public static void Apply()
    {
        LocalizationProjectSetup.Setup();
        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        Configure(FindText(scene, "Start Screen Canvas/Start Screen/Start Button/Start Button Label"), 48f);
        Configure(FindText(scene, "Start Screen Canvas/Start Screen/Loading Progress/Fill Area/Loading Status Text"), 30f);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[StartSceneTypographyMigration] Updated StartScene button and status typography.");
    }

    private static void Configure(TMP_Text text, float fontSize)
    {
        Undo.RecordObject(text, "Apply StartScene typography");
        text.enableAutoSizing = false;
        text.fontSize = fontSize;
        text.textWrappingMode = TextWrappingModes.NoWrap;
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
