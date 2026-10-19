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

public static class LobbyRecruitPresentationOverlayTypographyMigration
{
    private const string OverlayPath = "LobbyScreenCanvas/RecruitPresentationOverlay";

    [MenuItem("Tools/Defenders/UI Typography/Apply Recruit Presentation Overlay")]
    public static void Apply()
    {
        LocalizationProjectSetup.Setup();
        Scene scene = EditorSceneManager.OpenScene(LobbyLocalizationAudit.LobbyScenePath, OpenSceneMode.Single);

        TMP_Text newUnit = Configure(scene, OverlayPath + "/IntroContent/NewUnitText", 48f, 56f);
        TMP_Text unitName = Configure(scene, OverlayPath + "/IntroContent/NamePanel/UnitNameText", 32f, 44f);
        TMP_Text rarity = Configure(scene, OverlayPath + "/IntroContent/NamePanel/RarityText", 22f, 28f);
        TMP_Text skip = Configure(scene, OverlayPath + "/SkipButton/Text", 22f, 28f);
        Bind(newUnit, "recruit.presentation.new_unit");
        Bind(skip, "recruit.presentation.skip");

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[LobbyRecruitPresentationOverlayTypographyMigration] Localized and configured the recruit presentation overlay.");
    }

    [MenuItem("Tools/Defenders/UI Typography/Inspect Recruit Presentation Overlay")]
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

        Debug.Log("[LobbyRecruitPresentationOverlayTypographyMigration]\n" + report);
    }

    private static GameObject FindByPath(Scene scene, string path)
    {
        string[] parts = path.Split('/');
        GameObject root = scene.GetRootGameObjects().FirstOrDefault(candidate => candidate.name == parts[0]);
        if (root == null) return null;
        Transform target = parts.Length == 1 ? root.transform : root.transform.Find(string.Join("/", parts.Skip(1)));
        return target != null ? target.gameObject : null;
    }

    private static TMP_Text Configure(Scene scene, string path, float minimum, float maximum)
    {
        TMP_Text text = FindByPath(scene, path)?.GetComponent<TMP_Text>();
        if (text == null) throw new MissingReferenceException(path);
        Undo.RecordObject(text, "Configure recruit presentation typography");
        text.fontSize = maximum;
        text.enableAutoSizing = true;
        text.fontSizeMin = minimum;
        text.fontSizeMax = maximum;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        text.overflowMode = TextOverflowModes.Overflow;
        EditorUtility.SetDirty(text);
        return text;
    }

    private static void Bind(TMP_Text text, string key)
    {
        LocalizeStringEvent localizer = text.GetComponent<LocalizeStringEvent>();
        if (localizer == null)
            localizer = Undo.AddComponent<LocalizeStringEvent>(text.gameObject);
        localizer.StringReference = new LocalizedString(LocalizationTables.Lobby, key);
        if (localizer.OnUpdateString.GetPersistentEventCount() == 0)
            UnityEventTools.AddPersistentListener(localizer.OnUpdateString, text.SetText);
        EditorUtility.SetDirty(localizer);
    }

    private static string GetPath(Transform transform)
    {
        List<string> parts = new();
        while (transform != null) { parts.Add(transform.name); transform = transform.parent; }
        parts.Reverse();
        return string.Join("/", parts);
    }
}
