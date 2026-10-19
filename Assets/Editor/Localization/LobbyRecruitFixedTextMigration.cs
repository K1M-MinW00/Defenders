using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.SceneManagement;

public static class LobbyRecruitFixedTextMigration
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
        new("LobbyScreenCanvas/RecruitConfirmOverlay/ModalCard/Header/Text", "recruit.title"),
        new("LobbyScreenCanvas/LobbyScreenRoot/RecruitPanel/Panel/ScrollView/Viewport/Content/NormalButton/Text", "recruit.tab.normal"),
        new("LobbyScreenCanvas/LobbyScreenRoot/RecruitPanel/Panel/ScrollView/Viewport/Content/SpecialButton/Text", "recruit.tab.special"),
        new("LobbyScreenCanvas/LobbyScreenRoot/RecruitPanel/Panel/RateButton/Text", "recruit.rate_preview"),
        new("LobbyScreenCanvas/RecruitConfirmOverlay/ModalCard/ActionButtons/ConfirmButton/Text", "recruit.button.use"),
        new("LobbyScreenCanvas/RecruitConfirmOverlay/ModalCard/ActionButtons/CancelButton/Text", "recruit.button.back"),
    };

    [MenuItem("Tools/Defenders/Localization/Wire Lobby Recruit Fixed Text")]
    public static void WireLobbyRecruitFixedText()
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
        Debug.Log($"[LobbyRecruitFixedTextMigration] Wired {Bindings.Length} recruit labels.");
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

            if (string.IsNullOrEmpty(relativePath))
                return root;

            Transform child = root.transform.Find(relativePath);
            return child != null ? child.gameObject : null;
        }

        return null;
    }
}
