using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SettingsPanelLocalizationMigration
{
    private const string LobbyScenePath = "Assets/Scenes/LobbyScene.unity";

    [MenuItem("Tools/Defenders/Localization/Wire Settings Panel Text")]
    public static void WireSettingsPanelText()
    {
        Scene scene = EditorSceneManager.OpenScene(LobbyScenePath, OpenSceneMode.Single);
        SettingsPanelView view = Object.FindFirstObjectByType<SettingsPanelView>(FindObjectsInactive.Include);
        if (view == null)
            throw new MissingReferenceException("SettingsPanelView was not found in LobbyScene.");

        SerializedObject serializedView = new(view);
        Assign(serializedView, "titleText", view.transform, "ModalCard/Header/Label");
        Assign(serializedView, "soundLabelText", view.transform, "ModalCard/Sound_Panel/Label");
        Assign(serializedView, "languageLabelText", view.transform, "ModalCard/Langauge_Panel/Label");
        Assign(serializedView, "accountLabelText", view.transform, "ModalCard/Account_Panel/Account");
        Assign(serializedView, "copyUidButtonText", view.transform, "ModalCard/Uid_Panel/CopyUid_Button/Label");
        Assign(serializedView, "linkAccountButtonText", view.transform, "ModalCard/Account_Panel/LinkAccount_Button/Label");
        Assign(serializedView, "dismissHintText", view.transform, "DismissHint");
        serializedView.ApplyModifiedPropertiesWithoutUndo();

        EditorUtility.SetDirty(view);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[SettingsPanelLocalizationMigration] Settings panel text references are wired.");
    }

    private static void Assign(
        SerializedObject serializedView,
        string propertyName,
        Transform root,
        string relativePath)
    {
        Transform target = root.Find(relativePath);
        TMP_Text text = target != null ? target.GetComponent<TMP_Text>() : null;
        if (text == null)
            throw new MissingReferenceException($"TMP text was not found at '{relativePath}'.");

        SerializedProperty property = serializedView.FindProperty(propertyName);
        if (property == null)
            throw new MissingReferenceException($"Serialized property '{propertyName}' was not found.");

        property.objectReferenceValue = text;
    }
}
