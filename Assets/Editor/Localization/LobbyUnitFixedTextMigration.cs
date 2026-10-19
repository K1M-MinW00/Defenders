using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.SceneManagement;

public static class LobbyUnitFixedTextMigration
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
        new("LobbyScreenCanvas/LobbyScreenRoot/UnitPanel/PanelTitle/ValueText", "unit.title"),
        new("LobbyScreenCanvas/LobbyScreenRoot/UnitPanel/PanelSelectedUnit/PanelTitle/ValueText", "unit.roster.battle"),
        new("LobbyScreenCanvas/LobbyScreenRoot/UnitPanel/PanelOwnedUnit/PanelTitle/ValueText", "unit.roster.reserve"),
        new("LobbyScreenCanvas/UnitDetailPanel/ContentPanel/TrainingPanel/AttackTab/TextContainer/Text", "unit.stat.attack"),
        new("LobbyScreenCanvas/UnitDetailPanel/ContentPanel/TrainingPanel/HpTab/TextContainer/Text", "unit.stat.health"),
        new("LobbyScreenCanvas/UnitDetailPanel/ContentPanel/TrainingPanel/MaterialTab/Title/Text", "unit.training.select_materials"),
        new("LobbyScreenCanvas/UnitDetailPanel/ContentPanel/TrainingPanel/MaterialTab/MaterialsAndUpButtons/UpButton/Text", "unit.training.level_one"),
        new("LobbyScreenCanvas/UnitDetailPanel/ContentPanel/TrainingPanel/MaterialTab/MaterialsAndUpButtons/MaxUpButton/Text", "unit.training.level_max"),
        new("LobbyScreenCanvas/UnitDetailPanel/ContentPanel/TrainingPanel/TrainButton/Text", "unit.training.title"),
        new("LobbyScreenCanvas/UnitDetailPanel/NavigationBar/TrainButton/Text", "unit.training.title"),
    };

    [MenuItem("Tools/Defenders/Localization/Wire Lobby Unit Fixed Text")]
    public static void WireLobbyUnitFixedText()
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
        Debug.Log($"[LobbyUnitFixedTextMigration] Wired {Bindings.Length} unit labels.");
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
