using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class LobbyFuelProfileTypographyMigration
{
    private static readonly string[] RootPaths =
    {
        "LobbyScreenCanvas/FuelDetailOverlay",
        "LobbyScreenCanvas/ProfileEditOverlay",
        "LobbyScreenCanvas/NicknameEditOverlay",
    };

    [MenuItem("Tools/Defenders/UI Typography/Apply Fuel and Profile Overlays")]
    public static void Apply()
    {
        Scene scene = EditorSceneManager.OpenScene(LobbyLocalizationAudit.LobbyScenePath, OpenSceneMode.Single);
        int configured = 0;
        foreach (string rootPath in RootPaths)
        {
            GameObject root = FindByPath(scene, rootPath);
            if (root == null) throw new MissingReferenceException(rootPath);
            foreach (TMP_Text text in root.GetComponentsInChildren<TMP_Text>(true))
            {
                (float minimum, float maximum, TextWrappingModes wrapping) = ResolveStyle(GetPath(text.transform));
                Configure(text, minimum, maximum, wrapping);
                configured++;
            }
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log($"[LobbyFuelProfileTypographyMigration] Configured {configured} fuel, profile, and nickname labels.");
    }

    [MenuItem("Tools/Defenders/UI Typography/Inspect Fuel and Profile Overlays")]
    public static void Inspect()
    {
        Scene scene = EditorSceneManager.OpenScene(LobbyLocalizationAudit.LobbyScenePath, OpenSceneMode.Single);
        StringBuilder report = new();
        foreach (string rootPath in RootPaths)
        {
            GameObject root = FindByPath(scene, rootPath);
            if (root == null) throw new MissingReferenceException(rootPath);
            report.AppendLine($"SOURCE {rootPath}");
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
        Debug.Log("[LobbyFuelProfileTypographyMigration]\n" + report);
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
        if (path.Contains("/ActionButtons/") && path.EndsWith("/Text")) return (30f, 44f, TextWrappingModes.NoWrap);

        if (path.Contains("/FuelProducts/") && (path.EndsWith("/AdButton/Text") || path.EndsWith("/BuyButton/Text")))
            return (28f, 42f, TextWrappingModes.NoWrap);
        if (path.Contains("/FuelProducts/") && path.EndsWith("/ItemSlot/CountText"))
            return (32f, 44f, TextWrappingModes.NoWrap);
        if (path.EndsWith("/RemainAdText")) return (20f, 28f, TextWrappingModes.NoWrap);
        if (path.EndsWith("/FullRecoverText") || path.EndsWith("/NextRecoverText"))
            return (26f, 36f, TextWrappingModes.NoWrap);

        if (path.EndsWith("/NicknameInput/InputText")) return (20f, 30f, TextWrappingModes.Normal);
        if (path.EndsWith("/NicknameInput/TextArea/PlaceholderText")) return (28f, 40f, TextWrappingModes.NoWrap);
        if (path.EndsWith("/NicknameInput/TextArea/Text")) return (30f, 40f, TextWrappingModes.NoWrap);
        return (24f, 36f, TextWrappingModes.NoWrap);
    }

    private static void Configure(TMP_Text text, float minimum, float maximum, TextWrappingModes wrapping)
    {
        Undo.RecordObject(text, "Configure fuel and profile typography");
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
