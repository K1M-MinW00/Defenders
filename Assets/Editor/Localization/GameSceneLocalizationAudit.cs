using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.SceneManagement;

public static class GameSceneLocalizationAudit
{
    public const string ScenePath = "Assets/Scenes/GameScene.unity";
    public const string ReportPath = "Assets/Localization/Reports/GameSceneLocalizationAudit.csv";

    [MenuItem("Tools/Defenders/Localization/Audit Game Scene")]
    public static void GenerateReportBatch()
    {
        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        TMP_Text[] texts = scene.GetRootGameObjects()
            .SelectMany(root => root.GetComponentsInChildren<TMP_Text>(true))
            .OrderBy(text => GetPath(text.transform))
            .ToArray();

        List<Row> rows = new();
        foreach (TMP_Text text in texts)
        {
            if (!LobbyLocalizationAudit.HasTranslatableText(text.text))
                continue;

            LocalizeStringEvent localizer = text.GetComponent<LocalizeStringEvent>();
            string hierarchy = GetPath(text.transform);
            rows.Add(new Row
            {
                Hierarchy = hierarchy,
                SourceText = text.text.Replace('\r', ' ').Replace('\n', ' '),
                HasLocalizer = localizer != null,
                IsRuntimeLocalized = localizer == null && IsKnownRuntimeLocalized(hierarchy),
                Table = localizer?.StringReference.TableReference.TableCollectionName ?? string.Empty,
                Entry = localizer?.StringReference.TableEntryReference.ToString() ?? string.Empty,
            });
        }

        string fullPath = Path.GetFullPath(ReportPath);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
        StringBuilder csv = new("Hierarchy,SourceText,Status,HasLocalizer,RuntimeLocalized,Table,Entry\r\n");
        foreach (Row row in rows)
        {
            csv.Append(Csv(row.Hierarchy)).Append(',')
                .Append(Csv(row.SourceText)).Append(',')
                .Append(Csv(row.Status)).Append(',')
                .Append(row.HasLocalizer ? "true" : "false").Append(',')
                .Append(row.IsRuntimeLocalized ? "true" : "false").Append(',')
                .Append(Csv(row.Table)).Append(',')
                .Append(Csv(row.Entry)).Append("\r\n");
        }
        File.WriteAllText(fullPath, csv.ToString(), new UTF8Encoding(true));
        AssetDatabase.Refresh();

        Debug.Log($"[GameSceneLocalizationAudit] TMP={texts.Length}, translatable={rows.Count}, " +
                  $"direct={rows.Count(row => row.HasLocalizer)}, runtime={rows.Count(row => row.IsRuntimeLocalized)}, " +
                  $"review={rows.Count(row => !row.HasLocalizer && !row.IsRuntimeLocalized)}. " +
                  $"Report: {ReportPath}");
        foreach (Row row in rows.Where(row => !row.HasLocalizer && !row.IsRuntimeLocalized))
            Debug.Log($"[GameSceneLocalizationAudit] REVIEW {row.Hierarchy} | '{row.SourceText}'");
    }

    private static string GetPath(Transform transform)
    {
        Stack<string> names = new();
        while (transform != null) { names.Push(transform.name); transform = transform.parent; }
        return string.Join("/", names);
    }

    private static string Csv(string value) => $"\"{(value ?? string.Empty).Replace("\"", "\"\"")}\"";

    public static bool IsKnownRuntimeLocalized(string hierarchy)
    {
        if (string.IsNullOrWhiteSpace(hierarchy))
            return false;

        if (hierarchy.EndsWith("/Stage HUD Canvas/HP Summary View/Monster_Slider/Monster Count View") ||
            hierarchy.EndsWith("/Stage HUD Canvas/Top HUD/Stage Info View"))
            return true;

        if (hierarchy.Contains("/Monster Info Popup/Panel/Scroll View/Viewport/Content/Monster Info Item Template/") &&
            (hierarchy.EndsWith("/Description") || hierarchy.EndsWith("/Name Background/Name")))
            return true;

        if (!hierarchy.Contains("/StageResultOverlay/"))
            return false;

        return hierarchy.EndsWith("/Return_Button/Text (TMP)") ||
               hierarchy.EndsWith("/StageData_Panel/StageName_Text") ||
               hierarchy.EndsWith("/StageData_Panel/SurvivedWave_Panel/SurvivedWave_Text");
    }

    private sealed class Row
    {
        public string Hierarchy;
        public string SourceText;
        public bool HasLocalizer;
        public bool IsRuntimeLocalized;
        public string Table;
        public string Entry;
        public string Status => HasLocalizer ? "Direct" : IsRuntimeLocalized ? "Runtime" : "Review";
    }
}
