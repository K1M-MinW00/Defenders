using System;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.SceneManagement;

public static class LobbyNavigationLocalizationMigration
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
        new("LobbyScreenCanvas/LobbyScreenRoot/BottomNavigationBar/RecruitButton/Text", "nav.recruit"),
        new("LobbyScreenCanvas/LobbyScreenRoot/BottomNavigationBar/UnitButton/Text", "nav.units"),
        new("LobbyScreenCanvas/LobbyScreenRoot/BottomNavigationBar/BattleButton/Text", "nav.battle"),
        new("LobbyScreenCanvas/LobbyScreenRoot/BottomNavigationBar/LabButton/Text", "nav.lab"),
        new("LobbyScreenCanvas/LobbyScreenRoot/BottomNavigationBar/ShopButton/Text", "nav.shop"),
        new("LobbyScreenCanvas/LobbyScreenRoot/BattlePanel/PanelReward/ButtonSupply/Text", "shortcut.supply"),
        new("LobbyScreenCanvas/LobbyScreenRoot/BattlePanel/PanelReward/ButtonReward/Text", "shortcut.conquest_rewards"),
        new("LobbyScreenCanvas/LobbyScreenRoot/BattlePanel/UpperRightButtons/MailBoxButton/Text", "shortcut.mailbox"),
        new("LobbyScreenCanvas/LobbyScreenRoot/BattlePanel/UpperRightButtons/BagButton/Text", "shortcut.inventory"),
        new("LobbyScreenCanvas/LobbyScreenRoot/BattlePanel/UpperRightButtons/QuestButton/Text", "shortcut.quests"),
    };

    [MenuItem("Tools/Defenders/Localization/Wire Lobby Navigation Text")]
    public static void WireLobbyNavigationText()
    {
        LocalizationProjectSetup.Setup();
        Scene scene = EditorSceneManager.OpenScene(LobbyLocalizationAudit.LobbyScenePath, OpenSceneMode.Single);

        foreach (Binding binding in Bindings)
        {
            GameObject target = GameObject.Find(binding.HierarchyPath);
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
        Debug.Log($"[LobbyNavigationLocalizationMigration] Wired {Bindings.Length} lobby navigation labels.");
    }
}
