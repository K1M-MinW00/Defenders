using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class LobbyRecruitResultOverlayTypographyMigration
{
    private const string OverlayPath = "LobbyScreenCanvas/RecruitResultOverlay";
    private const string HeaderPath = OverlayPath + "/Header/Text";

    [MenuItem("Tools/Defenders/UI Typography/Apply Recruit Result Overlay")]
    public static void Apply()
    {
        LocalizationProjectSetup.Setup();
        Scene scene = EditorSceneManager.OpenScene(LobbyLocalizationAudit.LobbyScenePath, OpenSceneMode.Single);
        TMP_Text header = FindByPath(scene, HeaderPath)?.GetComponent<TMP_Text>();
        if (header == null) throw new MissingReferenceException(HeaderPath);

        Undo.RecordObject(header, "Configure recruit result typography");
        header.fontSize = 64f;
        header.enableAutoSizing = true;
        header.fontSizeMin = 56f;
        header.fontSizeMax = 64f;
        header.textWrappingMode = TextWrappingModes.NoWrap;
        header.overflowMode = TextOverflowModes.Overflow;
        EditorUtility.SetDirty(header);

        LocalizeStringEvent localizer = header.GetComponent<LocalizeStringEvent>();
        if (localizer == null)
            localizer = Undo.AddComponent<LocalizeStringEvent>(header.gameObject);
        localizer.StringReference = new LocalizedString(LocalizationTables.Lobby, "recruit.result.title");
        if (localizer.OnUpdateString.GetPersistentEventCount() == 0)
            UnityEventTools.AddPersistentListener(localizer.OnUpdateString, header.SetText);
        EditorUtility.SetDirty(localizer);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[LobbyRecruitResultOverlayTypographyMigration] Localized and configured the recruit result overlay.");
    }

    [MenuItem("Tools/Defenders/UI Typography/Inspect Recruit Result Overlay")]
    public static void Inspect()
    {
        Scene scene = EditorSceneManager.OpenScene(LobbyLocalizationAudit.LobbyScenePath, OpenSceneMode.Single);
        GameObject overlay = FindByPath(scene, OverlayPath);
        if (overlay == null) throw new MissingReferenceException(OverlayPath);

        Canvas.ForceUpdateCanvases();
        StringBuilder report = new();
        CanvasScaler scaler = overlay.GetComponentInParent<CanvasScaler>();
        if (scaler != null)
            report.AppendLine($"Canvas reference resolution: {scaler.referenceResolution}, match={scaler.matchWidthOrHeight:0.##}");

        TMP_Text[] texts = overlay.GetComponentsInChildren<TMP_Text>(true);
        report.AppendLine($"TMP count: {texts.Length}");
        foreach (TMP_Text text in texts.OrderBy(item => GetPath(item.transform)))
        {
            RectTransform rect = text.rectTransform;
            report.AppendLine(
                $"{GetPath(text.transform)} | active={text.gameObject.activeInHierarchy} | text='{text.text.Replace("\n", "\\n")}' | " +
                $"rect={rect.rect.width:0.#}x{rect.rect.height:0.#}, position={rect.anchoredPosition} | " +
                $"font={text.fontSize:0.#}, auto={text.enableAutoSizing}({text.fontSizeMin:0.#}-{text.fontSizeMax:0.#}), " +
                $"wrap={text.textWrappingMode}, overflow={text.overflowMode}, preferred={text.preferredWidth:0.#}x{text.preferredHeight:0.#}");
        }

        Debug.Log("[LobbyRecruitResultOverlayTypographyMigration]\n" + report);
    }

    private static GameObject FindByPath(Scene scene, string path)
    {
        string[] parts = path.Split('/');
        GameObject root = scene.GetRootGameObjects().FirstOrDefault(candidate => candidate.name == parts[0]);
        if (root == null) return null;
        Transform target = parts.Length == 1 ? root.transform : root.transform.Find(string.Join("/", parts.Skip(1)));
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
