using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class LobbyMailboxQuestTypographyMigration
{
    private static readonly string[] RootPaths =
    {
        "LobbyScreenCanvas/MailboxOverlay",
        "LobbyScreenCanvas/QuestOverlay",
    };
    private static readonly string[] PrefabPaths =
    {
        "Assets/Prefabs/UI/UI_MailSlot.prefab",
        "Assets/Prefabs/UI/Quest/QuestItem.prefab",
        "Assets/Prefabs/UI/Quest/QuestReward.prefab",
        "Assets/Prefabs/UI/Quest/QuestRewardDisplay.prefab",
    };

    [MenuItem("Tools/Defenders/UI Typography/Apply Mailbox and Quest UI")]
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
        Debug.Log($"[LobbyMailboxQuestTypographyMigration] Configured {configured} mailbox and quest labels.");
    }

    [MenuItem("Tools/Defenders/UI Typography/Inspect Mailbox and Quest UI")]
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
        Debug.Log("[LobbyMailboxQuestTypographyMigration]\n" + report);
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
        if (path.Contains("/MailboxOverlay/") || path.StartsWith("UI_MailSlot/"))
        {
            if (path.EndsWith("/DismissHintText")) return (24f, 32f, TextWrappingModes.NoWrap);
            if (path.EndsWith("/ModalCard/Header/Text")) return (38f, 50f, TextWrappingModes.NoWrap);
            if (path.Contains("/ActionButtons/") && path.EndsWith("/Text")) return (28f, 42f, TextWrappingModes.NoWrap);
            if (path.EndsWith("/Title_Text")) return (24f, 34f, TextWrappingModes.NoWrap);
            if (path.EndsWith("/Exp_Text")) return (18f, 26f, TextWrappingModes.NoWrap);
        }

        if (path.EndsWith("/QuestWindow/Header/TitleText")) return (36f, 47f, TextWrappingModes.NoWrap);
        if (path.EndsWith("/ActionButton/Text") || path.EndsWith("/ActionButton/Label"))
            return (13f, 22f, TextWrappingModes.NoWrap);
        if (path.EndsWith("/Content/DescriptionText") || path.EndsWith("/Content/Description"))
            return (16f, 23f, TextWrappingModes.NoWrap);
        if (path.EndsWith("/ProgressSlider/ProgressText")) return (15f, 18f, TextWrappingModes.NoWrap);
        if (path.EndsWith("/PointBadge/PointsText") || path.EndsWith("/PointBadge/Points"))
            return (22f, 31f, TextWrappingModes.NoWrap);
        if (path.EndsWith("/PeriodText")) return (14f, 18f, TextWrappingModes.NoWrap);
        if (path.EndsWith("/ThresholdText") || path.EndsWith("/Threshold"))
            return (14f, 18f, TextWrappingModes.NoWrap);
        if (path.EndsWith("/ScoreText")) return (21f, 27f, TextWrappingModes.NoWrap);
        if (path.EndsWith("/Summary/Timer/Text")) return (18f, 24f, TextWrappingModes.NoWrap);
        if (path.Contains("/QuestRewardDisplay/") && (path.EndsWith("/TitleText") || path.EndsWith("/Title")))
            return (22f, 32f, TextWrappingModes.NoWrap);
        return (16f, 26f, TextWrappingModes.NoWrap);
    }

    private static void Configure(TMP_Text text, float minimum, float maximum, TextWrappingModes wrapping)
    {
        Undo.RecordObject(text, "Configure mailbox and quest typography");
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
