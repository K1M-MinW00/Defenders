using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.SceneManagement;

public static class LobbyAccountExitFixedTextMigration
{
    private readonly struct Binding
    {
        public Binding(string path, string key) { Path = path; Key = key; }
        public string Path { get; }
        public string Key { get; }
    }

    private static readonly Binding[] Bindings =
    {
        new("LobbyScreenCanvas/AccountLinkOverlay/DismissHintText", "common.tap_to_close"),
        new("LobbyScreenCanvas/AccountLinkOverlay/ModalCard/Header/Text", "settings.account_management"),
        new("LobbyScreenCanvas/AccountLinkOverlay/ModalCard/GoogleSignInSection/ProviderText", "settings.google_sign_in"),
        new("LobbyScreenCanvas/AccountLinkOverlay/ModalCard/GoogleSignInSection/Button/Text", "settings.link"),
        new("LobbyScreenCanvas/AccountLinkOverlay/ModalCard/AppleSignInSection/ProviderText", "settings.apple_sign_in"),
        new("LobbyScreenCanvas/AccountLinkOverlay/ModalCard/AppleSignInSection/Button/Text", "settings.link"),
        new("LobbyScreenCanvas/AccountLinkOverlay/ModalCard/ActionButtons/LogoutButton/Text", "settings.logout"),
        new("LobbyScreenCanvas/AccountLinkOverlay/ModalCard/ActionButtons/WithdrawButton/Text", "settings.withdraw"),
        new("LobbyScreenCanvas/ExitConfirmationOverlay/ModalCard/Header/Text", "exit.title"),
        new("LobbyScreenCanvas/ExitConfirmationOverlay/ModalCard/MessageText", "exit.message"),
        new("LobbyScreenCanvas/ExitConfirmationOverlay/ModalCard/ConfirmButton/Text", "exit.confirm"),
        new("LobbyScreenCanvas/ExitConfirmationOverlay/ModalCard/CancelButton/Text", "common.cancel"),
    };

    [MenuItem("Tools/Defenders/Localization/Wire Account and Exit Fixed Text")]
    public static void Apply()
    {
        LocalizationProjectSetup.Setup();
        Scene scene = EditorSceneManager.OpenScene(LobbyLocalizationAudit.LobbyScenePath, OpenSceneMode.Single);
        foreach (Binding binding in Bindings)
        {
            GameObject target = Find(scene, binding.Path);
            TMP_Text text = target != null ? target.GetComponent<TMP_Text>() : null;
            if (text == null) throw new MissingReferenceException(binding.Path);

            LocalizeStringEvent localizer = target.GetComponent<LocalizeStringEvent>();
            if (localizer == null) localizer = Undo.AddComponent<LocalizeStringEvent>(target);
            localizer.StringReference = new LocalizedString(LocalizationTables.Common, binding.Key);
            if (localizer.OnUpdateString.GetPersistentEventCount() == 0)
                UnityEventTools.AddPersistentListener(localizer.OnUpdateString, text.SetText);
            EditorUtility.SetDirty(localizer);
        }
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log($"[LobbyAccountExitFixedTextMigration] Wired {Bindings.Length} account and exit labels.");
    }

    private static GameObject Find(Scene scene, string path)
    {
        int separator = path.IndexOf('/');
        string rootName = separator < 0 ? path : path.Substring(0, separator);
        string relativePath = separator < 0 ? string.Empty : path.Substring(separator + 1);
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            if (root.name != rootName) continue;
            Transform child = string.IsNullOrEmpty(relativePath) ? root.transform : root.transform.Find(relativePath);
            return child != null ? child.gameObject : null;
        }
        return null;
    }
}
