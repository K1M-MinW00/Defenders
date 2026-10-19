using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.SceneManagement;

public static class LobbyShopFixedTextMigration
{
    private readonly struct Binding
    {
        public Binding(string hierarchyPath, string key)
        {
            HierarchyPath = hierarchyPath;
            Key = key;
        }

        public string HierarchyPath { get; }
        public string Key { get; }
    }

    private static readonly Binding[] Bindings =
    {
        new("LobbyScreenCanvas/LobbyScreenRoot/ShopPanel/Tabs/LimitedTab/Text", "shop.tab.limited"),
        new("LobbyScreenCanvas/LobbyScreenRoot/ShopPanel/Tabs/PackageTab/Text", "shop.tab.package"),
        new("LobbyScreenCanvas/LobbyScreenRoot/ShopPanel/Tabs/RechargeTab/Text", "shop.tab.recharge"),
        new("LobbyScreenCanvas/LobbyScreenRoot/ShopPanel/Tabs/ExchangeTab/Text", "shop.tab.exchange"),
        new("LobbyScreenCanvas/LobbyScreenRoot/ShopPanel/Contents/LimitedPanel/Viewport/Content/SectionHeader/Text", "shop.tab.limited"),
        new("LobbyScreenCanvas/LobbyScreenRoot/ShopPanel/Contents/RechargePanel/Viewport/Content/SectionHeader/Text", "shop.tab.recharge"),
        new("LobbyScreenCanvas/LobbyScreenRoot/ShopPanel/Contents/ExchangePanel/Viewport/Content/SectionHeader/Text", "shop.tab.exchange"),
        new("LobbyScreenCanvas/LobbyScreenRoot/ShopPanel/Contents/PackagePanel/Viewport/Content/DailyPackage/SectionHeader/Text", "shop.section.daily_package"),
        new("LobbyScreenCanvas/LobbyScreenRoot/ShopPanel/Contents/PackagePanel/Viewport/Content/WeeklyPackage/SectionHeader/Text", "shop.section.weekly_package"),
        new("LobbyScreenCanvas/LobbyScreenRoot/ShopPanel/Contents/PackagePanel/Viewport/Content/MonthlyPackage/SectionHeader/Text", "shop.section.monthly_package"),
    };

    [MenuItem("Tools/Defenders/Localization/Wire Shop Fixed Text")]
    public static void WireShopFixedText()
    {
        LocalizationProjectSetup.Setup();
        Scene scene = EditorSceneManager.OpenScene(LobbyLocalizationAudit.LobbyScenePath, OpenSceneMode.Single);

        foreach (Binding binding in Bindings)
        {
            GameObject target = FindByHierarchyPath(scene, binding.HierarchyPath);
            TMP_Text text = target != null ? target.GetComponent<TMP_Text>() : null;
            if (text == null)
                throw new MissingReferenceException($"TMP text was not found at '{binding.HierarchyPath}'.");

            LocalizeStringEvent localizer = target.GetComponent<LocalizeStringEvent>();
            if (localizer == null)
                localizer = Undo.AddComponent<LocalizeStringEvent>(target);

            localizer.StringReference = new LocalizedString(LocalizationTables.Lobby, binding.Key);
            if (localizer.OnUpdateString.GetPersistentEventCount() == 0)
                UnityEventTools.AddPersistentListener(localizer.OnUpdateString, text.SetText);

            EditorUtility.SetDirty(localizer);
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log($"[LobbyShopFixedTextMigration] Wired {Bindings.Length} shop labels.");
    }

    private static GameObject FindByHierarchyPath(Scene scene, string hierarchyPath)
    {
        int separator = hierarchyPath.IndexOf('/');
        string rootName = separator < 0 ? hierarchyPath : hierarchyPath.Substring(0, separator);
        string relativePath = separator < 0 ? string.Empty : hierarchyPath.Substring(separator + 1);
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            if (root.name != rootName)
                continue;

            Transform child = string.IsNullOrEmpty(relativePath) ? root.transform : root.transform.Find(relativePath);
            return child != null ? child.gameObject : null;
        }

        return null;
    }
}
