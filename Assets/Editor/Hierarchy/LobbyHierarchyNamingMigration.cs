using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using TMPro;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class LobbyHierarchyNamingMigration
{
    public const string ReportPath = "Assets/Localization/Reports/LobbyHierarchyRenameMap.csv";

    private static readonly Dictionary<string, string> Words = new(StringComparer.OrdinalIgnoreCase)
    {
        ["btn"] = "Button", ["button"] = "Button", ["img"] = "Image", ["image"] = "Image",
        ["bg"] = "Background", ["tmp"] = "Text", ["text"] = "Text", ["label"] = "Text",
        ["atk"] = "Attack", ["hp"] = "Hp", ["exp"] = "Exp", ["prom"] = "Promotion",
        ["lb"] = "LimitBreak", ["nav"] = "Navigation", ["info"] = "Info", ["ui"] = "Ui",
        ["vfx"] = "Vfx", ["fx"] = "Fx", ["gem"] = "Gem", ["gold"] = "Gold",
    };

    [MenuItem("Tools/Defenders/Hierarchy/Normalize Lobby Scene Names")]
    public static void NormalizeLobbySceneNames()
    {
        Scene scene = EditorSceneManager.OpenScene(LobbyLocalizationAudit.LobbyScenePath, OpenSceneMode.Single);
        HashSet<Transform> animationBound = CollectAnimationBoundTransforms(scene);
        List<Transform> transforms = scene.GetRootGameObjects()
            .SelectMany(root => root.GetComponentsInChildren<Transform>(true))
            .OrderByDescending(GetDepth)
            .ToList();
        List<RenameRecord> records = new();

        foreach (Transform transform in transforms)
        {
            string oldPath = GetPath(transform);
            string newName = NormalizeName(transform.name, transform.GetComponent<TMP_Text>() != null);
            if (newName == transform.name)
                continue;
            if (animationBound.Contains(transform))
            {
                records.Add(new RenameRecord("SkippedAnimation", oldPath, transform));
                continue;
            }
            if (HasSiblingNamed(transform, newName))
            {
                records.Add(new RenameRecord("SkippedDuplicate", oldPath, transform));
                continue;
            }

            Undo.RecordObject(transform.gameObject, "Normalize Lobby hierarchy name");
            transform.name = newName;
            records.Add(new RenameRecord("Renamed", oldPath, transform));
        }

        // Parent renames change descendants' final paths. Resolve every renamed record by finding the object
        // through its stable local file id after all names have been changed.
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        WriteReport(records);
        Debug.Log($"[LobbyHierarchyNamingMigration] Renamed {records.Count(record => record.Status == "Renamed")} objects; " +
                  $"animation skips={records.Count(record => record.Status == "SkippedAnimation")}; " +
                  $"duplicate skips={records.Count(record => record.Status == "SkippedDuplicate")}.");
    }

    [MenuItem("Tools/Defenders/Hierarchy/Validate Lobby Scene Names and Paths")]
    public static void ValidateLobbySceneNamesAndPaths()
    {
        Scene scene = EditorSceneManager.OpenScene(LobbyLocalizationAudit.LobbyScenePath, OpenSceneMode.Single);
        List<string> invalidNames = scene.GetRootGameObjects()
            .SelectMany(root => root.GetComponentsInChildren<Transform>(true))
            .Select(transform => GetPath(transform))
            .Where(path => !Regex.IsMatch(path.Split('/').Last(), @"^[A-Z][A-Za-z0-9]*$"))
            .ToList();
        List<string> missingPaths = new();
        Regex pathPattern = new("\"(LobbyScreenCanvas/[^\"]+)\"");
        foreach (string file in Directory.GetFiles("Assets/Editor/Localization", "*.cs", SearchOption.AllDirectories))
        foreach (Match match in pathPattern.Matches(File.ReadAllText(file)))
        {
            if (FindByPath(scene, match.Groups[1].Value) == null)
                missingPaths.Add($"{file}: {match.Groups[1].Value}");
        }

        int missingScripts = scene.GetRootGameObjects().Sum(root =>
            root.GetComponentsInChildren<Transform>(true).Sum(transform =>
                GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(transform.gameObject)));
        if (invalidNames.Count > 0 || missingPaths.Count > 0 || missingScripts > 0)
            throw new InvalidOperationException(
                $"Lobby hierarchy validation failed. Invalid names={invalidNames.Count}, missing editor paths={missingPaths.Count}, missing scripts={missingScripts}.\n" +
                string.Join("\n", invalidNames.Take(10).Concat(missingPaths.Take(10))));
        Debug.Log("[LobbyHierarchyNamingMigration] Validation passed: names, editor paths, and script components are valid.");
    }

    [MenuItem("Tools/Defenders/Hierarchy/Resolve Lobby Duplicate Names")]
    public static void ResolveDuplicateNames()
    {
        Scene scene = EditorSceneManager.OpenScene(LobbyLocalizationAudit.LobbyScenePath, OpenSceneMode.Single);
        List<string> lines = File.ReadAllLines(ReportPath).ToList();
        int renamed = 0;
        for (int i = 1; i < lines.Count; i++)
        {
            Match row = Regex.Match(lines[i], "^\"SkippedDuplicate\",\"((?:\"\"|[^\"])*)\",\"((?:\"\"|[^\"])*)\"$");
            if (!row.Success) continue;
            string oldPath = row.Groups[1].Value.Replace("\"\"", "\"");
            string currentPath = row.Groups[2].Value.Replace("\"\"", "\"");
            GameObject target = FindByPath(scene, currentPath);
            if (target == null) continue;
            string desired = Regex.Replace(NormalizeName(target.name, target.GetComponent<TMP_Text>() != null), @"\d+$", string.Empty);
            int suffix = 1;
            string unique;
            do unique = desired + suffix++.ToString("00"); while (HasSiblingNamed(target.transform, unique));
            target.name = unique;
            lines[i] = Csv("RenamedDuplicate", oldPath, GetPath(target.transform));
            renamed++;
        }
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        File.WriteAllLines(ReportPath, lines, new UTF8Encoding(true));
        AssetDatabase.ImportAsset(ReportPath);
        Debug.Log($"[LobbyHierarchyNamingMigration] Resolved {renamed} duplicate names.");
    }

    public static string NormalizeName(string source, bool isText)
    {
        if (string.IsNullOrWhiteSpace(source)) return source;
        string sanitized = source.Trim().Replace("&", " And ");
        sanitized = Regex.Replace(sanitized, "([a-z0-9])([A-Z])", "$1 $2");
        string[] tokens = Regex.Split(sanitized, @"[^A-Za-z0-9]+")
            .Where(token => !string.IsNullOrWhiteSpace(token))
            .ToArray();
        if (tokens.Length == 0) return source;

        List<string> normalized = new();
        foreach (string token in tokens)
        {
            if (int.TryParse(token, out int number))
            {
                normalized.Add(number.ToString("00"));
                continue;
            }
            if (Words.TryGetValue(token, out string replacement))
                normalized.Add(replacement);
            else
                normalized.Add(char.ToUpperInvariant(token[0]) + token.Substring(1));
        }

        if (isText)
        {
            int last = normalized.Count - 1;
            if (normalized[last] is "Text" or "Label") normalized[last] = "Text";
            else normalized.Add("Text");
        }
        return string.Concat(normalized);
    }

    private static HashSet<Transform> CollectAnimationBoundTransforms(Scene scene)
    {
        HashSet<Transform> result = new();
        foreach (GameObject root in scene.GetRootGameObjects())
        foreach (Animator animator in root.GetComponentsInChildren<Animator>(true))
        {
            RuntimeAnimatorController controller = animator.runtimeAnimatorController;
            if (controller == null) continue;
            foreach (AnimationClip clip in controller.animationClips.Distinct())
            foreach (EditorCurveBinding binding in AnimationUtility.GetCurveBindings(clip)
                         .Concat(AnimationUtility.GetObjectReferenceCurveBindings(clip)))
            {
                Transform target = string.IsNullOrEmpty(binding.path) ? animator.transform : animator.transform.Find(binding.path);
                while (target != null && target.IsChildOf(animator.transform))
                {
                    result.Add(target);
                    if (target == animator.transform) break;
                    target = target.parent;
                }
            }
        }
        return result;
    }

    private static bool HasSiblingNamed(Transform transform, string name)
    {
        if (transform.parent == null)
            return transform.gameObject.scene.GetRootGameObjects().Any(root => root != transform.gameObject && root.name == name);
        for (int i = 0; i < transform.parent.childCount; i++)
        {
            Transform sibling = transform.parent.GetChild(i);
            if (sibling != transform && sibling.name == name) return true;
        }
        return false;
    }

    private static GameObject FindByPath(Scene scene, string path)
    {
        int separator = path.IndexOf('/');
        string rootName = separator < 0 ? path : path.Substring(0, separator);
        string relativePath = separator < 0 ? string.Empty : path.Substring(separator + 1);
        GameObject root = scene.GetRootGameObjects().FirstOrDefault(candidate => candidate.name == rootName);
        if (root == null) return null;
        Transform target = string.IsNullOrEmpty(relativePath) ? root.transform : root.transform.Find(relativePath);
        return target != null ? target.gameObject : null;
    }

    private static int GetDepth(Transform transform) { int depth = 0; while (transform.parent != null) { depth++; transform = transform.parent; } return depth; }
    private static string GetPath(Transform transform) { List<string> parts = new(); while (transform != null) { parts.Add(transform.name); transform = transform.parent; } parts.Reverse(); return string.Join("/", parts); }

    private static void WriteReport(IEnumerable<RenameRecord> records)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(ReportPath));
        List<string> lines = new() { "Status,OldPath,PathAfterOwnRename" };
        lines.AddRange(records.Select(record => Csv(record.Status, record.OldPath, GetPath(record.Target))));
        File.WriteAllLines(ReportPath, lines, new UTF8Encoding(true));
        AssetDatabase.ImportAsset(ReportPath);
    }

    private static string Csv(params string[] values) => string.Join(",", values.Select(value => $"\"{(value ?? string.Empty).Replace("\"", "\"\"")}\""));
    private readonly struct RenameRecord
    {
        public RenameRecord(string status, string oldPath, Transform target) { Status = status; OldPath = oldPath; Target = target; }
        public string Status { get; }
        public string OldPath { get; }
        public Transform Target { get; }
    }
}
