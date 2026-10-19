using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class LobbyBattleSectorTypographyMigration
{
    private const string PanelPath = "LobbyScreenCanvas/LobbyScreenRoot/BattlePanel/PanelSector";
    private const string StageTextPath = PanelPath + "/TextStageText";
    private const string BestWaveTextPath = PanelPath + "/TextGroupWave/TextBestText";
    private const string WaveValueTextPath = PanelPath + "/TextGroupWave/TextWaveText";

    [MenuItem("Tools/Defenders/UI Typography/Apply Lobby Battle Sector")]
    public static void Apply()
    {
        Scene scene = EditorSceneManager.OpenScene(LobbyLocalizationAudit.LobbyScenePath, OpenSceneMode.Single);
        Configure(scene, StageTextPath, 70f);
        Configure(scene, BestWaveTextPath, 22f);
        Configure(scene, WaveValueTextPath, 20f);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[LobbyBattleSectorTypographyMigration] Configured the sector and best-wave labels.");
    }

    [MenuItem("Tools/Defenders/UI Typography/Inspect Lobby Battle Sector")]
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

        RectTransform panelRect = (RectTransform)panel.transform;
        report.AppendLine($"Panel: rect={panelRect.rect.width:0.#}x{panelRect.rect.height:0.#}, anchors={panelRect.anchorMin}->{panelRect.anchorMax}, position={panelRect.anchoredPosition}");
        foreach (TMP_Text text in panel.GetComponentsInChildren<TMP_Text>(true))
        {
            RectTransform rect = text.rectTransform;
            Vector2 sectorKo = text.GetPreferredValues("섹터\n10-10");
            Vector2 sectorEn = text.GetPreferredValues("Sector\n10-10");
            Vector2 bestEn = text.GetPreferredValues("Best Wave");
            report.AppendLine(
                $"{GetPath(text.transform)} | current='{text.text.Replace("\n", "\\n")}' | rect={rect.rect.width:0.#}x{rect.rect.height:0.#} | " +
                $"anchors={rect.anchorMin}->{rect.anchorMax}, position={rect.anchoredPosition}, sizeDelta={rect.sizeDelta} | " +
                $"font={text.fontSize:0.#}, auto={text.enableAutoSizing}({text.fontSizeMin:0.#}-{text.fontSizeMax:0.#}), wrap={text.textWrappingMode}, overflow={text.overflowMode}, " +
                $"preferredCurrent={text.preferredWidth:0.#}x{text.preferredHeight:0.#}, sectorKo={sectorKo.x:0.#}x{sectorKo.y:0.#}, sectorEn={sectorEn.x:0.#}x{sectorEn.y:0.#}, bestEn={bestEn.x:0.#}x{bestEn.y:0.#}");
        }

        Debug.Log("[LobbyBattleSectorTypographyMigration]\n" + report);
    }

    private static GameObject FindByPath(Scene scene, string path)
    {
        string[] parts = path.Split('/');
        GameObject root = scene.GetRootGameObjects().FirstOrDefault(candidate => candidate.name == parts[0]);
        if (root == null) return null;
        Transform target = parts.Length == 1 ? root.transform : root.transform.Find(string.Join("/", parts.Skip(1)));
        return target != null ? target.gameObject : null;
    }

    private static void Configure(Scene scene, string path, float fontSize)
    {
        TMP_Text text = FindByPath(scene, path)?.GetComponent<TMP_Text>();
        if (text == null) throw new MissingReferenceException(path);
        Undo.RecordObject(text, "Configure battle sector typography");
        text.fontSize = fontSize;
        text.enableAutoSizing = false;
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
