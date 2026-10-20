using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class LoadingSceneSetup
{
    private const string ScenePath = "Assets/Scenes/LoadingScene.unity";

    [MenuItem("Tools/Scene Flow/Create or Repair Loading Scene")]
    public static void CreateOrRepair()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        GameObject canvasObject = new("LoadingCanvas", typeof(RectTransform), typeof(Canvas),
            typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(LoadingScreenView));
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080f, 1920f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        Image background = CreateImage("Background", canvasObject.transform, new Color32(7, 12, 24, 255));
        Stretch(background.rectTransform);

        Image glow = CreateImage("CenterGlow", canvasObject.transform, new Color32(18, 51, 82, 110));
        SetRect(glow.rectTransform, new Vector2(0.5f, 0.58f), new Vector2(850f, 850f), Vector2.zero);
        glow.rectTransform.localRotation = Quaternion.Euler(0f, 0f, 45f);

        TMP_Text title = CreateText("Title", canvasObject.transform, "DEFENDERS", 74f, FontStyles.Bold);
        title.color = new Color32(111, 220, 255, 255);
        title.characterSpacing = 8f;
        SetRect(title.rectTransform, new Vector2(0.5f, 0.76f), new Vector2(900f, 110f), Vector2.zero);

        RectTransform spinner = CreateRect("Spinner", canvasObject.transform);
        SetRect(spinner, new Vector2(0.5f, 0.58f), new Vector2(220f, 220f), Vector2.zero);
        for (int index = 0; index < 8; index++)
        {
            float angle = index * 45f * Mathf.Deg2Rad;
            byte alpha = (byte)(80 + index * 22);
            Image dot = CreateImage($"Dot{index + 1}", spinner, new Color32(102, 220, 255, alpha));
            RectTransform rect = dot.rectTransform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(26f, 26f);
            rect.anchoredPosition = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * 86f;
            rect.localRotation = Quaternion.Euler(0f, 0f, -index * 45f);
        }

        TMP_Text status = CreateText("Status", canvasObject.transform, "LOADING", 34f, FontStyles.Bold);
        status.color = new Color32(220, 242, 255, 255);
        status.characterSpacing = 5f;
        SetRect(status.rectTransform, new Vector2(0.5f, 0.45f), new Vector2(600f, 70f), Vector2.zero);

        Image tipPanel = CreateImage("TipPanel", canvasObject.transform, new Color32(14, 25, 43, 235));
        SetRect(tipPanel.rectTransform, new Vector2(0.5f, 0.16f), new Vector2(900f, 250f), Vector2.zero);

        TMP_Text tipLabel = CreateText("TipLabel", tipPanel.transform, "TIP", 28f, FontStyles.Bold);
        tipLabel.color = new Color32(91, 217, 255, 255);
        SetRect(tipLabel.rectTransform, new Vector2(0.5f, 0.76f), new Vector2(780f, 48f), Vector2.zero);

        TMP_Text tipText = CreateText("TipText", tipPanel.transform, string.Empty, 31f, FontStyles.Normal);
        tipText.color = new Color32(226, 235, 244, 255);
        tipText.enableWordWrapping = true;
        SetRect(tipText.rectTransform, new Vector2(0.5f, 0.4f), new Vector2(780f, 125f), Vector2.zero);

        SerializedObject view = new(canvasObject.GetComponent<LoadingScreenView>());
        view.FindProperty("spinner").objectReferenceValue = spinner;
        view.FindProperty("tipText").objectReferenceValue = tipText;
        view.FindProperty("statusText").objectReferenceValue = status;
        view.ApplyModifiedPropertiesWithoutUndo();

        EditorSceneManager.SaveScene(scene, ScenePath);
        AddToBuildSettings();
        AssetDatabase.SaveAssets();
        Debug.Log($"[{nameof(LoadingSceneSetup)}] Loading scene is ready: {ScenePath}");
    }

    private static void AddToBuildSettings()
    {
        var scenes = EditorBuildSettings.scenes.ToList();
        if (scenes.Any(entry => entry.path == ScenePath))
            return;

        int startIndex = scenes.FindIndex(entry => entry.path == "Assets/Scenes/StartScene.unity");
        scenes.Insert(startIndex >= 0 ? startIndex + 1 : 0, new EditorBuildSettingsScene(ScenePath, true));
        EditorBuildSettings.scenes = scenes.ToArray();
    }

    private static Image CreateImage(string name, Transform parent, Color color)
    {
        GameObject gameObject = new(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        gameObject.transform.SetParent(parent, false);
        Image image = gameObject.GetComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        return image;
    }

    private static TMP_Text CreateText(string name, Transform parent, string value, float size, FontStyles style)
    {
        GameObject gameObject = new(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        gameObject.transform.SetParent(parent, false);
        TextMeshProUGUI text = gameObject.GetComponent<TextMeshProUGUI>();
        text.font = TMP_Settings.defaultFontAsset;
        text.text = value;
        text.fontSize = size;
        text.fontStyle = style;
        text.alignment = TextAlignmentOptions.Center;
        text.raycastTarget = false;
        return text;
    }

    private static RectTransform CreateRect(string name, Transform parent)
    {
        GameObject gameObject = new(name, typeof(RectTransform));
        gameObject.transform.SetParent(parent, false);
        return gameObject.GetComponent<RectTransform>();
    }

    private static void SetRect(RectTransform rect, Vector2 anchor, Vector2 size, Vector2 position)
    {
        rect.anchorMin = rect.anchorMax = anchor;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = position;
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}
