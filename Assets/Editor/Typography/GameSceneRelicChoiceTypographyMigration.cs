using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class GameSceneRelicChoiceTypographyMigration
{
    private const string ScenePath = "Assets/Scenes/GameScene.unity";
    private const string TitlePath = "Stage UI Root/UI_StageRelics/RelicChoiceOverlay/Panel/Title";

    [MenuItem("Tools/Defenders/Typography/Apply Game Scene Relic Choice Typography")]
    public static void Apply()
    {
        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        TMP_Text title = FindText(scene, TitlePath);
        RectTransform rect = title.rectTransform;
        LayoutElement layout = title.GetComponent<LayoutElement>();

        Undo.RecordObject(title, "Apply relic choice title typography");
        Undo.RecordObject(rect, "Resize relic choice title");
        if (layout != null)
            Undo.RecordObject(layout, "Resize relic choice title layout");

        title.enableAutoSizing = false;
        title.fontSize = 42f;
        title.textWrappingMode = TextWrappingModes.NoWrap;
        title.overflowMode = TextOverflowModes.Overflow;
        rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 70f);
        if (layout != null)
            layout.preferredHeight = 70f;

        EditorUtility.SetDirty(title);
        EditorUtility.SetDirty(rect);
        if (layout != null)
            EditorUtility.SetDirty(layout);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[GameSceneRelicChoiceTypographyMigration] Updated relic choice title.");
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
