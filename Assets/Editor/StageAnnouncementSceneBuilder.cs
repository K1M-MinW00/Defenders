#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class StageAnnouncementSceneBuilder
{
    private const string ScenePath = "Assets/Scenes/GameScene.unity";

    [MenuItem("Tools/Defenders/Build Stage Announcements")]
    public static void Build()
    {
        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        GameObject stageRoot = GameObject.Find("Stage UI Root");
        GameObject currencyView = GameObject.Find("Currency View");
        StageUIController stageUI = Object.FindFirstObjectByType<StageUIController>();
        TMP_Text fontSource = currencyView?.GetComponentInChildren<TMP_Text>(true);

        if (stageRoot == null || stageUI == null || fontSource == null)
            throw new System.InvalidOperationException("Stage announcement dependencies are missing.");

        RectTransform root = GetOrCreateRect(stageRoot.transform, "UI_StageAnnouncement");
        root.gameObject.layer = LayerMask.NameToLayer("UI");
        ConfigureCanvas(root.gameObject, 14);

        CanvasGroup canvasGroup = GetOrAdd<CanvasGroup>(root.gameObject);
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        RectTransform content = GetOrCreateRect(root, "AnimatedRoot");
        Stretch(content);

        RectTransform stageIntro = BuildStageIntro(content, fontSource);
        RectTransform bossWave = BuildBossWave(content, fontSource);
        stageIntro.gameObject.SetActive(false);
        bossWave.gameObject.SetActive(false);

        StageAnnouncementUI announcement = GetOrAdd<StageAnnouncementUI>(root.gameObject);
        SerializedObject serializedAnnouncement = new(announcement);
        serializedAnnouncement.FindProperty("canvasGroup").objectReferenceValue = canvasGroup;
        serializedAnnouncement.FindProperty("animatedRoot").objectReferenceValue = content;
        serializedAnnouncement.FindProperty("stageIntroRoot").objectReferenceValue = stageIntro.gameObject;
        serializedAnnouncement.FindProperty("stageInfoText").objectReferenceValue =
            stageIntro.Find("StageInfo")?.GetComponent<TMP_Text>();
        serializedAnnouncement.FindProperty("bossWaveRoot").objectReferenceValue = bossWave.gameObject;
        serializedAnnouncement.ApplyModifiedPropertiesWithoutUndo();

        SerializedObject serializedStageUI = new(stageUI);
        serializedStageUI.FindProperty("announcementUI").objectReferenceValue = announcement;
        serializedStageUI.ApplyModifiedPropertiesWithoutUndo();

        EditorUtility.SetDirty(announcement);
        EditorUtility.SetDirty(stageUI);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[StageAnnouncementSceneBuilder] Stage announcements added to GameScene.");
    }

    private static RectTransform BuildStageIntro(Transform parent, TMP_Text fontSource)
    {
        RectTransform root = GetOrCreateRect(parent, "StageIntro");
        Center(root, new Vector2(620f, 270f));

        Image background = GetOrAdd<Image>(root.gameObject);
        background.color = new Color(0.025f, 0.055f, 0.085f, 0.88f);
        background.raycastTarget = false;

        TMP_Text label = GetOrCreateText(root, "SectorLabel", fontSource);
        ConfigureTextRect(label.rectTransform, new Vector2(0f, 58f), new Vector2(600f, 72f));
        label.text = "섹터";
        label.fontSize = 48f;
        label.color = new Color(0.36f, 1f, 0.66f, 1f);

        TMP_Text info = GetOrCreateText(root, "StageInfo", fontSource);
        ConfigureTextRect(info.rectTransform, new Vector2(0f, -44f), new Vector2(600f, 130f));
        info.text = "1-1";
        info.fontSize = 92f;
        info.color = Color.white;
        info.enableAutoSizing = true;
        info.fontSizeMin = 54f;
        info.fontSizeMax = 92f;
        return root;
    }

    private static RectTransform BuildBossWave(Transform parent, TMP_Text fontSource)
    {
        RectTransform root = GetOrCreateRect(parent, "BossWave");
        root.anchorMin = new Vector2(0f, 0.5f);
        root.anchorMax = new Vector2(1f, 0.5f);
        root.pivot = new Vector2(0.5f, 0.5f);
        root.anchoredPosition = Vector2.zero;
        root.sizeDelta = new Vector2(0f, 270f);

        Image background = GetOrAdd<Image>(root.gameObject);
        background.color = new Color(0.22f, 0.015f, 0.02f, 0.92f);
        background.raycastTarget = false;

        TMP_Text warning = GetOrCreateText(root, "Warning", fontSource);
        ConfigureTextRect(warning.rectTransform, new Vector2(0f, 65f), new Vector2(180f, 100f));
        warning.text = "!";
        warning.fontSize = 88f;
        warning.color = new Color(1f, 0.2f, 0.12f, 1f);

        TMP_Text title = GetOrCreateText(root, "Title", fontSource);
        ConfigureTextRect(title.rectTransform, new Vector2(0f, -52f), new Vector2(900f, 120f));
        title.text = "보스 웨이브";
        title.fontSize = 72f;
        title.color = Color.white;
        title.enableAutoSizing = true;
        title.fontSizeMin = 42f;
        title.fontSizeMax = 72f;
        return root;
    }

    private static TMP_Text GetOrCreateText(Transform parent, string name, TMP_Text fontSource)
    {
        RectTransform rect = GetOrCreateRect(parent, name);
        TextMeshProUGUI text = GetOrAdd<TextMeshProUGUI>(rect.gameObject);
        text.font = fontSource.font;
        text.fontSharedMaterial = fontSource.fontSharedMaterial;
        text.fontStyle = FontStyles.Bold;
        text.alignment = TextAlignmentOptions.Center;
        text.raycastTarget = false;
        return text;
    }

    private static void ConfigureTextRect(RectTransform rect, Vector2 position, Vector2 size)
    {
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
    }

    private static void ConfigureCanvas(GameObject target, int sortingOrder)
    {
        Canvas canvas = GetOrAdd<Canvas>(target);
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = sortingOrder;

        CanvasScaler scaler = GetOrAdd<CanvasScaler>(target);
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080f, 1920f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
    }

    private static RectTransform GetOrCreateRect(Transform parent, string name)
    {
        if (parent.Find(name) is RectTransform existing)
            return existing;

        GameObject child = new(name, typeof(RectTransform));
        child.layer = LayerMask.NameToLayer("UI");
        RectTransform rect = child.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        return rect;
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.localScale = Vector3.one;
    }

    private static void Center(RectTransform rect, Vector2 size)
    {
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = size;
    }

    private static T GetOrAdd<T>(GameObject target) where T : Component
    {
        return target.TryGetComponent(out T component) ? component : target.AddComponent<T>();
    }
}
#endif
