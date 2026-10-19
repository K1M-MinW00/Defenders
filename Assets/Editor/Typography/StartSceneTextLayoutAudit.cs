using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.Localization;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Localization.Tables;
using UnityEngine.SceneManagement;

public static class StartSceneTextLayoutAudit
{
    private const string ScenePath = "Assets/Scenes/StartScene.unity";

    private static readonly string[] ActionKeys =
    {
        "startup.action.start",
        "startup.action.retry",
    };

    private static readonly string[] StatusKeys =
    {
        "startup.status.initializing",
        "startup.status.checking_login",
        "startup.status.loading_user_data",
        "startup.status.ready",
        "startup.status.loading_lobby",
        "startup.error.boot_failed",
        "startup.error.login_failed",
        "startup.error.user_data_load_failed",
        "startup.error.required_service_missing",
        "startup.error.lobby_scene_missing",
        "startup.error.scene_already_loading",
        "startup.error.lobby_load_failed",
    };

    [MenuItem("Tools/Defenders/Localization/Audit Start Scene Text Layout")]
    public static void Run()
    {
        LocalizationProjectSetup.Setup();
        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        TMP_Text action = FindText(scene, "Start Screen Canvas/Start Screen/Start Button/Start Button Label");
        TMP_Text status = FindText(scene, "Start Screen Canvas/Start Screen/Loading Progress/Fill Area/Loading Status Text");
        Canvas.ForceUpdateCanvases();

        List<string> failures = new();
        Audit(action, ActionKeys, failures);
        Audit(status, StatusKeys, failures);
        if (failures.Count > 0)
            throw new System.InvalidOperationException(string.Join("\n", failures));

        Debug.Log($"[StartSceneTextLayoutAudit] Passed {2 * (ActionKeys.Length + StatusKeys.Length)} Korean/English samples.");
    }

    private static void Audit(TMP_Text text, IEnumerable<string> keys, ICollection<string> failures)
    {
        Rect rect = text.rectTransform.rect;
        foreach (string locale in new[] { "ko", "en" })
        {
            StringTableCollection collection = LocalizationEditorSettings.GetStringTableCollection(LocalizationTables.Common);
            StringTable table = collection?.GetTable(locale) as StringTable;
            foreach (string key in keys)
            {
                string value = table?.GetEntry(key)?.Value ?? string.Empty;
                Vector2 preferred = text.GetPreferredValues(value, float.PositiveInfinity, float.PositiveInfinity);
                if (string.IsNullOrWhiteSpace(value) || preferred.x > rect.width + 0.5f || preferred.y > rect.height + 0.5f)
                    failures.Add($"[{locale}] {key}: '{value}' preferred={preferred.x:0.#}x{preferred.y:0.#}, rect={rect.width:0.#}x{rect.height:0.#}");
            }
        }
    }

    private static TMP_Text FindText(Scene scene, string path)
    {
        int separator = path.IndexOf('/');
        string rootName = path.Substring(0, separator);
        string relativePath = path.Substring(separator + 1);
        GameObject root = scene.GetRootGameObjects().FirstOrDefault(item => item.name == rootName);
        Transform target = root != null ? root.transform.Find(relativePath) : null;
        TMP_Text text = target != null ? target.GetComponent<TMP_Text>() : null;
        return text != null ? text : throw new MissingReferenceException(path);
    }
}
