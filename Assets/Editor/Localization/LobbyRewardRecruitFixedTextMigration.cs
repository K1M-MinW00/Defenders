using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.SceneManagement;

public static class LobbyRewardRecruitFixedTextMigration
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

    private static readonly Binding[] SceneBindings =
    {
        new("LobbyScreenCanvas/IdleRewardOverlay/ModalCard/Header/Text", "shortcut.supply"),
        new("LobbyScreenCanvas/IdleRewardOverlay/ModalCard/RewardScrollView/CurrentRewardTitleText", "idle.current_rewards"),
        new("LobbyScreenCanvas/IdleRewardOverlay/ModalCard/MaxRewardTimeText", "idle.max_supply_support"),
        new("LobbyScreenCanvas/IdleRewardOverlay/ModalCard/ClaimButton/Text", "idle.claim.button"),
        new("LobbyScreenCanvas/WaveMilestoneRewardOverlay/ModalCard/Header/Text", "wave_reward.title"),
        new("LobbyScreenCanvas/WaveMilestoneRewardOverlay/ModalCard/ClaimAllButton/Text", "wave_reward.claim_all"),
        new("LobbyScreenCanvas/LobbyScreenRoot/RecruitPanel/Panel/Title/Text", "recruit.panel.title"),
        new("LobbyScreenCanvas/LobbyScreenRoot/RecruitPanel/Panel/DrawButtons/Draw1Button/Text", "recruit.button.recruit"),
        new("LobbyScreenCanvas/LobbyScreenRoot/RecruitPanel/Panel/DrawButtons/Draw10Button/Text", "recruit.button.recruit"),
    };

    private static readonly Binding[] WavePrefabBindings =
    {
        new("WaveMilestoneRewardOverlay/ModalCard/Header/Label", "wave_reward.title"),
        new("WaveMilestoneRewardOverlay/ModalCard/ClaimAllButton/Label", "wave_reward.claim_all"),
    };

    [MenuItem("Tools/Defenders/Localization/Wire Reward And Recruit Fixed Text")]
    public static void WireFixedText()
    {
        LocalizationProjectSetup.Setup();
        Scene scene = EditorSceneManager.OpenScene(LobbyLocalizationAudit.LobbyScenePath, OpenSceneMode.Single);
        foreach (Binding binding in SceneBindings)
            Wire(FindByHierarchyPath(scene, binding.HierarchyPath), binding);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        const string prefabPath = "Assets/Prefabs/UI/WaveMilestoneRewardOverlay.prefab";
        GameObject prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);
        try
        {
            foreach (Binding binding in WavePrefabBindings)
                Wire(FindByHierarchyPath(prefabRoot, binding.HierarchyPath), binding);
            PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(prefabRoot);
        }

        Debug.Log($"[LobbyRewardRecruitFixedTextMigration] Wired {SceneBindings.Length + WavePrefabBindings.Length} labels.");
    }

    private static void Wire(GameObject target, Binding binding)
    {
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

    private static GameObject FindByHierarchyPath(Scene scene, string hierarchyPath)
    {
        int separator = hierarchyPath.IndexOf('/');
        string rootName = separator < 0 ? hierarchyPath : hierarchyPath.Substring(0, separator);
        string relativePath = separator < 0 ? string.Empty : hierarchyPath.Substring(separator + 1);
        foreach (GameObject root in scene.GetRootGameObjects())
            if (root.name == rootName)
                return FindByRelativePath(root, relativePath);
        return null;
    }

    private static GameObject FindByHierarchyPath(GameObject root, string hierarchyPath)
    {
        int separator = hierarchyPath.IndexOf('/');
        string rootName = separator < 0 ? hierarchyPath : hierarchyPath.Substring(0, separator);
        string relativePath = separator < 0 ? string.Empty : hierarchyPath.Substring(separator + 1);
        return root.name == rootName ? FindByRelativePath(root, relativePath) : null;
    }

    private static GameObject FindByRelativePath(GameObject root, string relativePath)
    {
        Transform child = string.IsNullOrEmpty(relativePath) ? root.transform : root.transform.Find(relativePath);
        return child != null ? child.gameObject : null;
    }
}
