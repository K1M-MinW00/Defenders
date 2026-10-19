using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class LobbyTypographyAudit
{
    [MenuItem("Tools/Defenders/UI Typography/Audit Lobby Scene")]
    public static void Audit()
    {
        Scene scene = EditorSceneManager.OpenScene(LobbyLocalizationAudit.LobbyScenePath, OpenSceneMode.Single);
        TMP_Text[] texts = scene.GetRootGameObjects()
            .SelectMany(root => root.GetComponentsInChildren<TMP_Text>(true))
            .OrderBy(text => GetPath(text.transform))
            .ToArray();

        List<TMP_Text> ellipsis = texts.Where(text => text.overflowMode == TextOverflowModes.Ellipsis).ToList();
        List<TMP_Text> noAutoSize = texts.Where(text => !text.enableAutoSizing).ToList();
        List<TMP_Text> automaticWrap = texts.Where(text =>
            text.textWrappingMode == TextWrappingModes.Normal ||
            text.textWrappingMode == TextWrappingModes.PreserveWhitespace).ToList();

        StringBuilder report = new();
        report.AppendLine($"TOTAL={texts.Length} ELLIPSIS={ellipsis.Count} AUTO_SIZE_OFF={noAutoSize.Count} AUTO_WRAP={automaticWrap.Count}");
        Append(report, "ELLIPSIS", ellipsis);
        Append(report, "AUTO_SIZE_OFF", noAutoSize);
        Append(report, "AUTO_WRAP", automaticWrap);
        Debug.Log("[LobbyTypographyAudit]\n" + report);
    }

    private static void Append(StringBuilder report, string heading, IEnumerable<TMP_Text> texts)
    {
        report.AppendLine($"[{heading}]");
        foreach (TMP_Text text in texts)
        {
            Rect rect = text.rectTransform.rect;
            report.AppendLine(
                $"{GetPath(text.transform)} | text='{text.text.Replace("\n", "\\n")}' | " +
                $"rect={rect.width:0.#}x{rect.height:0.#} font={text.fontSize:0.#} " +
                $"auto={text.enableAutoSizing}({text.fontSizeMin:0.#}-{text.fontSizeMax:0.#}) " +
                $"wrap={text.textWrappingMode} overflow={text.overflowMode}");
        }
    }

    private static string GetPath(Transform transform)
    {
        Stack<string> parts = new();
        while (transform != null) { parts.Push(transform.name); transform = transform.parent; }
        return string.Join("/", parts);
    }
}
