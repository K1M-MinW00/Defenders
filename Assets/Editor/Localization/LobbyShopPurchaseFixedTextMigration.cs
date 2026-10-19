using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.SceneManagement;

public static class LobbyShopPurchaseFixedTextMigration
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
        new("LobbyScreenCanvas/LobbyScreenRoot/ShopPanel/PurchaseConfirmPopup/ModalCard/Text", "shop.purchase.title"),
        new("LobbyScreenCanvas/LobbyScreenRoot/ShopPanel/PurchaseConfirmPopup/ModalCard/Buttons/Confirm/Text", "shop.purchase.buy"),
        new("LobbyScreenCanvas/LobbyScreenRoot/ShopPanel/PurchaseConfirmPopup/ModalCard/Buttons/Cancel/Text", "shop.purchase.cancel"),
        new("LobbyScreenCanvas/LobbyScreenRoot/ShopPanel/PurchaseConfirmPopup/ModalCard/Buttons/Recharge/Text", "shop.purchase.recharge"),
        new("LobbyScreenCanvas/LobbyScreenRoot/ShopPanel/PurchaseResultPopup/ModalCard/Close/Text", "shop.purchase.close"),
    };

    [MenuItem("Tools/Defenders/Localization/Wire Shop Purchase Fixed Text")]
    public static void WireShopPurchaseFixedText()
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
        Debug.Log($"[LobbyShopPurchaseFixedTextMigration] Wired {Bindings.Length} shop purchase labels.");
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
