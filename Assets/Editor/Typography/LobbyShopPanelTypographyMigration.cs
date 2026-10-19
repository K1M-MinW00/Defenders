using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class LobbyShopPanelTypographyMigration
{
    private const string PanelPath = "LobbyScreenCanvas/LobbyScreenRoot/ShopPanel";
    private static readonly string[] CardPaths =
    {
        "Assets/Prefabs/UI/Shop/LimitedShopCard.prefab",
        "Assets/Prefabs/UI/Shop/PackageShopCard.prefab",
        "Assets/Prefabs/UI/Shop/RechargeShopCard.prefab",
        "Assets/Prefabs/UI/Shop/ExchangeShopCard.prefab",
        "Assets/Prefabs/UI/Shop/ShopRewardSlot.prefab",
    };

    [MenuItem("Tools/Defenders/UI Typography/Apply Shop Panel")]
    public static void Apply()
    {
        Scene scene = EditorSceneManager.OpenScene(LobbyLocalizationAudit.LobbyScenePath, OpenSceneMode.Single);
        GameObject panel = FindByPath(scene, PanelPath);
        if (panel == null) throw new MissingReferenceException(PanelPath);

        foreach (TMP_Text text in panel.GetComponentsInChildren<TMP_Text>(true))
        {
            string path = GetPath(text.transform);
            (float minimum, float maximum, TextWrappingModes wrapping) = ResolvePanelStyle(path);
            Configure(text, minimum, maximum, wrapping);
        }

        foreach (string assetPath in CardPaths)
        {
            GameObject root = PrefabUtility.LoadPrefabContents(assetPath);
            try
            {
                foreach (TMP_Text text in root.GetComponentsInChildren<TMP_Text>(true))
                {
                    (float minimum, float maximum, TextWrappingModes wrapping) =
                        ResolveCardStyle(root.name, GetPath(text.transform));
                    Configure(text, minimum, maximum, wrapping);
                }
                PrefabUtility.SaveAsPrefabAsset(root, assetPath);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        Debug.Log("[LobbyShopPanelTypographyMigration] Configured the shop panel, purchase popups, and 5 shop prefabs.");
    }

    [MenuItem("Tools/Defenders/UI Typography/Inspect Shop Panel")]
    public static void Inspect()
    {
        Scene scene = EditorSceneManager.OpenScene(LobbyLocalizationAudit.LobbyScenePath, OpenSceneMode.Single);
        GameObject panel = FindByPath(scene, PanelPath);
        if (panel == null) throw new MissingReferenceException(PanelPath);

        StringBuilder report = new();
        AppendTexts(report, panel, PanelPath);
        foreach (string assetPath in CardPaths)
        {
            GameObject root = PrefabUtility.LoadPrefabContents(assetPath);
            try { AppendTexts(report, root, assetPath); }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }

        Debug.Log("[LobbyShopPanelTypographyMigration]\n" + report);
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

    private static (float, float, TextWrappingModes) ResolvePanelStyle(string path)
    {
        if (path.EndsWith("/Banner/TitleText")) return (50f, 64f, TextWrappingModes.NoWrap);
        if (path.EndsWith("/Banner/GemPanel/GemText")) return (26f, 36f, TextWrappingModes.NoWrap);
        if (path.Contains("/NotificationBadge/Text")) return (28f, 34f, TextWrappingModes.NoWrap);
        if (path.Contains("/Tabs/") && path.EndsWith("/Text")) return (28f, 40f, TextWrappingModes.NoWrap);
        if (path.Contains("/SectionHeader/Text")) return (32f, 40f, TextWrappingModes.NoWrap);
        if (path.EndsWith("/Contents/FeedbackText")) return (24f, 32f, TextWrappingModes.NoWrap);
        if (path.EndsWith("/PurchaseConfirmPopup/ModalCard/Text")) return (38f, 52f, TextWrappingModes.NoWrap);
        if (path.Contains("/PurchaseConfirmPopup/ModalCard/Buttons/") && path.EndsWith("/Text")) return (22f, 27f, TextWrappingModes.NoWrap);
        if (path.Contains("/PurchaseConfirmPopup/ModalCard/Prices/")) return (20f, 30f, TextWrappingModes.NoWrap);
        if (path.EndsWith("/RemainingGemText")) return (22f, 30f, TextWrappingModes.NoWrap);
        if (path.EndsWith("/WarningText")) return (22f, 30f, TextWrappingModes.NoWrap);
        if (path.EndsWith("/PurchaseResultPopup/ModalCard/ResultTitleText")) return (34f, 44f, TextWrappingModes.NoWrap);
        if (path.EndsWith("/PurchaseResultPopup/ModalCard/Close/Text")) return (22f, 27f, TextWrappingModes.NoWrap);
        if (path.EndsWith("/PurchaseResultPopup/ModalCard/MessageText")) return (20f, 30f, TextWrappingModes.NoWrap);
        if (path.EndsWith("/PurchaseResultPopup/ModalCard/ProductNameText")) return (22f, 32f, TextWrappingModes.NoWrap);
        return (18f, 28f, TextWrappingModes.NoWrap);
    }

    private static (float, float, TextWrappingModes) ResolveCardStyle(string rootName, string path)
    {
        if (path.EndsWith("/Description")) return (16f, 20f, TextWrappingModes.Normal);
        if (path.EndsWith("/Name"))
        {
            if (rootName == "LimitedShopCard") return (24f, 34f, TextWrappingModes.NoWrap);
            if (rootName == "RechargeShopCard") return (16f, 27f, TextWrappingModes.NoWrap);
            return (19f, 27f, TextWrappingModes.NoWrap);
        }
        if (path.EndsWith("/Buy/Label")) return (19f, 27f, TextWrappingModes.NoWrap);
        if (path.EndsWith("/Remain")) return (13f, 18f, TextWrappingModes.NoWrap);
        if (path.EndsWith("/Status")) return (13f, 22f, TextWrappingModes.NoWrap);
        if (path.EndsWith("/Timer")) return (13f, 17f, TextWrappingModes.NoWrap);
        if (path.EndsWith("/BonusPanel/BonusText")) return (15f, 20f, TextWrappingModes.NoWrap);
        if (path.EndsWith("/Amount")) return (17f, 21f, TextWrappingModes.NoWrap);
        return (16f, 24f, TextWrappingModes.NoWrap);
    }

    private static void Configure(TMP_Text text, float minimum, float maximum, TextWrappingModes wrapping)
    {
        Undo.RecordObject(text, "Configure shop typography");
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
