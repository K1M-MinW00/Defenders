using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.SceneManagement;

public static class LobbyEquipmentChangeFixedTextMigration
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
        new("LobbyScreenCanvas/UnitDetailPanel/ContentPanel/EquipmentPanel/WeaponSlots/MainWeapon/Title/Text", "equipment.slot.main_weapon"),
        new("LobbyScreenCanvas/UnitDetailPanel/ContentPanel/EquipmentPanel/WeaponSlots/SubWeapon/Title/Text", "equipment.slot.sub_weapon"),
        new("LobbyScreenCanvas/UnitDetailPanel/ContentPanel/EquipmentPanel/WeaponSlots/Helmet/Title/Text", "equipment.slot.helmet"),
        new("LobbyScreenCanvas/UnitDetailPanel/ContentPanel/EquipmentPanel/WeaponSlots/Armor/Title/Text", "equipment.slot.armor"),
        new("LobbyScreenCanvas/UnitDetailPanel/ContentPanel/EquipmentPanel/StatsSlots/EquipmentStats/Title/Text", "equipment.stats.title"),
        new("LobbyScreenCanvas/UnitDetailPanel/ContentPanel/EquipmentPanel/StatsSlots/BonusStats/Title/Text", "equipment.modification.title"),
        new("LobbyScreenCanvas/UnitDetailPanel/EquipmentChangeOverlay/ModalCard/Header/TitleText", "equipment.change.title"),
        new("LobbyScreenCanvas/UnitDetailPanel/EquipmentChangeOverlay/ModalCard/GuideText", "equipment.change.guide"),
        new("LobbyScreenCanvas/UnitDetailPanel/EquipmentChangeOverlay/ModalCard/PreviewInfoText", "equipment.change.empty"),
        new("LobbyScreenCanvas/UnitDetailPanel/EquipmentChangeOverlay/ModalCard/Equip/Text", "equipment.button.equip"),
    };

    [MenuItem("Tools/Defenders/Localization/Wire Equipment Change Fixed Text")]
    public static void WireEquipmentChangeFixedText()
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
        Debug.Log($"[LobbyEquipmentChangeFixedTextMigration] Wired {Bindings.Length} equipment labels.");
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
