using System;
using TMPro;
using UnityEditor;
using UnityEditor.Localization;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Localization.Tables;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class GameSceneRelicTooltipTypographyMigration
{
    private const string ScenePath = "Assets/Scenes/GameScene.unity";
    private const string TooltipPath = "Stage UI Root/UI_StageRelics/RelicTooltip";
    private const string NamePath = TooltipPath + "/Name";
    private const string DescriptionPath = TooltipPath + "/Description";

    private static readonly string[] RelicIds =
    {
        "WarlordsSeal", "HawkeyeCrystal", "BerserkersSpring", "FrostShackles",
        "ReinforcementFlare", "EliteSummons", "AbundantVein", "DiceOfFate"
    };

    [MenuItem("Tools/Defenders/Typography/Apply Game Scene Relic Tooltip Typography")]
    public static void Apply()
    {
        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        GameObject tooltip = Find(scene, TooltipPath);
        TMP_Text name = FindText(scene, NamePath);
        TMP_Text description = FindText(scene, DescriptionPath);
        RectTransform tooltipRect = tooltip.GetComponent<RectTransform>();
        VerticalLayoutGroup layout = tooltip.GetComponent<VerticalLayoutGroup>();

        Undo.RecordObjects(new UnityEngine.Object[]
        {
            tooltipRect, layout, name, name.rectTransform, description, description.rectTransform,
            name.GetComponent<LayoutElement>(), description.GetComponent<LayoutElement>()
        }, "Apply relic tooltip typography");

        tooltipRect.sizeDelta = new Vector2(420f, 240f);
        layout.padding = new RectOffset(20, 20, 10, 10);
        layout.spacing = 10f;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;

        Configure(name, 380f, 44f, 28f, TextWrappingModes.NoWrap);
        Configure(description, 380f, 156f, 21f, TextWrappingModes.Normal);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AuditCurrentScene(scene, name, description);
        Debug.Log("[GameSceneRelicTooltipTypographyMigration] Updated and verified the relic tooltip for 32 localized samples.");
    }

    [MenuItem("Tools/Defenders/Typography/Audit Game Scene Relic Tooltip Typography")]
    public static void Audit()
    {
        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        AuditCurrentScene(scene, FindText(scene, NamePath), FindText(scene, DescriptionPath));
        Debug.Log("[GameSceneRelicTooltipTypographyMigration] Relic tooltip audit passed: 32 localized samples.");
    }

    private static void Configure(TMP_Text text, float width, float height, float fontSize, TextWrappingModes wrapping)
    {
        text.enableAutoSizing = false;
        text.fontSize = fontSize;
        text.textWrappingMode = wrapping;
        text.overflowMode = TextOverflowModes.Overflow;
        text.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
        text.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);

        LayoutElement element = text.GetComponent<LayoutElement>();
        if (element != null)
        {
            element.preferredWidth = width;
            element.preferredHeight = height;
        }

        EditorUtility.SetDirty(text);
        EditorUtility.SetDirty(text.rectTransform);
        if (element != null)
            EditorUtility.SetDirty(element);
    }

    private static void AuditCurrentScene(Scene scene, TMP_Text name, TMP_Text description)
    {
        StringTableCollection collection = LocalizationEditorSettings.GetStringTableCollection(LocalizationTables.Relics);
        if (collection == null)
            throw new InvalidOperationException("Relics localization table was not found.");

        foreach (string locale in new[] { "ko", "en" })
        {
            StringTable table = collection.GetTable(locale) as StringTable;
            if (table == null)
                throw new InvalidOperationException($"Relics table for locale '{locale}' was not found.");

            foreach (string id in RelicIds)
            {
                VerifyFits(name, GetValue(table, id + ".name"), false, locale, id + ".name");
                VerifyFits(description, GetValue(table, id + ".description"), true, locale, id + ".description");
            }
        }
    }

    private static string GetValue(StringTable table, string key)
    {
        string value = table.GetEntry(key)?.Value;
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidOperationException($"Missing relic localization entry: {table.LocaleIdentifier.Code}/{key}");
        return value;
    }

    private static void VerifyFits(TMP_Text text, string value, bool wrap, string locale, string key)
    {
        Rect rect = text.rectTransform.rect;
        Vector2 preferred = wrap
            ? text.GetPreferredValues(value, rect.width, float.PositiveInfinity)
            : text.GetPreferredValues(value, float.PositiveInfinity, float.PositiveInfinity);
        bool fits = preferred.y <= rect.height + 0.5f && (wrap || preferred.x <= rect.width + 0.5f);
        if (!fits)
            throw new InvalidOperationException(
                $"Relic tooltip overflow: {locale}/{key}, preferred={preferred.x:F1}x{preferred.y:F1}, rect={rect.width:F1}x{rect.height:F1}");
    }

    private static TMP_Text FindText(Scene scene, string path)
    {
        TMP_Text text = Find(scene, path)?.GetComponent<TMP_Text>();
        return text != null ? text : throw new MissingReferenceException(path);
    }

    private static GameObject Find(Scene scene, string path)
    {
        int separator = path.IndexOf('/');
        string rootName = separator < 0 ? path : path.Substring(0, separator);
        string relativePath = separator < 0 ? string.Empty : path.Substring(separator + 1);
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            if (root.name != rootName)
                continue;
            Transform target = string.IsNullOrEmpty(relativePath) ? root.transform : root.transform.Find(relativePath);
            return target != null ? target.gameObject : null;
        }
        throw new MissingReferenceException(path);
    }
}
