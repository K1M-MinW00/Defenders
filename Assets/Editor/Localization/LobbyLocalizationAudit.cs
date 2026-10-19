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
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using UnityEngine.SceneManagement;

public enum LocalizationAuditIssueType
{
    MissingLocalizeStringEvent,
    EmptyTableReference,
    EmptyEntryReference,
    MissingTableCollection,
    MissingTableEntry,
    MissingKoreanTranslation,
    MissingEnglishTranslation,
}

public enum LocalizationAuditClassification
{
    FixedUiCandidate,
    DynamicDataCandidate,
    NeedsReview,
}

[Serializable]
public sealed class LocalizationAuditIssue
{
    public LocalizationAuditIssueType Type;
    public string AssetPath;
    public string HierarchyPath;
    public string SourceText;
    public string Table;
    public string Entry;
    public LocalizationAuditClassification Classification;
}

public static class LobbyLocalizationAudit
{
    public const string LobbyScenePath = "Assets/Scenes/LobbyScene.unity";
    public const string DefaultReportPath = "Assets/Localization/Reports/LobbyLocalizationAudit.csv";

    private static readonly Regex TranslatableCharacter = new("[A-Za-z가-힣]", RegexOptions.Compiled);

    [MenuItem("Tools/Defenders/Localization/Audit Lobby Scene")]
    public static void AuditLobbyScene()
    {
        IReadOnlyList<LocalizationAuditIssue> issues = ScanLobbyScene();
        WriteCsv(issues, DefaultReportPath);
        Debug.Log($"[LobbyLocalizationAudit] {issues.Count} issue(s) found. Report: {DefaultReportPath}");
        LobbyLocalizationAuditWindow.ShowResults(issues);
    }

    public static void GenerateLobbyReportBatch()
    {
        IReadOnlyList<LocalizationAuditIssue> issues = ScanLobbyScene();
        WriteCsv(issues, DefaultReportPath);
        Debug.Log($"[LobbyLocalizationAudit] {issues.Count} issue(s) found. Report: {DefaultReportPath}");
    }

    public static IReadOnlyList<LocalizationAuditIssue> ScanLobbyScene()
    {
        var issues = new List<LocalizationAuditIssue>();
        bool alreadyLoaded = SceneManager.GetSceneByPath(LobbyScenePath).isLoaded;
        Scene scene = alreadyLoaded
            ? SceneManager.GetSceneByPath(LobbyScenePath)
            : EditorSceneManager.OpenScene(LobbyScenePath, OpenSceneMode.Additive);

        try
        {
            foreach (GameObject root in scene.GetRootGameObjects())
                ScanTexts(root.GetComponentsInChildren<TMP_Text>(true), LobbyScenePath, issues);

            foreach (string prefabPath in AssetDatabase.GetDependencies(LobbyScenePath, true)
                         .Where(path => path.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase))
                         .Distinct())
            {
                GameObject prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);
                try
                {
                    ScanTexts(prefabRoot.GetComponentsInChildren<TMP_Text>(true), prefabPath, issues);
                }
                finally
                {
                    PrefabUtility.UnloadPrefabContents(prefabRoot);
                }
            }
        }
        finally
        {
            if (!alreadyLoaded)
                EditorSceneManager.CloseScene(scene, true);
        }

        return issues
            .OrderBy(issue => issue.AssetPath, StringComparer.Ordinal)
            .ThenBy(issue => issue.HierarchyPath, StringComparer.Ordinal)
            .ThenBy(issue => issue.Type)
            .ToArray();
    }

    public static bool HasTranslatableText(string value)
    {
        return !string.IsNullOrWhiteSpace(value) && TranslatableCharacter.IsMatch(value);
    }

    public static void WriteCsv(IEnumerable<LocalizationAuditIssue> issues, string assetPath)
    {
        string fullPath = Path.GetFullPath(assetPath);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath) ?? throw new InvalidOperationException());

        var csv = new StringBuilder("Issue,Classification,Asset,Hierarchy,Table,Entry,SourceText\r\n");
        foreach (LocalizationAuditIssue issue in issues)
        {
            csv.Append(Csv(issue.Type.ToString())).Append(',')
                .Append(Csv(issue.Classification.ToString())).Append(',')
                .Append(Csv(issue.AssetPath)).Append(',')
                .Append(Csv(issue.HierarchyPath)).Append(',')
                .Append(Csv(issue.Table)).Append(',')
                .Append(Csv(issue.Entry)).Append(',')
                .Append(Csv(issue.SourceText)).Append("\r\n");
        }

        File.WriteAllText(fullPath, csv.ToString(), new UTF8Encoding(true));
        AssetDatabase.Refresh();
    }

    private static void ScanTexts(IEnumerable<TMP_Text> texts, string assetPath, ICollection<LocalizationAuditIssue> issues)
    {
        foreach (TMP_Text text in texts)
        {
            if (!HasTranslatableText(text.text))
                continue;

            LocalizeStringEvent localizer = text.GetComponent<LocalizeStringEvent>();
            if (localizer == null)
            {
                Add(issues, LocalizationAuditIssueType.MissingLocalizeStringEvent, text, assetPath);
                continue;
            }

            TableReference tableReference = localizer.StringReference.TableReference;
            TableEntryReference entryReference = localizer.StringReference.TableEntryReference;
            if (tableReference.ReferenceType == TableReference.Type.Empty)
            {
                Add(issues, LocalizationAuditIssueType.EmptyTableReference, text, assetPath);
                continue;
            }

            string tableName = tableReference.TableCollectionName;
            if (entryReference.ReferenceType == TableEntryReference.Type.Empty)
            {
                Add(issues, LocalizationAuditIssueType.EmptyEntryReference, text, assetPath, tableName);
                continue;
            }

            var collection = LocalizationEditorSettings.GetStringTableCollection(tableReference);
            if (collection == null)
            {
                Add(issues, LocalizationAuditIssueType.MissingTableCollection, text, assetPath, tableName, entryReference.ToString());
                continue;
            }

            SharedTableData.SharedTableEntry sharedEntry = collection.SharedData.GetEntryFromReference(entryReference);
            if (sharedEntry == null)
            {
                Add(issues, LocalizationAuditIssueType.MissingTableEntry, text, assetPath, collection.TableCollectionName, entryReference.ToString());
                continue;
            }

            CheckTranslation(collection, "ko", LocalizationAuditIssueType.MissingKoreanTranslation, text, assetPath, sharedEntry.Key, issues);
            CheckTranslation(collection, "en", LocalizationAuditIssueType.MissingEnglishTranslation, text, assetPath, sharedEntry.Key, issues);
        }
    }

    private static void CheckTranslation(
        UnityEditor.Localization.StringTableCollection collection,
        string localeCode,
        LocalizationAuditIssueType issueType,
        TMP_Text text,
        string assetPath,
        string key,
        ICollection<LocalizationAuditIssue> issues)
    {
        var table = collection.GetTable(localeCode) as StringTable;
        string value = table?.GetEntry(key)?.Value;
        if (string.IsNullOrWhiteSpace(value))
            Add(issues, issueType, text, assetPath, collection.TableCollectionName, key);
    }

    private static void Add(
        ICollection<LocalizationAuditIssue> issues,
        LocalizationAuditIssueType type,
        TMP_Text text,
        string assetPath,
        string table = "",
        string entry = "")
    {
        issues.Add(new LocalizationAuditIssue
        {
            Type = type,
            AssetPath = assetPath,
            HierarchyPath = GetHierarchyPath(text.transform),
            SourceText = text.text?.Replace('\r', ' ').Replace('\n', ' '),
            Table = table ?? string.Empty,
            Entry = entry ?? string.Empty,
            Classification = Classify(text.text, assetPath, GetHierarchyPath(text.transform)),
        });
    }

    private static string GetHierarchyPath(Transform transform)
    {
        var names = new Stack<string>();
        while (transform != null)
        {
            names.Push(transform.name);
            transform = transform.parent;
        }

        return string.Join("/", names);
    }

    private static string Csv(string value)
    {
        return $"\"{(value ?? string.Empty).Replace("\"", "\"\"")}\"";
    }

    public static LocalizationAuditClassification Classify(string sourceText, string assetPath, string hierarchyPath)
    {
        string value = sourceText?.Trim() ?? string.Empty;
        string lowerPath = (assetPath + "/" + hierarchyPath).ToLowerInvariant();
        string[] dynamicPlaceholders =
        {
            "상품명", "가격", "퀘스트 설명", "유닛 이름", "장비 이름", "아이템 이름",
            "스킬 이름", "스킬 설명", "설명 텍스트", "description", "item name", "unit name",
        };

        if (lowerPath.Contains("ui_mailslot") &&
            (lowerPath.Contains("title_text") || lowerPath.Contains("exp_text")))
            return LocalizationAuditClassification.DynamicDataCandidate;

        if ((lowerPath.Contains("labcard") || lowerPath.Contains("laboffercard")) &&
            lowerPath.EndsWith("/value"))
            return LocalizationAuditClassification.DynamicDataCandidate;

        if (lowerPath.Contains("rechargeshopcard/bonuspanel/bonustext") ||
            lowerPath.Contains("wavemilestonerewardslot/claimbutton/label") ||
            lowerPath.Contains("wavemilestonerewardslot/wavelabel") ||
            (lowerPath.Contains("wavemilestonerewardoverlay/modalcard/header/stage/") &&
             (lowerPath.EndsWith("/label") || lowerPath.EndsWith("/text"))))
            return LocalizationAuditClassification.DynamicDataCandidate;

        if ((lowerPath.Contains("questitem") &&
             (lowerPath.Contains("actionbutton/label") || lowerPath.Contains("content/description"))) ||
            lowerPath.Contains("questrewarddisplay/panel/title"))
            return LocalizationAuditClassification.DynamicDataCandidate;

        if (lowerPath.Contains("nicknameeditoverlay") &&
            lowerPath.Contains("check_button/label"))
            return LocalizationAuditClassification.DynamicDataCandidate;

        if (dynamicPlaceholders.Any(placeholder =>
                string.Equals(value, placeholder, StringComparison.OrdinalIgnoreCase)))
            return LocalizationAuditClassification.DynamicDataCandidate;

        if (lowerPath.Contains("content/") &&
            (lowerPath.Contains("questitem_") || lowerPath.Contains("slot") || lowerPath.Contains("card")))
            return LocalizationAuditClassification.NeedsReview;

        return LocalizationAuditClassification.FixedUiCandidate;
    }
}

public sealed class LobbyLocalizationAuditWindow : EditorWindow
{
    private IReadOnlyList<LocalizationAuditIssue> issues = Array.Empty<LocalizationAuditIssue>();
    private Vector2 scroll;

    public static void ShowResults(IReadOnlyList<LocalizationAuditIssue> results)
    {
        var window = GetWindow<LobbyLocalizationAuditWindow>("Lobby Localization Audit");
        window.issues = results;
        window.Show();
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField($"Issues: {issues.Count}", EditorStyles.boldLabel);
        if (GUILayout.Button("Scan Lobby Scene Again"))
        {
            issues = LobbyLocalizationAudit.ScanLobbyScene();
            LobbyLocalizationAudit.WriteCsv(issues, LobbyLocalizationAudit.DefaultReportPath);
        }

        scroll = EditorGUILayout.BeginScrollView(scroll);
        foreach (LocalizationAuditIssue issue in issues)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField(issue.Type.ToString(), EditorStyles.boldLabel);
            EditorGUILayout.LabelField(issue.Classification.ToString());
            EditorGUILayout.LabelField(issue.AssetPath);
            EditorGUILayout.SelectableLabel(issue.HierarchyPath, GUILayout.Height(EditorGUIUtility.singleLineHeight));
            EditorGUILayout.LabelField($"{issue.Table} / {issue.Entry}");
            EditorGUILayout.LabelField(issue.SourceText, EditorStyles.wordWrappedLabel);
            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.EndScrollView();
    }
}
