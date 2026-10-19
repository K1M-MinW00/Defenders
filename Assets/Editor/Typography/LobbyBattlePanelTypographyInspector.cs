using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class LobbyBattlePanelTypographyInspector
{
    private const string PanelPath = "LobbyScreenCanvas/LobbyScreenRoot/BattlePanel";

    [MenuItem("Tools/Defenders/UI Typography/Apply Lobby Battle Panel")]
    public static void Apply()
    {
        Scene scene = EditorSceneManager.OpenScene(LobbyLocalizationAudit.LobbyScenePath, OpenSceneMode.Single);

        Configure(scene, PanelPath + "/PanelLowerButton/ButtonChallenge/Text", 24f, 28f);
        Configure(scene, PanelPath + "/PanelLowerButton/ButtonGuide/Text", 24f, 28f);
        Configure(scene, PanelPath + "/PanelLowerButton/ButtonGo/Text", 72f, 90f);
        Configure(scene, PanelPath + "/PanelLowerButton/ButtonGo/PanelBattery/ValueText", 24f, 28f);

        Configure(scene, PanelPath + "/TopBar/FuelPanel/FuelText", 18f, 24f);
        Configure(scene, PanelPath + "/TopBar/GemPanel/Text", 20f, 28f);
        Configure(scene, PanelPath + "/TopBar/GoldPanel/ValueText", 20f, 28f);
        Configure(scene, PanelPath + "/TopBar/LevelAndPower/LevelPanel/Level/LevelText", 16f, 20f);
        Configure(scene, PanelPath + "/TopBar/LevelAndPower/PowerPanel/PowerText", 16f, 20f);

        Configure(scene, PanelPath + "/UpperLeftButtons/AttendanceEventButton/Text", 18f, 20f);
        Configure(scene, PanelPath + "/UpperLeftButtons/BattlePassButton/Text", 18f, 20f);
        Configure(scene, PanelPath + "/UpperLeftButtons/GrowthEventButton/Text", 18f, 20f);
        Configure(scene, PanelPath + "/UpperLeftButtons/PrivilegeButton/Text", 18f, 20f);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[LobbyBattlePanelTypographyInspector] Configured the remaining battle panel labels.");
    }

    [MenuItem("Tools/Defenders/UI Typography/Inspect Lobby Battle Panel")]
    public static void Inspect()
    {
        Scene scene = EditorSceneManager.OpenScene(LobbyLocalizationAudit.LobbyScenePath, OpenSceneMode.Single);
        GameObject panel = FindByPath(scene, PanelPath);
        if (panel == null) throw new MissingReferenceException(PanelPath);

        Canvas.ForceUpdateCanvases();
        StringBuilder report = new();
        CanvasScaler scaler = panel.GetComponentInParent<CanvasScaler>();
        if (scaler != null)
            report.AppendLine($"Canvas reference resolution: {scaler.referenceResolution}, match={scaler.matchWidthOrHeight:0.##}");

        TMP_Text[] texts = panel.GetComponentsInChildren<TMP_Text>(true);
        report.AppendLine($"TMP count: {texts.Length}");
        foreach (TMP_Text text in texts.OrderBy(item => GetPath(item.transform)))
        {
            RectTransform rect = text.rectTransform;
            report.AppendLine(
                $"{GetPath(text.transform)} | active={text.gameObject.activeInHierarchy} | text='{text.text.Replace("\n", "\\n")}' | " +
                $"rect={rect.rect.width:0.#}x{rect.rect.height:0.#}, position={rect.anchoredPosition} | " +
                $"font={text.fontSize:0.#}, auto={text.enableAutoSizing}({text.fontSizeMin:0.#}-{text.fontSizeMax:0.#}), " +
                $"wrap={text.textWrappingMode}, overflow={text.overflowMode}, preferred={text.preferredWidth:0.#}x{text.preferredHeight:0.#}");
        }

        Debug.Log("[LobbyBattlePanelTypographyInspector]\n" + report);
    }

    private static GameObject FindByPath(Scene scene, string path)
    {
        string[] parts = path.Split('/');
        GameObject root = scene.GetRootGameObjects().FirstOrDefault(candidate => candidate.name == parts[0]);
        if (root == null) return null;
        Transform target = parts.Length == 1 ? root.transform : root.transform.Find(string.Join("/", parts.Skip(1)));
        return target != null ? target.gameObject : null;
    }

    private static void Configure(Scene scene, string path, float minimum, float maximum)
    {
        TMP_Text text = FindByPath(scene, path)?.GetComponent<TMP_Text>();
        if (text == null) throw new MissingReferenceException(path);
        Undo.RecordObject(text, "Configure battle panel typography");
        text.fontSize = maximum;
        text.enableAutoSizing = true;
        text.fontSizeMin = minimum;
        text.fontSizeMax = maximum;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        text.overflowMode = TextOverflowModes.Overflow;
        EditorUtility.SetDirty(text);
    }

    private static string GetPath(Transform transform)
    {
        List<string> parts = new();
        while (transform != null) { parts.Add(transform.name); transform = transform.parent; }
        parts.Reverse();
        return string.Join("/", parts);
    }
}
