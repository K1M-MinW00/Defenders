using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class FixedRuntimeUiSceneMigration
{
    private const string LobbyScenePath = "Assets/Scenes/LobbyScene.unity";
    private const string GameScenePath = "Assets/Scenes/GameScene.unity";

    [MenuItem("Tools/UI/Configure Fixed Runtime UI")]
    public static void ConfigureScenes()
    {
        ConfigureScene(LobbyScenePath, includeItemTooltip: true);
        ConfigureScene(GameScenePath, includeItemTooltip: true);
        Debug.Log("[FixedRuntimeUiSceneMigration] Fixed tooltip and feedback UI configured.");
    }

    private static void ConfigureScene(string scenePath, bool includeItemTooltip)
    {
        Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        if (includeItemTooltip)
            ConfigureItemTooltip();
        ConfigureFeedbackToast();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    private static void ConfigureItemTooltip()
    {
        ItemTooltipUI view = Object.FindFirstObjectByType<ItemTooltipUI>(FindObjectsInactive.Include);
        if (view == null)
        {
            Canvas canvas = Object.FindFirstObjectByType<Canvas>(FindObjectsInactive.Include).rootCanvas;
            GameObject root = CreateUiObject("ItemTooltip", canvas.transform, typeof(CanvasGroup), typeof(Image), typeof(ItemTooltipUI));
            Stretch(root.GetComponent<RectTransform>());
            root.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.001f);

            GameObject panel = CreateUiObject("Panel", root.transform, typeof(Image));
            RectTransform panelRect = panel.GetComponent<RectTransform>();
            panelRect.sizeDelta = new Vector2(260f, 64f);
            panel.GetComponent<Image>().color = new Color(0.06f, 0.06f, 0.08f, 0.94f);

            GameObject labelObject = CreateUiObject("Name", panel.transform, typeof(TextMeshProUGUI));
            RectTransform labelRect = labelObject.GetComponent<RectTransform>();
            Stretch(labelRect, 12f, 8f, -12f, -8f);
            TextMeshProUGUI label = labelObject.GetComponent<TextMeshProUGUI>();
            label.alignment = TextAlignmentOptions.Center;
            label.color = Color.white;
            label.enableAutoSizing = true;
            label.fontSizeMin = 14f;
            label.fontSizeMax = 26f;

            view = root.GetComponent<ItemTooltipUI>();
            SerializedObject serialized = new SerializedObject(view);
            serialized.FindProperty("canvasGroup").objectReferenceValue = root.GetComponent<CanvasGroup>();
            serialized.FindProperty("panel").objectReferenceValue = panelRect;
            serialized.FindProperty("label").objectReferenceValue = label;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        Canvas tooltipCanvas = view.GetComponent<Canvas>();
        if (tooltipCanvas == null)
            tooltipCanvas = view.gameObject.AddComponent<Canvas>();
        tooltipCanvas.overrideSorting = true;
        tooltipCanvas.sortingOrder = 2000;

        if (view.GetComponent<GraphicRaycaster>() == null)
            view.gameObject.AddComponent<GraphicRaycaster>();

        view.gameObject.SetActive(true);
        view.transform.SetAsLastSibling();
        EditorUtility.SetDirty(view);
    }

    private static void ConfigureFeedbackToast()
    {
        UIFeedbackToast view = Object.FindFirstObjectByType<UIFeedbackToast>(FindObjectsInactive.Include);
        if (view == null)
        {
            GameObject canvasObject = CreateUiObject("UI_FeedbackToastCanvas", null, typeof(Canvas), typeof(CanvasScaler));
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.overrideSorting = true;
            canvas.sortingOrder = 1000;
            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            GameObject root = CreateUiObject("UI_FeedbackToast", canvasObject.transform, typeof(CanvasGroup), typeof(Image), typeof(UIFeedbackToast));
            RectTransform rootRect = root.GetComponent<RectTransform>();
            rootRect.anchorMin = new Vector2(0f, 0.65f);
            rootRect.anchorMax = new Vector2(1f, 0.65f);
            rootRect.pivot = new Vector2(0.5f, 0.5f);
            rootRect.sizeDelta = new Vector2(0f, 80f);
            Image background = root.GetComponent<Image>();
            background.color = new Color(0.08f, 0.08f, 0.1f, 0.92f);
            background.raycastTarget = false;

            GameObject messageObject = CreateUiObject("Message", root.transform, typeof(TextMeshProUGUI));
            RectTransform messageRect = messageObject.GetComponent<RectTransform>();
            Stretch(messageRect, 24f, 8f, -24f, -8f);
            TextMeshProUGUI message = messageObject.GetComponent<TextMeshProUGUI>();
            message.alignment = TextAlignmentOptions.Center;
            message.color = Color.white;
            message.fontSize = 28f;
            message.textWrappingMode = TextWrappingModes.NoWrap;
            message.raycastTarget = false;

            view = root.GetComponent<UIFeedbackToast>();
            SerializedObject serialized = new SerializedObject(view);
            serialized.FindProperty("canvasGroup").objectReferenceValue = root.GetComponent<CanvasGroup>();
            serialized.FindProperty("messageText").objectReferenceValue = message;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        view.gameObject.SetActive(true);
        EditorUtility.SetDirty(view);
    }

    private static GameObject CreateUiObject(string name, Transform parent, params System.Type[] components)
    {
        GameObject result = new GameObject(name, typeof(RectTransform));
        if (parent != null)
            result.transform.SetParent(parent, false);
        foreach (System.Type component in components)
            result.AddComponent(component);
        return result;
    }

    private static void Stretch(RectTransform rect, float left = 0f, float bottom = 0f, float right = 0f, float top = 0f)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(left, bottom);
        rect.offsetMax = new Vector2(right, top);
    }
}
