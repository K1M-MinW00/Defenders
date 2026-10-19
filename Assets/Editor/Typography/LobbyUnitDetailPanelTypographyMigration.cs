using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class LobbyUnitDetailPanelTypographyMigration
{
    private const string PanelPath = "LobbyScreenCanvas/UnitDetailPanel";

    [MenuItem("Tools/Defenders/UI Typography/Apply Unit Detail Panel")]
    public static void Apply()
    {
        Scene scene = EditorSceneManager.OpenScene(LobbyLocalizationAudit.LobbyScenePath, OpenSceneMode.Single);
        GameObject panel = FindByPath(scene, PanelPath);
        if (panel == null) throw new MissingReferenceException(PanelPath);

        int configured = 0;
        foreach (TMP_Text text in panel.GetComponentsInChildren<TMP_Text>(true))
        {
            string path = GetPath(text.transform);
            if (path.Contains("/EquipmentChangeOverlay/") || path.Contains("/EquipmentEnhancementOverlay/"))
                continue;

            (float minimum, float maximum) = ResolveSize(path);
            Undo.RecordObject(text, "Configure unit detail typography");
            text.fontSize = maximum;
            text.enableAutoSizing = true;
            text.fontSizeMin = minimum;
            text.fontSizeMax = maximum;
            text.textWrappingMode = TextWrappingModes.NoWrap;
            text.overflowMode = TextOverflowModes.Overflow;
            EditorUtility.SetDirty(text);
            configured++;
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log($"[LobbyUnitDetailPanelTypographyMigration] Configured {configured} unit detail labels; nested equipment overlays were excluded.");
    }

    [MenuItem("Tools/Defenders/UI Typography/Inspect Unit Detail Panel")]
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

        Debug.Log("[LobbyUnitDetailPanelTypographyMigration]\n" + report);
    }

    private static GameObject FindByPath(Scene scene, string path)
    {
        string[] parts = path.Split('/');
        GameObject root = scene.GetRootGameObjects().FirstOrDefault(candidate => candidate.name == parts[0]);
        if (root == null) return null;
        Transform target = parts.Length == 1 ? root.transform : root.transform.Find(string.Join("/", parts.Skip(1)));
        return target != null ? target.gameObject : null;
    }

    private static (float minimum, float maximum) ResolveSize(string path)
    {
        if (path.EndsWith("/UpperBar/UnitNameText")) return (40f, 56f);
        if (path.EndsWith("/UpperBar/RarityPanel/RarityText")) return (30f, 40f);
        if (path.Contains("/NavigationBar/")) return (34f, 40f);
        if (path.EndsWith("/ActivePanel/Text") || path.EndsWith("/PassivePanel/Text")) return (28f, 36f);
        if ((path.Contains("/ActiveSkillPanel/") || path.Contains("/PassiveSkillPanel/")) && path.EndsWith("/Title/Text")) return (48f, 56f);
        if (path.EndsWith("/SkillNameText")) return (32f, 44f);
        if (path.EndsWith("/TrainButton/Text") || path.EndsWith("/PromoteButton/Text") || path.EndsWith("/LimitBreakButton/Text")) return (48f, 56f);
        if (path.Contains("/SkillPanel/") && path.EndsWith("/ValueText")) return (18f, 20f);
        if (path.Contains("/WeaponSlots/") && path.EndsWith("/Title/Text")) return (16f, 24f);
        if (path.Contains("/WeaponSlots/") && path.EndsWith("/LevelText")) return (22f, 28f);
        if (path.Contains("/StatsSlots/") && path.EndsWith("/Title/Text")) return (24f, 30f);
        if (path.EndsWith("/TemplateText")) return (18f, 24f);
        if (path.Contains("/UpgradePanel/") && path.EndsWith("/ValueText")) return (28f, 34f);
        if (path.EndsWith("/MaterialTab/Title/Text")) return (24f, 28f);
        if (path.EndsWith("/MaxUpButton/Text") || path.EndsWith("/UpButton/Text")) return (20f, 24f);
        if (path.EndsWith("/LevelText") || path.EndsWith("/LevelUpText") || path.EndsWith("/ExpUpText")) return (36f, 48f);
        if (path.EndsWith("/AttackText") || path.EndsWith("/HpText")) return (30f, 38f);
        if (path.EndsWith("/GoldText") || path.EndsWith("/MaterialText")) return (24f, 32f);
        if (path.EndsWith("/ExpSlider/FillArea/ValueText")) return (22f, 28f);
        if (path.EndsWith("/PromotionText")) return (22f, 28f);
        return (20f, 28f);
    }

    private static string GetPath(Transform transform)
    {
        List<string> parts = new();
        while (transform != null) { parts.Add(transform.name); transform = transform.parent; }
        parts.Reverse();
        return string.Join("/", parts);
    }
}
