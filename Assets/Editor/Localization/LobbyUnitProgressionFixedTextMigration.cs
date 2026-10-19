using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.SceneManagement;

public static class LobbyUnitProgressionFixedTextMigration
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
        new("LobbyScreenCanvas/UnitDetailPanel/NavigationBar/PromotionButton/Text", "unit.nav.promotion"),
        new("LobbyScreenCanvas/UnitDetailPanel/ContentPanel/PromotionPanel/PromoteButton/Text", "unit.nav.promotion"),
        new("LobbyScreenCanvas/UnitDetailPanel/NavigationBar/LimitBreakButton/Text", "unit.nav.limit_break"),
        new("LobbyScreenCanvas/UnitDetailPanel/ContentPanel/LimitBreakPanel/LimitBreakButton/Text", "unit.nav.limit_break"),
        new("LobbyScreenCanvas/UnitDetailPanel/NavigationBar/EquipButton/Text", "unit.nav.equipment"),
        new("LobbyScreenCanvas/UnitDetailPanel/ContentPanel/PromotionPanel/MaterialTab/Title/Text", "unit.promotion.cost"),
        new("LobbyScreenCanvas/UnitDetailPanel/ContentPanel/CommonPanel/UnitDataPanel/SkillPanel/ActiveButton/Icon/ValueText", "unit.skill.active"),
        new("LobbyScreenCanvas/UnitDetailPanel/ActivePanel/ActiveSkillPanel/Title/Text", "unit.skill.active"),
        new("LobbyScreenCanvas/UnitDetailPanel/ContentPanel/CommonPanel/UnitDataPanel/SkillPanel/PassiveButton/Icon/ValueText", "unit.skill.passive"),
        new("LobbyScreenCanvas/UnitDetailPanel/PassivePanel/PassiveSkillPanel/Title/Text", "unit.skill.passive"),
        new("LobbyScreenCanvas/UnitDetailPanel/ContentPanel/PromotionPanel/PromotionDetail/UpgradePanel/UpgradeEntry01/ValueText", "unit.promotion.effect.passive_unlock"),
        new("LobbyScreenCanvas/UnitDetailPanel/ContentPanel/PromotionPanel/PromotionDetail/UpgradePanel/UpgradeEntry02/ValueText", "unit.promotion.effect.active_health"),
        new("LobbyScreenCanvas/UnitDetailPanel/ContentPanel/PromotionPanel/PromotionDetail/UpgradePanel/UpgradeEntry03/ValueText", "unit.promotion.effect.passive_attack"),
    };

    [MenuItem("Tools/Defenders/Localization/Wire Unit Progression Fixed Text")]
    public static void WireUnitProgressionFixedText()
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
        Debug.Log($"[LobbyUnitProgressionFixedTextMigration] Wired {Bindings.Length} progression labels.");
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
