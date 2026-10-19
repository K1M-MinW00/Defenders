using System.Linq;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class LobbyTMPPanelInspector
{
    private const string BottomNavigationPath = "LobbyScreenCanvas/LobbyScreenRoot/BottomNavigationBar";
    private static readonly string[] BottomNavigationLabelPaths =
    {
        BottomNavigationPath + "/RecruitButton/Text",
        BottomNavigationPath + "/UnitButton/Text",
        BottomNavigationPath + "/BattleButton/Text",
        BottomNavigationPath + "/LabButton/Text",
        BottomNavigationPath + "/ShopButton/Text",
    };
    private static readonly string[] BottomNavigationEnglish =
    {
        "Recruit", "Units", "Battle", "Lab", "Shop",
    };

    [MenuItem("Tools/Defenders/UI Typography/Apply Lobby Bottom Navigation")]
    public static void ApplyBottomNavigation()
    {
        Scene scene = EditorSceneManager.OpenScene(LobbyLocalizationAudit.LobbyScenePath, OpenSceneMode.Single);
        foreach (string path in BottomNavigationLabelPaths)
        {
            GameObject target = FindByPath(scene, path);
            TMP_Text text = target != null ? target.GetComponent<TMP_Text>() : null;
            if (text == null) throw new MissingReferenceException(path);

            Undo.RecordObject(text, "Configure lobby bottom navigation typography");
            text.fontSize = 50f;
            text.enableAutoSizing = true;
            text.fontSizeMin = 44f;
            text.fontSizeMax = 50f;
            text.textWrappingMode = TextWrappingModes.NoWrap;
            text.overflowMode = TextOverflowModes.Overflow;
            EditorUtility.SetDirty(text);
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log($"[LobbyTMPPanelInspector] Configured {BottomNavigationLabelPaths.Length} bottom navigation labels.");
    }

    [MenuItem("Tools/Defenders/UI Typography/Inspect Lobby Bottom Navigation")]
    public static void InspectBottomNavigation()
    {
        Scene scene = EditorSceneManager.OpenScene(LobbyLocalizationAudit.LobbyScenePath, OpenSceneMode.Single);
        GameObject panel = FindByPath(scene, BottomNavigationPath);
        if (panel == null) throw new MissingReferenceException(BottomNavigationPath);

        Canvas.ForceUpdateCanvases();

        StringBuilder report = new();
        CanvasScaler scaler = panel.GetComponentInParent<CanvasScaler>();
        if (scaler != null)
            report.AppendLine($"Canvas reference resolution: {scaler.referenceResolution}, match={scaler.matchWidthOrHeight:0.##}");
        RectTransform panelRect = (RectTransform)panel.transform;
        report.AppendLine($"Panel: rect={panelRect.rect.width:0.#}x{panelRect.rect.height:0.#}, anchors={panelRect.anchorMin}->{panelRect.anchorMax}, sizeDelta={panelRect.sizeDelta}");
        TMP_Text[] texts = panel.GetComponentsInChildren<TMP_Text>(true);
        foreach (TMP_Text text in texts)
        {
            RectTransform textRect = (RectTransform)text.transform;
            Rect rect = textRect.rect;
            RectTransform parentRect = textRect.parent as RectTransform;
            report.AppendLine(
                $"{GetPath(text.transform)} | text='{text.text.Replace("\n", " ")}' | rect={rect.width:0.#}x{rect.height:0.#} | " +
                $"parent={parentRect?.rect.width:0.#}x{parentRect?.rect.height:0.#} | anchors={textRect.anchorMin}->{textRect.anchorMax}, offsets={textRect.offsetMin}->{textRect.offsetMax} | " +
                $"font={text.fontSize:0.#}, auto={text.enableAutoSizing}({text.fontSizeMin:0.#}-{text.fontSizeMax:0.#}), " +
                $"wrap={text.textWrappingMode}, overflow={text.overflowMode}, preferred={text.preferredWidth:0.#}x{text.preferredHeight:0.#}");
        }
        for (int index = 0; index < BottomNavigationLabelPaths.Length; index++)
        {
            TMP_Text text = FindByPath(scene, BottomNavigationLabelPaths[index]).GetComponent<TMP_Text>();
            Vector2 preferred = text.GetPreferredValues(BottomNavigationEnglish[index]);
            report.AppendLine($"English '{BottomNavigationEnglish[index]}' at {text.fontSize:0.#}: preferred={preferred.x:0.#}x{preferred.y:0.#}, available={text.rectTransform.rect.width:0.#}x{text.rectTransform.rect.height:0.#}");
        }
        Debug.Log("[LobbyTMPPanelInspector]\n" + report);
    }

    private static GameObject FindByPath(Scene scene, string path)
    {
        string[] parts = path.Split('/');
        GameObject root = scene.GetRootGameObjects().FirstOrDefault(candidate => candidate.name == parts[0]);
        if (root == null) return null;
        Transform target = parts.Length == 1 ? root.transform : root.transform.Find(string.Join("/", parts.Skip(1)));
        return target != null ? target.gameObject : null;
    }

    private static string GetPath(Transform transform)
    {
        System.Collections.Generic.List<string> parts = new();
        while (transform != null) { parts.Add(transform.name); transform = transform.parent; }
        parts.Reverse();
        return string.Join("/", parts);
    }
}
