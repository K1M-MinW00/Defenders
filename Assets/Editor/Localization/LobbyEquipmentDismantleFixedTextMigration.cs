using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.SceneManagement;

public static class LobbyEquipmentDismantleFixedTextMigration
{
    private readonly struct Binding
    {
        public Binding(string hierarchyPath, string table, string key)
        {
            HierarchyPath = hierarchyPath;
            Table = table;
            Key = key;
        }

        public string HierarchyPath { get; }
        public string Table { get; }
        public string Key { get; }
    }

    private static readonly Binding[] Bindings =
    {
        new("LobbyScreenCanvas/InventoryOverlay/ModalCard/InventoryContentHost/EquipmentInventoryBody/DismantleButton/Text", LocalizationTables.Lobby, "inventory.dismantle.title"),
        new("LobbyScreenCanvas/InventoryOverlay/EquipmentDismantleOverlay/ModalCard/Header/TitleText", LocalizationTables.Lobby, "inventory.dismantle.title"),
        new("LobbyScreenCanvas/InventoryOverlay/EquipmentDismantleOverlay/ModalCard/Confirm/Text", LocalizationTables.Lobby, "inventory.dismantle.title"),
        new("LobbyScreenCanvas/InventoryOverlay/EquipmentDismantleOverlay/ModalCard/GuideText", LocalizationTables.Lobby, "inventory.dismantle.guide"),
        new("LobbyScreenCanvas/InventoryOverlay/EquipmentDismantleOverlay/ModalCard/RewardTitleBackground/RewardTitleText", LocalizationTables.Lobby, "inventory.dismantle.rewards"),
        new("LobbyScreenCanvas/EquipmentRewardOverlay/TitleText", LocalizationTables.Lobby, "inventory.dismantle.rewards_acquired"),
        new("LobbyScreenCanvas/EquipmentRewardOverlay/HintText", LocalizationTables.Common, "common.tap_to_close"),
    };

    [MenuItem("Tools/Defenders/Localization/Wire Equipment Dismantle Fixed Text")]
    public static void WireEquipmentDismantleFixedText()
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

            localizer.StringReference = new LocalizedString(binding.Table, binding.Key);
            if (localizer.OnUpdateString.GetPersistentEventCount() == 0)
                UnityEventTools.AddPersistentListener(localizer.OnUpdateString, text.SetText);

            EditorUtility.SetDirty(localizer);
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log($"[LobbyEquipmentDismantleFixedTextMigration] Wired {Bindings.Length} dismantle labels.");
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
