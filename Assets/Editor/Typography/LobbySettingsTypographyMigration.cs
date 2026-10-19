using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class LobbySettingsTypographyMigration
{
    private static readonly string[] RootPaths =
    {
        "LobbyScreenCanvas/SettingsOverlay",
        "LobbyScreenCanvas/AccountLinkOverlay",
    };

    [MenuItem("Tools/Defenders/UI Typography/Apply Settings UI")]
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
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log($"[LobbySettingsTypographyMigration] Configured {configured} settings and account labels.");
    }

    [MenuItem("Tools/Defenders/UI Typography/Inspect Settings UI")]
    public static void Inspect()
    {
        Scene scene = EditorSceneManager.OpenScene(LobbyLocalizationAudit.LobbyScenePath, OpenSceneMode.Single);
        StringBuilder report = new();
        foreach (string path in RootPaths)
        {
            GameObject root = FindByPath(scene, path);
            if (root == null) throw new MissingReferenceException(path);
            report.AppendLine($"SOURCE {path}");
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
        Debug.Log("[LobbySettingsTypographyMigration]\n" + report);
    }

    private static GameObject FindByPath(Scene scene, string path)
    {
        string[] parts = path.Split('/');
        GameObject root = scene.GetRootGameObjects().FirstOrDefault(candidate => candidate.name == parts[0]);
        if (root == null) return null;
        Transform target = root.transform.Find(string.Join("/", parts.Skip(1)));
        return target != null ? target.gameObject : null;
    }

    private static (float, float, TextWrappingModes) ResolveStyle(string path)
    {
        if (path.EndsWith("/DismissHintText")) return (24f, 32f, TextWrappingModes.NoWrap);
        if (path.EndsWith("/ModalCard/Header/Text")) return (38f, 50f, TextWrappingModes.NoWrap);

        if (path.Contains("/SettingsOverlay/"))
        {
            if (path.EndsWith("/SoundPanel/Text") || path.EndsWith("/LangaugePanel/Text") || path.EndsWith("/AccountPanel/AccountText"))
                return (24f, 32f, TextWrappingModes.NoWrap);
            if (path.EndsWith("/LanguageDropdown/Text")) return (18f, 28f, TextWrappingModes.NoWrap);
            if (path.EndsWith("/LanguageDropdown/Template/Viewport/Content/Item/ItemText"))
                return (14f, 20f, TextWrappingModes.NoWrap);
            if (path.EndsWith("/LevelImage/LevelText")) return (25f, 34f, TextWrappingModes.NoWrap);
            if (path.EndsWith("/NicknamePanel/NicknameText")) return (24f, 44f, TextWrappingModes.NoWrap);
            if (path.EndsWith("/UidPanel/UidText")) return (18f, 28f, TextWrappingModes.NoWrap);
            if (path.EndsWith("/CopyUidButton/Text") || path.EndsWith("/LinkAccountButton/Text"))
                return (16f, 24f, TextWrappingModes.NoWrap);
        }

        if (path.Contains("/AccountLinkOverlay/"))
        {
            if (path.Contains("/ActionButtons/") && path.EndsWith("/Text")) return (26f, 38f, TextWrappingModes.NoWrap);
            if (path.EndsWith("/ProviderText")) return (24f, 34f, TextWrappingModes.NoWrap);
            if ((path.Contains("/GoogleSignInSection/Button/") || path.Contains("/AppleSignInSection/Button/")) && path.EndsWith("/Text"))
                return (18f, 24f, TextWrappingModes.NoWrap);
        }
        return (18f, 28f, TextWrappingModes.NoWrap);
    }

    private static void Configure(TMP_Text text, float minimum, float maximum, TextWrappingModes wrapping)
    {
        Undo.RecordObject(text, "Configure settings typography");
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
