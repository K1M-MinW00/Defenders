using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.SceneManagement;

public static class GameSceneHudMonsterFixedTextMigration
{
    private const string ScenePath = "Assets/Scenes/GameScene.unity";

    private readonly struct Binding
    {
        public Binding(string path, string table, string key) { Path = path; Table = table; Key = key; }
        public string Path { get; }
        public string Table { get; }
        public string Key { get; }
    }

    private static readonly Binding[] Bindings =
    {
        new("Stage UI Root/Stage HUD Canvas/Top HUD/Wave Track View/Label_Img/Label_Text", LocalizationTables.Stage, "hud.wave"),
        new("Stage UI Root/Stage Popup Canvas/Monster Info Popup/Close Instruction", LocalizationTables.Common, "common.tap_to_close"),
        new("Stage UI Root/Stage Popup Canvas/Monster Info Popup/Panel/Header/Title", LocalizationTables.Stage, "monster.info.title"),
        new("Stage UI Root/Stage Popup Canvas/Monster Info Popup/Panel/Scroll View/Viewport/Content/Empty Text", LocalizationTables.Stage, "monster.info.empty"),
    };

    [MenuItem("Tools/Defenders/Localization/Wire Game Scene HUD and Monster Text")]
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
            localizer.StringReference = new LocalizedString(binding.Table, binding.Key);
            if (localizer.OnUpdateString.GetPersistentEventCount() == 0)
                UnityEventTools.AddPersistentListener(localizer.OnUpdateString, text.SetText);
            EditorUtility.SetDirty(localizer);
        }
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log($"[GameSceneHudMonsterFixedTextMigration] Wired {Bindings.Length} HUD and monster labels.");
    }

    private static GameObject Find(Scene scene, string path)
    {
        int separator = path.IndexOf('/');
        string rootName = path.Substring(0, separator);
        string relativePath = path.Substring(separator + 1);
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            if (root.name != rootName) continue;
            Transform target = root.transform.Find(relativePath);
            return target != null ? target.gameObject : null;
        }
        return null;
    }
}
