using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.SceneManagement;

public static class LobbyFuelProfileNicknameFixedTextMigration
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
        new("LobbyScreenCanvas/FuelDetailOverlay/ModalCard/Header/Text", LocalizationTables.Lobby, "fuel.purchase.title"),
        new("LobbyScreenCanvas/FuelDetailOverlay/ModalCard/FuelProducts/Fuel10Slot/AdButton/Text", LocalizationTables.Lobby, "fuel.purchase.ad"),
        new("LobbyScreenCanvas/FuelDetailOverlay/DismissHintText", LocalizationTables.Common, "common.tap_to_close"),

        new("LobbyScreenCanvas/ProfileEditOverlay/ModalCard/Header/Text", LocalizationTables.Lobby, "profile.title"),
        new("LobbyScreenCanvas/ProfileEditOverlay/ModalCard/ActionButtons/CancelButton/Text", LocalizationTables.Lobby, "profile.cancel"),
        new("LobbyScreenCanvas/ProfileEditOverlay/ModalCard/ActionButtons/CheckButton/Text", LocalizationTables.Lobby, "profile.confirm"),
        new("LobbyScreenCanvas/ProfileEditOverlay/DismissHintText", LocalizationTables.Common, "common.tap_to_close"),

        new("LobbyScreenCanvas/NicknameEditOverlay/ModalCard/Header/Text", LocalizationTables.Lobby, "nickname.title"),
        new("LobbyScreenCanvas/NicknameEditOverlay/ModalCard/NicknameInput/InputText", LocalizationTables.Lobby, "nickname.rule"),
        new("LobbyScreenCanvas/NicknameEditOverlay/ModalCard/NicknameInput/TextArea/PlaceholderText", LocalizationTables.Lobby, "nickname.placeholder"),
        new("LobbyScreenCanvas/NicknameEditOverlay/ModalCard/ActionButtons/CancelButton/Text", LocalizationTables.Lobby, "nickname.cancel"),
        new("LobbyScreenCanvas/NicknameEditOverlay/DismissHintText", LocalizationTables.Common, "common.tap_to_close"),

        new("LobbyScreenCanvas/InventoryOverlay/DismissHintText", LocalizationTables.Common, "common.tap_to_close"),
    };

    [MenuItem("Tools/Defenders/Localization/Wire Fuel Profile Nickname Fixed Text")]
    public static void WireFixedText()
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
        Debug.Log($"[LobbyFuelProfileNicknameFixedTextMigration] Wired {Bindings.Length} labels.");
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
