using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class LobbyRewardOverlaysTypographyMigration
{
    private static readonly string[] RootPaths =
    {
        "LobbyScreenCanvas/IdleRewardOverlay",
        "LobbyScreenCanvas/WaveMilestoneRewardOverlay",
    };
    private static readonly string[] PrefabPaths =
    {
        "Assets/Prefabs/UI/IdleRewardSlot.prefab",
        "Assets/Prefabs/UI/WaveMilestoneRewardSlot.prefab",
        "Assets/Prefabs/UI/WaveMilestoneRewardOverlay.prefab",
    };

    [MenuItem("Tools/Defenders/UI Typography/Apply Reward Overlays")]
    public static void Apply()
    {
        Scene scene = EditorSceneManager.OpenScene(LobbyLocalizationAudit.LobbyScenePath, OpenSceneMode.Single);
        int configured = 0;
        foreach (string path in RootPaths)
        {
            GameObject root = FindByPath(scene, path);
            if (root == null) throw new MissingReferenceException(path);
            foreach (TMP_Text text in root.GetComponentsInChildren<TMP_Text>(true))
            {
                (float minimum, float maximum, TextWrappingModes wrapping) = ResolveStyle(GetPath(text.transform));
                Configure(text, minimum, maximum, wrapping);
                configured++;
            }
        }
        foreach (string path in PrefabPaths)
        {
            GameObject root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                foreach (TMP_Text text in root.GetComponentsInChildren<TMP_Text>(true))
                {
                    (float minimum, float maximum, TextWrappingModes wrapping) = ResolveStyle(GetPath(text.transform));
                    Configure(text, minimum, maximum, wrapping);
                    configured++;
                }
                PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        Debug.Log($"[LobbyRewardOverlaysTypographyMigration] Configured {configured} reward overlay labels.");
    }

    [MenuItem("Tools/Defenders/UI Typography/Inspect Reward Overlays")]
    public static void Inspect()
    {
        Scene scene = EditorSceneManager.OpenScene(LobbyLocalizationAudit.LobbyScenePath, OpenSceneMode.Single);
        StringBuilder report = new();
        foreach (string path in RootPaths)
        {
            GameObject root = FindByPath(scene, path);
            if (root == null) throw new MissingReferenceException(path);
            AppendTexts(report, root, path);
        }
        foreach (string path in PrefabPaths)
        {
            GameObject root = PrefabUtility.LoadPrefabContents(path);
            try { AppendTexts(report, root, path); }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }
        Debug.Log("[LobbyRewardOverlaysTypographyMigration]\n" + report);
    }

    private static void AppendTexts(StringBuilder report, GameObject root, string label)
    {
        report.AppendLine($"SOURCE {label}");
        foreach (TMP_Text text in root.GetComponentsInChildren<TMP_Text>(true).OrderBy(item => GetPath(item.transform)))
        {
            Rect rect = text.rectTransform.rect;
            report.AppendLine(
                $"{GetPath(text.transform)} | text='{text.text.Replace("\n", "\\n")}' | " +
                $"rect={rect.width:0.#}x{rect.height:0.#} | font={text.fontSize:0.#}, " +
                $"auto={text.enableAutoSizing}({text.fontSizeMin:0.#}-{text.fontSizeMax:0.#}), " +
                $"wrap={text.textWrappingMode}, overflow={text.overflowMode}");
        }
    }

    private static (float, float, TextWrappingModes) ResolveStyle(string path)
    {
        if (path.Contains("/IdleRewardOverlay/") || path.StartsWith("IdleRewardSlot/"))
        {
            if (path.EndsWith("/ClaimButton/Text")) return (32f, 48f, TextWrappingModes.NoWrap);
            if (path.EndsWith("/Header/Text")) return (42f, 54f, TextWrappingModes.NoWrap);
            if (path.EndsWith("/MaxRewardTimeText")) return (20f, 30f, TextWrappingModes.NoWrap);
            if (path.EndsWith("/GoldHourlyText") || path.EndsWith("/MaterialHourlyText"))
                return (20f, 27f, TextWrappingModes.NoWrap);
            if (path.EndsWith("/CurrentRewardTitleText")) return (34f, 48f, TextWrappingModes.NoWrap);
            if (path.EndsWith("/SectorDescriptionText")) return (22f, 26f, TextWrappingModes.Normal);
            if (path.EndsWith("/AccumulatedTimeText")) return (22f, 30f, TextWrappingModes.NoWrap);
            if (path.EndsWith("/Amount")) return (18f, 24f, TextWrappingModes.NoWrap);
        }

        if (path.EndsWith("/ClaimAllButton/Text") || path.EndsWith("/ClaimAllButton/Label"))
            return (26f, 36f, TextWrappingModes.NoWrap);
        if (path.EndsWith("/Header/Text") || path.EndsWith("/Header/Label"))
            return (24f, 42f, TextWrappingModes.NoWrap);
        if (path.EndsWith("/Header/Stage/Text") || path.EndsWith("/Header/Stage/Label"))
            return (30f, 40f, TextWrappingModes.NoWrap);
        if (path.EndsWith("/ClaimButton/Label")) return (24f, 32f, TextWrappingModes.NoWrap);
        if (path.EndsWith("/WaveLabel")) return (22f, 28f, TextWrappingModes.NoWrap);
        return (18f, 28f, TextWrappingModes.NoWrap);
    }

    private static void Configure(TMP_Text text, float minimum, float maximum, TextWrappingModes wrapping)
    {
        Undo.RecordObject(text, "Configure reward overlay typography");
        text.fontSize = maximum;
        text.enableAutoSizing = true;
        text.fontSizeMin = minimum;
        text.fontSizeMax = maximum;
        text.textWrappingMode = wrapping;
        text.overflowMode = TextOverflowModes.Overflow;
        EditorUtility.SetDirty(text);
    }

    private static GameObject FindByPath(Scene scene, string path)
    {
        string[] parts = path.Split('/');
        GameObject root = scene.GetRootGameObjects().FirstOrDefault(candidate => candidate.name == parts[0]);
        if (root == null) return null;
        Transform target = root.transform.Find(string.Join("/", parts.Skip(1)));
        return target != null ? target.gameObject : null;
    }

    private static string GetPath(Transform transform)
    {
        List<string> parts = new();
        while (transform != null) { parts.Add(transform.name); transform = transform.parent; }
        parts.Reverse();
        return string.Join("/", parts);
    }
}
