using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class LobbyRecruitPanelTypographyMigration
{
    private const string PanelPath = "LobbyScreenCanvas/LobbyScreenRoot/RecruitPanel";

    [MenuItem("Tools/Defenders/UI Typography/Apply Lobby Recruit Panel")]
    public static void Apply()
    {
        Scene scene = EditorSceneManager.OpenScene(LobbyLocalizationAudit.LobbyScenePath, OpenSceneMode.Single);

        Configure(scene, PanelPath + "/CurrencyView/GemPanel/ValueText", 20f, 28f);
        Configure(scene, PanelPath + "/CurrencyView/TicketPanel/TicketCountText", 20f, 28f);
        Configure(scene, PanelPath + "/Panel/Title/Text", 56f, 64f);
        Configure(scene, PanelPath + "/Panel/ScrollView/Viewport/Content/NormalButton/Text", 36f, 42f);
        Configure(scene, PanelPath + "/Panel/ScrollView/Viewport/Content/SpecialButton/Text", 36f, 42f);
        Configure(scene, PanelPath + "/Panel/RateButton/Text", 26f, 30f);
        Configure(scene, PanelPath + "/Panel/DrawButtons/Draw1Button/Text", 54f, 64f);
        Configure(scene, PanelPath + "/Panel/DrawButtons/Draw10Button/Text", 54f, 64f);
        Configure(scene, PanelPath + "/Panel/DrawButtons/Draw1Button/CostText", 48f, 56f);
        Configure(scene, PanelPath + "/Panel/DrawButtons/Draw10Button/CostText", 48f, 56f);
        Configure(scene, PanelPath + "/Panel/BannerPanel/PityText", 26f, 32f, true);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[LobbyRecruitPanelTypographyMigration] Configured the recruit panel labels.");
    }

    [MenuItem("Tools/Defenders/UI Typography/Inspect Lobby Recruit Panel")]
    public static void Inspect()
    {
        Scene scene = EditorSceneManager.OpenScene(LobbyLocalizationAudit.LobbyScenePath, OpenSceneMode.Single);
        GameObject panel = FindByPath(scene, PanelPath);
        if (panel == null) throw new MissingReferenceException(PanelPath);

        Canvas.ForceUpdateCanvases();
        StringBuilder report = new();
        CanvasScaler scaler = panel.GetComponentInParent<CanvasScaler>();
        if (scaler != null)
            report.AppendLine($"Canvas reference resolution: {scaler.referenceResolution}, match={scaler.matchWidthOrHeight:0.##}");

        TMP_Text[] texts = panel.GetComponentsInChildren<TMP_Text>(true);
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

        Debug.Log("[LobbyRecruitPanelTypographyMigration]\n" + report);
    }

    private static GameObject FindByPath(Scene scene, string path)
    {
        string[] parts = path.Split('/');
        GameObject root = scene.GetRootGameObjects().FirstOrDefault(candidate => candidate.name == parts[0]);
        if (root == null) return null;
        Transform target = parts.Length == 1 ? root.transform : root.transform.Find(string.Join("/", parts.Skip(1)));
        return target != null ? target.gameObject : null;
    }

    private static void Configure(Scene scene, string path, float minimum, float maximum, bool allowWrapping = false)
    {
        TMP_Text text = FindByPath(scene, path)?.GetComponent<TMP_Text>();
        if (text == null) throw new MissingReferenceException(path);
        Undo.RecordObject(text, "Configure recruit panel typography");
        text.fontSize = maximum;
        text.enableAutoSizing = true;
        text.fontSizeMin = minimum;
        text.fontSizeMax = maximum;
        text.textWrappingMode = allowWrapping ? TextWrappingModes.Normal : TextWrappingModes.NoWrap;
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
