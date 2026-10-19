using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class LobbyInventoryTypographyMigration
{
    private static readonly string[] RootPaths =
    {
        "LobbyScreenCanvas/InventoryOverlay",
        "LobbyScreenCanvas/ItemDetailOverlay",
        "LobbyScreenCanvas/ItemTooltip",
    };
    private const string InventorySlotPath = "Assets/Prefabs/UI/UI_InventorySlot.prefab";

    [MenuItem("Tools/Defenders/UI Typography/Apply Inventory UI")]
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

        GameObject slot = PrefabUtility.LoadPrefabContents(InventorySlotPath);
        try
        {
            foreach (TMP_Text text in slot.GetComponentsInChildren<TMP_Text>(true))
            {
                Configure(text, 12f, 20f, TextWrappingModes.NoWrap);
                configured++;
            }
            PrefabUtility.SaveAsPrefabAsset(slot, InventorySlotPath);
        }
        finally { PrefabUtility.UnloadPrefabContents(slot); }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        Debug.Log($"[LobbyInventoryTypographyMigration] Configured {configured} inventory labels and the inventory slot prefab.");
    }

    [MenuItem("Tools/Defenders/UI Typography/Inspect Inventory UI")]
    public static void Inspect()
    {
        Scene scene = EditorSceneManager.OpenScene(LobbyLocalizationAudit.LobbyScenePath, OpenSceneMode.Single);
        StringBuilder report = new();
        foreach (string rootPath in RootPaths)
        {
            GameObject root = FindByPath(scene, rootPath);
            if (root == null) throw new MissingReferenceException(rootPath);
            AppendTexts(report, root, rootPath);
        }

        GameObject slot = PrefabUtility.LoadPrefabContents(InventorySlotPath);
        try { AppendTexts(report, slot, InventorySlotPath); }
        finally { PrefabUtility.UnloadPrefabContents(slot); }
        Debug.Log("[LobbyInventoryTypographyMigration]\n" + report);
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
        if (path.EndsWith("/InventoryOverlay/ModalCard/Header/Text")) return (46f, 60f, TextWrappingModes.NoWrap);
        if (path.Contains("/InventoryTabs/") && path.EndsWith("/Text")) return (30f, 44f, TextWrappingModes.NoWrap);
        if (path.EndsWith("/Capacity/CapacityText")) return (30f, 42f, TextWrappingModes.NoWrap);
        if (path.EndsWith("/DismantleButton/Text")) return (28f, 36f, TextWrappingModes.NoWrap);
        if (path.EndsWith("/EquipmentFilterOverlay/ModalCard/Cards/All/Icon/Text")) return (22f, 32f, TextWrappingModes.NoWrap);
        if (path.EndsWith("/EquipmentFilterOverlay/ModalCard/Cards/Header/TitleText")) return (14f, 32f, TextWrappingModes.NoWrap);
        if (path.EndsWith("/EquipmentFilterOverlay/ModalCard/Reset/Text")) return (28f, 36f, TextWrappingModes.NoWrap);
        if (path.EndsWith("/EquipmentSlotTemplate/LevelText")) return (22f, 28f, TextWrappingModes.NoWrap);

        if (path.Contains("/EquipmentDismantleOverlay/") && path.EndsWith("/Header/TitleText"))
            return (36f, 48f, TextWrappingModes.NoWrap);
        if (path.Contains("/EquipmentDismantleOverlay/") && path.EndsWith("/GuideText"))
            return (24f, 34f, TextWrappingModes.NoWrap);
        if (path.Contains("/EquipmentDismantleOverlay/") && path.EndsWith("/Confirm/Text"))
            return (28f, 36f, TextWrappingModes.NoWrap);
        if (path.Contains("/EquipmentDismantleOverlay/") && path.EndsWith("/RewardTitleText"))
            return (28f, 38f, TextWrappingModes.NoWrap);
        if (path.Contains("/EquipmentDismantleOverlay/") && path.EndsWith("/TotalExpText"))
            return (24f, 34f, TextWrappingModes.NoWrap);

        if (path.EndsWith("/ItemDetailOverlay/ModalCard/Header/Text")) return (38f, 50f, TextWrappingModes.NoWrap);
        if (path.EndsWith("/ItemDetail/DescriptionText")) return (26f, 34f, TextWrappingModes.Normal);
        if (path.EndsWith("/ItemDetail/Icon/CountText")) return (28f, 38f, TextWrappingModes.NoWrap);
        if (path.EndsWith("/ItemDetail/ItemNameText")) return (30f, 50f, TextWrappingModes.NoWrap);
        if (path.EndsWith("/ItemTooltip/Panel/NameText")) return (12f, 24f, TextWrappingModes.NoWrap);
        return (20f, 32f, TextWrappingModes.NoWrap);
    }

    private static void Configure(TMP_Text text, float minimum, float maximum, TextWrappingModes wrapping)
    {
        Undo.RecordObject(text, "Configure inventory typography");
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
