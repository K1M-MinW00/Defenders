using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using TMPro;
using UnityEditor;
using UnityEditor.Localization;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.Localization.Tables;
using UnityEngine.SceneManagement;

public static class GameSceneTextLayoutAudit
{
    public const string ReportPath = "Assets/Localization/Reports/GameSceneTextLayoutAudit.csv";
    private const string ScenePath = "Assets/Scenes/GameScene.unity";

    private readonly struct RuntimeSample
    {
        public RuntimeSample(string table, string key, params object[] arguments)
        { Table = table; Key = key; Arguments = arguments; }
        public string Table { get; }
        public string Key { get; }
        public object[] Arguments { get; }
    }

    private static readonly Dictionary<string, RuntimeSample> RuntimeSamples = new()
    {
        ["Stage UI Root/Stage HUD Canvas/HP Summary View/Monster_Slider/Monster Count View"] = new(LocalizationTables.Stage, "hud.monsters.remaining", 99),
        ["Stage UI Root/Stage HUD Canvas/Top HUD/Stage Info View"] = new(LocalizationTables.Stage, "hud.stage", 10, 10),
        ["Stage UI Root/Stage Popup Canvas/Monster Info Popup/Panel/Scroll View/Viewport/Content/Monster Info Item Template/Description"] = new(LocalizationTables.Stage, "monster.description.WereWolf"),
        ["Stage UI Root/Stage Popup Canvas/Monster Info Popup/Panel/Scroll View/Viewport/Content/Monster Info Item Template/Name Background/Name"] = new(LocalizationTables.Stage, "monster.name.Skeleton_Archer"),
        ["Stage UI Root/Stage Popup Canvas/StageResultOverlay/Stage Clear Panel/Return_Button/Text (TMP)"] = new(LocalizationTables.Stage, "result.processing"),
        ["Stage UI Root/Stage Popup Canvas/StageResultOverlay/Stage Clear Panel/StageData_Panel/StageName_Text"] = new(LocalizationTables.Stage, "result.stage", "10-10"),
        ["Stage UI Root/Stage Popup Canvas/StageResultOverlay/Stage Clear Panel/StageData_Panel/SurvivedWave_Panel/SurvivedWave_Text"] = new(LocalizationTables.Stage, "result.waves_survived", 10, 10),
        ["Stage UI Root/Stage Popup Canvas/StageResultOverlay/Stage Fail Panel/Return_Button/Text (TMP)"] = new(LocalizationTables.Stage, "result.processing"),
        ["Stage UI Root/Stage Popup Canvas/StageResultOverlay/Stage Fail Panel/StageData_Panel/StageName_Text"] = new(LocalizationTables.Stage, "result.stage", "10-10"),
        ["Stage UI Root/Stage Popup Canvas/StageResultOverlay/Stage Fail Panel/StageData_Panel/SurvivedWave_Panel/SurvivedWave_Text"] = new(LocalizationTables.Stage, "result.waves_survived", 10, 10),
    };

    [MenuItem("Tools/Defenders/Localization/Audit Game Scene Text Layout")]
    public static void GenerateReportBatch()
    {
        LocalizationProjectSetup.Setup();
        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        GameObject monsterPopup = Find(scene, "Stage UI Root/Stage Popup Canvas/Monster Info Popup");
        GameObject itemTemplate = Find(scene, "Stage UI Root/Stage Popup Canvas/Monster Info Popup/Panel/Scroll View/Viewport/Content/Monster Info Item Template");
        GameObject emptyText = Find(scene, "Stage UI Root/Stage Popup Canvas/Monster Info Popup/Panel/Scroll View/Viewport/Content/Empty Text");
        bool popupWasActive = monsterPopup != null && monsterPopup.activeSelf;
        bool itemWasActive = itemTemplate != null && itemTemplate.activeSelf;
        bool emptyWasActive = emptyText != null && emptyText.activeSelf;
        monsterPopup?.SetActive(true);
        itemTemplate?.SetActive(true);
        emptyText?.SetActive(true);
        Canvas.ForceUpdateCanvases();
        RectTransform content = itemTemplate?.transform.parent as RectTransform;
        if (content != null)
            UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(content);

        List<Row> rows = new();
        TMP_Text[] texts = scene.GetRootGameObjects()
            .SelectMany(root => root.GetComponentsInChildren<TMP_Text>(true))
            .OrderBy(text => PathOf(text.transform), StringComparer.Ordinal)
            .ToArray();

        foreach (TMP_Text text in texts)
        {
            string path = PathOf(text.transform);
            LocalizeStringEvent localizer = text.GetComponent<LocalizeStringEvent>();
            if (localizer == null && !RuntimeSamples.ContainsKey(path))
                continue;

            foreach (string locale in new[] { "ko", "en" })
            {
                string value = localizer != null
                    ? Resolve(localizer, locale)
                    : Resolve(RuntimeSamples[path], locale);
                if (string.IsNullOrWhiteSpace(value))
                    continue;

                Rect rect = text.rectTransform.rect;
                bool description = IsDescription(path);
                float originalFontSize = text.fontSize;
                bool originalAutoSize = text.enableAutoSizing;
                if (originalAutoSize)
                {
                    text.enableAutoSizing = false;
                    text.fontSize = text.fontSizeMin;
                }
                Vector2 preferred = description
                    ? text.GetPreferredValues(value, Mathf.Max(1f, rect.width), float.PositiveInfinity)
                    : text.GetPreferredValues(value, float.PositiveInfinity, float.PositiveInfinity);
                if (originalAutoSize)
                {
                    text.fontSize = originalFontSize;
                    text.enableAutoSizing = true;
                }
                bool widthOverflow = !description && preferred.x > rect.width + 0.5f;
                bool heightOverflow = preferred.y > rect.height + 0.5f;
                bool destructiveOverflow = text.overflowMode == TextOverflowModes.Ellipsis ||
                                           text.overflowMode == TextOverflowModes.Truncate ||
                                           text.overflowMode == TextOverflowModes.Masking;
                string status = widthOverflow || heightOverflow || destructiveOverflow ? "Review" : "Pass";

                rows.Add(new Row(path, locale, value, rect, preferred, description,
                    originalAutoSize, originalFontSize, text.fontSizeMin, text.overflowMode.ToString(), status));
            }
        }

        if (itemTemplate != null) itemTemplate.SetActive(itemWasActive);
        if (emptyText != null) emptyText.SetActive(emptyWasActive);
        if (monsterPopup != null) monsterPopup.SetActive(popupWasActive);

        string fullPath = Path.GetFullPath(ReportPath);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
        StringBuilder csv = new("Status,Locale,Hierarchy,Kind,Text,RectWidth,RectHeight,PreferredWidth,PreferredHeight,AutoSize,FontSize,FontSizeMin,OverflowMode\r\n");
        foreach (Row row in rows)
            csv.Append(row.ToCsv());
        File.WriteAllText(fullPath, csv.ToString(), new UTF8Encoding(true));
        AssetDatabase.Refresh();

        int review = rows.Count(row => row.Status == "Review");
        Debug.Log($"[GameSceneTextLayoutAudit] samples={rows.Count}, pass={rows.Count - review}, review={review}. Report: {ReportPath}");
        foreach (Row row in rows.Where(row => row.Status == "Review"))
            Debug.Log($"[GameSceneTextLayoutAudit] REVIEW [{row.Locale}] {row.Hierarchy} | '{row.Value}' | rect={row.Rect.width:0.#}x{row.Rect.height:0.#}, preferred={row.Preferred.x:0.#}x{row.Preferred.y:0.#}, overflow={row.OverflowMode}");
    }

    private static string Resolve(LocalizeStringEvent localizer, string locale)
    {
        StringTableCollection collection = LocalizationEditorSettings.GetStringTableCollection(localizer.StringReference.TableReference);
        SharedTableData.SharedTableEntry shared = collection?.SharedData.GetEntryFromReference(localizer.StringReference.TableEntryReference);
        return shared == null ? string.Empty : (collection.GetTable(locale) as StringTable)?.GetEntry(shared.Key)?.Value ?? string.Empty;
    }

    private static string Resolve(RuntimeSample sample, string locale)
    {
        StringTableCollection collection = LocalizationEditorSettings.GetStringTableCollection(sample.Table);
        string value = (collection?.GetTable(locale) as StringTable)?.GetEntry(sample.Key)?.Value ?? string.Empty;
        for (int index = 0; index < sample.Arguments.Length; index++)
            value = Regex.Replace(value, "\\{" + index + "(?::[^}]+)?\\}", sample.Arguments[index]?.ToString() ?? string.Empty);
        return value;
    }

    private static bool IsDescription(string path) =>
        path.IndexOf("Description", StringComparison.OrdinalIgnoreCase) >= 0 ||
        path.IndexOf("Desc_Text", StringComparison.OrdinalIgnoreCase) >= 0 ||
        path.IndexOf("Guide", StringComparison.OrdinalIgnoreCase) >= 0 ||
        path.EndsWith("/Empty Text", StringComparison.OrdinalIgnoreCase);

    private static string PathOf(Transform transform)
    {
        Stack<string> names = new();
        while (transform != null) { names.Push(transform.name); transform = transform.parent; }
        return string.Join("/", names);
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

    private static string Csv(string value) => $"\"{(value ?? string.Empty).Replace("\"", "\"\"")}\"";

    private sealed class Row
    {
        public Row(string hierarchy, string locale, string value, Rect rect, Vector2 preferred,
            bool description, bool autoSize, float fontSize, float fontSizeMin, string overflowMode, string status)
        {
            Hierarchy = hierarchy; Locale = locale; Value = value; Rect = rect; Preferred = preferred;
            Description = description; AutoSize = autoSize; FontSize = fontSize; FontSizeMin = fontSizeMin;
            OverflowMode = overflowMode; Status = status;
        }
        public string Hierarchy { get; }
        public string Locale { get; }
        public string Value { get; }
        public Rect Rect { get; }
        public Vector2 Preferred { get; }
        public bool Description { get; }
        public bool AutoSize { get; }
        public float FontSize { get; }
        public float FontSizeMin { get; }
        public string OverflowMode { get; }
        public string Status { get; }
        public string ToCsv() =>
            $"{Csv(Status)},{Csv(Locale)},{Csv(Hierarchy)},{Csv(Description ? "Description" : "SingleLine")},{Csv(Value)}," +
            $"{Rect.width:0.###},{Rect.height:0.###},{Preferred.x:0.###},{Preferred.y:0.###},{AutoSize},{FontSize:0.###},{FontSizeMin:0.###},{Csv(OverflowMode)}\r\n";
    }
}
