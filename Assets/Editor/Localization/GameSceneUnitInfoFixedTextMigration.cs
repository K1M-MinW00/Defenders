using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.SceneManagement;

public static class GameSceneUnitInfoFixedTextMigration
{
    private const string ScenePath = "Assets/Scenes/GameScene.unity";

    private readonly struct Binding
    {
        public Binding(string path, string key) { Path = path; Key = key; }
        public string Path { get; }
        public string Key { get; }
    }

    private static readonly Binding[] Bindings =
    {
        new("Stage UI Root/UI_StageUnitInfoPanel/Panel/Content/Active/Header", "unit_info.active"),
        new("Stage UI Root/UI_StageUnitInfoPanel/Panel/Content/Passive/Header", "unit_info.passive"),
    };

    [MenuItem("Tools/Defenders/Localization/Wire Game Scene Unit Info Text")]
    public static void Apply()
    {
        LocalizationProjectSetup.Setup();
        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        foreach (Binding binding in Bindings)
        {
            GameObject target = Find(scene, binding.Path);
            TMP_Text text = target != null ? target.GetComponent<TMP_Text>() : null;
            if (text == null) throw new MissingReferenceException(binding.Path);

            LocalizeStringEvent localizer = target.GetComponent<LocalizeStringEvent>();
            if (localizer == null) localizer = Undo.AddComponent<LocalizeStringEvent>(target);
            localizer.StringReference = new LocalizedString(LocalizationTables.Stage, binding.Key);
            if (localizer.OnUpdateString.GetPersistentEventCount() == 0)
                UnityEventTools.AddPersistentListener(localizer.OnUpdateString, text.SetText);
            EditorUtility.SetDirty(localizer);
        }
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log($"[GameSceneUnitInfoFixedTextMigration] Wired {Bindings.Length} unit info labels.");
    }

    private static GameObject Find(Scene scene, string path)
    {
        int separator = path.IndexOf('/');
        string rootName = separator < 0 ? path : path.Substring(0, separator);
        string relativePath = separator < 0 ? string.Empty : path.Substring(separator + 1);
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            if (root.name != rootName) continue;
            Transform target = string.IsNullOrEmpty(relativePath) ? root.transform : root.transform.Find(relativePath);
            return target != null ? target.gameObject : null;
        }
        return null;
    }
}
