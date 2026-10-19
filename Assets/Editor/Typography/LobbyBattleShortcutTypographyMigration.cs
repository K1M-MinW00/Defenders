using System.Linq;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class LobbyBattleShortcutTypographyMigration
{
    private const string RewardPanelPath = "LobbyScreenCanvas/LobbyScreenRoot/BattlePanel/PanelReward";
    private static readonly string[] LabelPaths =
    {
        "LobbyScreenCanvas/LobbyScreenRoot/BattlePanel/PanelReward/ButtonSupply/Text",
        "LobbyScreenCanvas/LobbyScreenRoot/BattlePanel/PanelReward/ButtonReward/Text",
        "LobbyScreenCanvas/LobbyScreenRoot/BattlePanel/UpperRightButtons/MailBoxButton/Text",
        "LobbyScreenCanvas/LobbyScreenRoot/BattlePanel/UpperRightButtons/BagButton/Text",
        "LobbyScreenCanvas/LobbyScreenRoot/BattlePanel/UpperRightButtons/QuestButton/Text",
    };

    private static readonly string[] Korean = { "보급품", "정복 보상", "우편함", "가방", "퀘스트" };
    private static readonly string[] English = { "Supplies", "Conquest Rewards", "Mailbox", "Bag", "Quests" };

    [MenuItem("Tools/Defenders/UI Typography/Apply Lobby Battle Shortcuts")]
    public static void Apply()
    {
        Scene scene = EditorSceneManager.OpenScene(LobbyLocalizationAudit.LobbyScenePath, OpenSceneMode.Single);

        RectTransform rewardPanel = FindByPath(scene, RewardPanelPath)?.GetComponent<RectTransform>();
        RectTransform supplyButton = FindByPath(scene, RewardPanelPath + "/ButtonSupply")?.GetComponent<RectTransform>();
        RectTransform rewardButton = FindByPath(scene, RewardPanelPath + "/ButtonReward")?.GetComponent<RectTransform>();
        TMP_Text rewardText = FindByPath(scene, LabelPaths[1])?.GetComponent<TMP_Text>();
        if (rewardPanel == null || supplyButton == null || rewardButton == null || rewardText == null)
            throw new MissingReferenceException(RewardPanelPath);

        Undo.RecordObjects(new Object[] { rewardPanel, supplyButton, rewardButton, rewardText.rectTransform }, "Resize battle reward shortcuts");
        rewardPanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 440f);
        supplyButton.anchoredPosition = new Vector2(90f, supplyButton.anchoredPosition.y);
        rewardButton.anchoredPosition = new Vector2(320f, rewardButton.anchoredPosition.y);
        rewardText.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 240f);

        foreach (string path in LabelPaths)
        {
            TMP_Text text = FindByPath(scene, path)?.GetComponent<TMP_Text>();
            if (text == null) throw new MissingReferenceException(path);
            Undo.RecordObject(text, "Configure battle shortcut typography");
            text.fontSize = 22f;
            text.enableAutoSizing = true;
            text.fontSizeMin = 20f;
            text.fontSizeMax = 22f;
            text.textWrappingMode = TextWrappingModes.NoWrap;
            text.overflowMode = TextOverflowModes.Overflow;
            EditorUtility.SetDirty(text);
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log($"[LobbyBattleShortcutTypographyMigration] Configured {LabelPaths.Length} shortcut labels and resized the reward group.");
    }

    [MenuItem("Tools/Defenders/UI Typography/Inspect Lobby Battle Shortcuts")]
    public static void Inspect()
    {
        Scene scene = EditorSceneManager.OpenScene(LobbyLocalizationAudit.LobbyScenePath, OpenSceneMode.Single);
        Canvas.ForceUpdateCanvases();
        StringBuilder report = new();
        CanvasScaler scaler = scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<CanvasScaler>(true)).FirstOrDefault();
        if (scaler != null)
            report.AppendLine($"Canvas reference resolution: {scaler.referenceResolution}, match={scaler.matchWidthOrHeight:0.##}");

        for (int index = 0; index < LabelPaths.Length; index++)
        {
            TMP_Text text = FindByPath(scene, LabelPaths[index])?.GetComponent<TMP_Text>();
            if (text == null) throw new MissingReferenceException(LabelPaths[index]);
            RectTransform rect = text.rectTransform;
            RectTransform parent = rect.parent as RectTransform;
            RectTransform group = parent != null ? parent.parent as RectTransform : null;
            Vector2 koPreferred = text.GetPreferredValues(Korean[index]);
            Vector2 enPreferred = text.GetPreferredValues(English[index]);
            report.AppendLine(
                $"{LabelPaths[index]} | current='{text.text.Replace("\n", " ")}' | rect={rect.rect.width:0.#}x{rect.rect.height:0.#} | " +
                $"parent={parent?.rect.width:0.#}x{parent?.rect.height:0.#} | anchors={rect.anchorMin}->{rect.anchorMax}, offsets={rect.offsetMin}->{rect.offsetMax} | " +
                $"buttonAnchors={parent?.anchorMin}->{parent?.anchorMax}, buttonPos={parent?.anchoredPosition}, group={group?.name} {group?.rect.width:0.#}x{group?.rect.height:0.#} " +
                $"groupAnchors={group?.anchorMin}->{group?.anchorMax}, groupPos={group?.anchoredPosition} | " +
                $"font={text.fontSize:0.#}, auto={text.enableAutoSizing}({text.fontSizeMin:0.#}-{text.fontSizeMax:0.#}), wrap={text.textWrappingMode}, overflow={text.overflowMode} | " +
                $"ko='{Korean[index]}' {koPreferred.x:0.#}x{koPreferred.y:0.#}, en='{English[index]}' {enPreferred.x:0.#}x{enPreferred.y:0.#}");
        }

        Debug.Log("[LobbyBattleShortcutTypographyMigration]\n" + report);
    }

    private static GameObject FindByPath(Scene scene, string path)
    {
        string[] parts = path.Split('/');
        GameObject root = scene.GetRootGameObjects().FirstOrDefault(candidate => candidate.name == parts[0]);
        if (root == null) return null;
        Transform target = parts.Length == 1 ? root.transform : root.transform.Find(string.Join("/", parts.Skip(1)));
        return target != null ? target.gameObject : null;
    }
}
