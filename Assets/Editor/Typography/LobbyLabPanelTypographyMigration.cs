using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class LobbyLabPanelTypographyMigration
{
    private const string PanelPath = "LobbyScreenCanvas/LobbyScreenRoot/LabPanel";
    private const string LabCardPath = "Assets/Prefabs/UI/Lab/LabCard.prefab";
    private const string LabOfferCardPath = "Assets/Prefabs/UI/Lab/LabOfferCard.prefab";

    [MenuItem("Tools/Defenders/UI Typography/Apply Lab Panel")]
    public static void Apply()
    {
        Scene scene = EditorSceneManager.OpenScene(LobbyLocalizationAudit.LobbyScenePath, OpenSceneMode.Single);
        GameObject panel = FindByPath(scene, PanelPath);
        if (panel == null) throw new MissingReferenceException(PanelPath);

        Configure(FindText(panel.transform, "Panel/Title/Text"), 48f, 64f, TextWrappingModes.NoWrap);
        Configure(FindText(panel.transform, "Panel/LimitText"), 22f, 30f, TextWrappingModes.NoWrap);
        Configure(FindText(panel.transform, "Panel/UpgradeButton/PriceText"), 25f, 32f, TextWrappingModes.NoWrap);
        Configure(FindText(panel.transform, "Panel/UpgradeButton/UpgradeText"), 28f, 42f, TextWrappingModes.NoWrap);
        Configure(FindText(panel.transform, "UpperBar/MaterialPanel/ValueText"), 24f, 34f, TextWrappingModes.NoWrap);

        LabPanelView view = panel.GetComponent<LabPanelView>();
        if (view == null) throw new MissingComponentException(nameof(LabPanelView));
        SerializedObject serializedView = new(view);
        Configure(GetTextReference(serializedView, "detailName"), 30f, 40f, TextWrappingModes.NoWrap);
        Configure(GetTextReference(serializedView, "detailDescription"), 22f, 26f, TextWrappingModes.Normal);
        Configure(GetTextReference(serializedView, "detailValue"), 38f, 52f, TextWrappingModes.NoWrap);

        ConfigureCardPrefab(LabCardPath, 14f, 22f, 18f, 23f);
        ConfigureCardPrefab(LabOfferCardPath, 16f, 25f, 18f, 23f);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        Debug.Log("[LobbyLabPanelTypographyMigration] Configured the lab panel, detail overlay, and 2 card prefabs.");
    }

    [MenuItem("Tools/Defenders/UI Typography/Inspect Lab Panel")]
    public static void Inspect()
    {
        Scene scene = EditorSceneManager.OpenScene(LobbyLocalizationAudit.LobbyScenePath, OpenSceneMode.Single);
        GameObject panel = FindByPath(scene, PanelPath);
        if (panel == null) throw new MissingReferenceException(PanelPath);

        Canvas.ForceUpdateCanvases();
        StringBuilder report = new();
        TMP_Text[] texts = panel.GetComponentsInChildren<TMP_Text>(true);
        report.AppendLine($"TMP count: {texts.Length}");
        foreach (TMP_Text text in texts.OrderBy(item => GetPath(item.transform)))
        {
            Rect rect = text.rectTransform.rect;
            report.AppendLine(
                $"{GetPath(text.transform)} | text='{text.text.Replace("\n", "\\n")}' | " +
                $"rect={rect.width:0.#}x{rect.height:0.#} | font={text.fontSize:0.#}, " +
                $"auto={text.enableAutoSizing}({text.fontSizeMin:0.#}-{text.fontSizeMax:0.#}), " +
                $"wrap={text.textWrappingMode}, overflow={text.overflowMode}");
        }

        LabPanelView view = panel.GetComponent<LabPanelView>();
        if (view != null)
        {
            SerializedObject serializedView = new(view);
            foreach (string propertyName in new[] { "detailName", "detailDescription", "detailValue" })
            {
                TMP_Text text = serializedView.FindProperty(propertyName)?.objectReferenceValue as TMP_Text;
                if (text == null) continue;
                Rect rect = text.rectTransform.rect;
                report.AppendLine(
                    $"REFERENCE {propertyName}: {GetPath(text.transform)} | text='{text.text.Replace("\n", "\\n")}' | " +
                    $"rect={rect.width:0.#}x{rect.height:0.#} | font={text.fontSize:0.#}, " +
                    $"auto={text.enableAutoSizing}({text.fontSizeMin:0.#}-{text.fontSizeMax:0.#}), " +
                    $"wrap={text.textWrappingMode}, overflow={text.overflowMode}");
            }
        }

        Debug.Log("[LobbyLabPanelTypographyMigration]\n" + report);
    }

    private static GameObject FindByPath(Scene scene, string path)
    {
        string[] parts = path.Split('/');
        GameObject root = scene.GetRootGameObjects().FirstOrDefault(candidate => candidate.name == parts[0]);
        if (root == null) return null;
        Transform target = root.transform.Find(string.Join("/", parts.Skip(1)));
        return target != null ? target.gameObject : null;
    }

    private static TMP_Text FindText(Transform root, string relativePath)
    {
        Transform target = root.Find(relativePath);
        TMP_Text text = target != null ? target.GetComponent<TMP_Text>() : null;
        if (text == null) throw new MissingReferenceException($"{PanelPath}/{relativePath}");
        return text;
    }

    private static TMP_Text GetTextReference(SerializedObject serializedObject, string propertyName)
    {
        TMP_Text text = serializedObject.FindProperty(propertyName)?.objectReferenceValue as TMP_Text;
        if (text == null) throw new MissingReferenceException(propertyName);
        return text;
    }

    private static void ConfigureCardPrefab(
        string assetPath,
        float nameMinimum,
        float nameMaximum,
        float valueMinimum,
        float valueMaximum)
    {
        GameObject root = PrefabUtility.LoadPrefabContents(assetPath);
        try
        {
            Configure(FindText(root.transform, "Name"), nameMinimum, nameMaximum, TextWrappingModes.NoWrap);
            Configure(FindText(root.transform, "Value"), valueMinimum, valueMaximum, TextWrappingModes.NoWrap);
            PrefabUtility.SaveAsPrefabAsset(root, assetPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    private static void Configure(
        TMP_Text text,
        float minimum,
        float maximum,
        TextWrappingModes wrapping)
    {
        Undo.RecordObject(text, "Configure lab typography");
        text.fontSize = maximum;
        text.enableAutoSizing = true;
        text.fontSizeMin = minimum;
        text.fontSizeMax = maximum;
        text.textWrappingMode = wrapping;
        text.overflowMode = TextOverflowModes.Overflow;
        EditorUtility.SetDirty(text);
    }

    private static string GetPath(Transform transform)
    {
        List<string> parts = new();
        while (transform != null) { parts.Add(transform.name); transform = transform.parent; }
        parts.Reverse();
        return string.Join("/", parts);
    }
}
