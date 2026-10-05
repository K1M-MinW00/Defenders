#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public static class StageRelicSceneBuilder
{
    private const string ScenePath = "Assets/Scenes/GameScene.unity";

    [MenuItem("Tools/Defenders/Migrate Stage Relic Choice Icons")]
    public static void MigrateChoiceIcons()
    {
        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        StageRelicUI relicUI = Object.FindFirstObjectByType<StageRelicUI>(FindObjectsInactive.Include);
        if (relicUI == null)
            throw new System.InvalidOperationException("StageRelicUI was not found.");

        Transform relicRoot = relicUI.transform;
        Image firstSymbol = ConvertChoiceSymbol(
            relicRoot.Find("RelicChoiceOverlay/Panel/Choices/ChoiceCard_1/Icon/Symbol"));
        Image secondSymbol = ConvertChoiceSymbol(
            relicRoot.Find("RelicChoiceOverlay/Panel/Choices/ChoiceCard_2/Icon/Symbol"));

        SerializedObject serializedRelicUI = new(relicUI);
        SetReference(serializedRelicUI, "firstChoiceSymbol", firstSymbol);
        SetReference(serializedRelicUI, "secondChoiceSymbol", secondSymbol);
        serializedRelicUI.ApplyModifiedPropertiesWithoutUndo();

        EditorUtility.SetDirty(relicUI);
        EditorUtility.SetDirty(firstSymbol.gameObject);
        EditorUtility.SetDirty(secondSymbol.gameObject);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[StageRelicSceneBuilder] Choice relic symbols migrated from TMP to Image.");
    }

    [MenuItem("Tools/Defenders/Build Stage Relic UI Root")]
    public static void Build()
    {
        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        GameObject stageRoot = GameObject.Find("Stage UI Root");
        if (stageRoot == null)
            throw new System.InvalidOperationException("Stage UI Root was not found.");

        Transform existing = stageRoot.transform.Find("UI_StageRelics");
        GameObject relicObject = existing != null
            ? existing.gameObject
            : new GameObject("UI_StageRelics", typeof(RectTransform));

        relicObject.transform.SetParent(stageRoot.transform, false);
        RectTransform rect = relicObject.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Canvas canvas = GetOrAdd<Canvas>(relicObject);
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting = true;
        canvas.sortingOrder = 15;

        CanvasScaler scaler = GetOrAdd<CanvasScaler>(relicObject);
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080f, 1920f);
        scaler.matchWidthOrHeight = 0.5f;
        GetOrAdd<GraphicRaycaster>(relicObject);
        StageRelicUI relicUI = GetOrAdd<StageRelicUI>(relicObject);

        while (relicObject.transform.childCount > 0)
            Object.DestroyImmediate(relicObject.transform.GetChild(0).gameObject);

        RectTransform ownedList = CreateOwnedList(relicObject.transform);
        (GameObject tooltip, TMP_Text tooltipName, TMP_Text tooltipDescription) = CreateTooltip(relicObject.transform);
        ChoiceOverlayRefs choices = CreateChoiceOverlay(relicObject.transform);

        SerializedObject serializedRelicUI = new(relicUI);
        SetReference(serializedRelicUI, "ownedList", ownedList);
        SetReference(serializedRelicUI, "tooltip", tooltip);
        SetReference(serializedRelicUI, "tooltipName", tooltipName);
        SetReference(serializedRelicUI, "tooltipDescription", tooltipDescription);
        SetReference(serializedRelicUI, "choiceOverlay", choices.Overlay);
        SetReference(serializedRelicUI, "firstChoiceRoot", choices.First.Root);
        SetReference(serializedRelicUI, "firstChoiceName", choices.First.Name);
        SetReference(serializedRelicUI, "firstChoiceSymbol", choices.First.Symbol);
        SetReference(serializedRelicUI, "firstChoiceDescription", choices.First.Description);
        SetReference(serializedRelicUI, "firstChoiceButton", choices.First.Button);
        SetReference(serializedRelicUI, "secondChoiceRoot", choices.Second.Root);
        SetReference(serializedRelicUI, "secondChoiceName", choices.Second.Name);
        SetReference(serializedRelicUI, "secondChoiceSymbol", choices.Second.Symbol);
        SetReference(serializedRelicUI, "secondChoiceDescription", choices.Second.Description);
        SetReference(serializedRelicUI, "secondChoiceButton", choices.Second.Button);
        serializedRelicUI.ApplyModifiedPropertiesWithoutUndo();

        StageUIController controller = stageRoot.GetComponent<StageUIController>();
        if (controller == null)
            throw new System.InvalidOperationException("StageUIController was not found.");

        SerializedObject serializedController = new(controller);
        serializedController.FindProperty("relicUI").objectReferenceValue = relicUI;
        serializedController.ApplyModifiedPropertiesWithoutUndo();

        StageSessionController session = Object.FindFirstObjectByType<StageSessionController>();
        if (session == null)
            throw new System.InvalidOperationException("StageSessionController was not found.");

        StageRelicService relicService = GetOrAdd<StageRelicService>(session.gameObject);
        SerializedObject serializedSession = new(session);
        serializedSession.FindProperty("relicService").objectReferenceValue = relicService;
        serializedSession.ApplyModifiedPropertiesWithoutUndo();

        EditorUtility.SetDirty(relicObject);
        EditorUtility.SetDirty(controller);
        EditorUtility.SetDirty(session);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[StageRelicSceneBuilder] UI_StageRelics hierarchy created and linked.");
    }

    private static RectTransform CreateOwnedList(Transform parent)
    {
        GameObject result = CreateUi("OwnedRelics", parent);
        RectTransform rect = result.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(1f, 0.35f);
        rect.anchorMax = new Vector2(1f, 0.78f);
        rect.pivot = Vector2.one;
        rect.anchoredPosition = new Vector2(-20f, 0f);
        rect.sizeDelta = new Vector2(72f, 0f);
        VerticalLayoutGroup layout = result.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 8f;
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;
        return rect;
    }

    private static (GameObject, TMP_Text, TMP_Text) CreateTooltip(Transform parent)
    {
        GameObject result = CreateUi("RelicTooltip", parent, typeof(Image), typeof(VerticalLayoutGroup));
        RectTransform rect = result.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.zero;
        rect.pivot = new Vector2(1f, 0.5f);
        rect.sizeDelta = new Vector2(430f, 210f);
        result.GetComponent<Image>().color = new Color(0.025f, 0.07f, 0.09f, 0.97f);
        VerticalLayoutGroup layout = result.GetComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(18, 18, 16, 16);
        layout.spacing = 10f;

        TMP_Text name = CreateText("Name", result.transform, string.Empty, 28f, 44f, FontStyles.Bold);
        TMP_Text description = CreateText("Description", result.transform, string.Empty, 21f, 120f);
        result.SetActive(false);
        return (result, name, description);
    }

    private static ChoiceOverlayRefs CreateChoiceOverlay(Transform parent)
    {
        GameObject overlay = CreateUi("RelicChoiceOverlay", parent, typeof(Image));
        Stretch(overlay.GetComponent<RectTransform>());
        overlay.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.68f);

        GameObject panel = CreateUi("Panel", overlay.transform, typeof(VerticalLayoutGroup));
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.08f, 0.24f);
        panelRect.anchorMax = new Vector2(0.92f, 0.83f);
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        VerticalLayoutGroup panelLayout = panel.GetComponent<VerticalLayoutGroup>();
        panelLayout.spacing = 28f;
        panelLayout.childAlignment = TextAnchor.UpperCenter;
        panelLayout.childControlWidth = true;
        panelLayout.childControlHeight = false;
        panelLayout.childForceExpandWidth = true;

        TMP_Text title = CreateText("Title", panel.transform, "유물을 선택하세요.", 42f, 70f, FontStyles.Bold);
        title.alignment = TextAlignmentOptions.Center;

        GameObject cardRow = CreateUi("Choices", panel.transform, typeof(HorizontalLayoutGroup), typeof(LayoutElement));
        cardRow.GetComponent<LayoutElement>().preferredHeight = 720f;
        HorizontalLayoutGroup rowLayout = cardRow.GetComponent<HorizontalLayoutGroup>();
        rowLayout.spacing = 34f;
        rowLayout.childControlWidth = true;
        rowLayout.childControlHeight = true;
        rowLayout.childForceExpandWidth = true;
        rowLayout.childForceExpandHeight = true;

        ChoiceCardRefs first = CreateChoiceCard("ChoiceCard_1", cardRow.transform);
        ChoiceCardRefs second = CreateChoiceCard("ChoiceCard_2", cardRow.transform);
        overlay.SetActive(false);
        return new ChoiceOverlayRefs(overlay, first, second);
    }

    private static ChoiceCardRefs CreateChoiceCard(string name, Transform parent)
    {
        GameObject root = CreateUi(name, parent, typeof(Image), typeof(VerticalLayoutGroup), typeof(LayoutElement));
        root.GetComponent<Image>().color = new Color(0.02f, 0.12f, 0.15f, 0.97f);
        LayoutElement cardLayout = root.GetComponent<LayoutElement>();
        cardLayout.minWidth = 0f;
        cardLayout.preferredWidth = 0f;
        cardLayout.flexibleWidth = 1f;
        VerticalLayoutGroup layout = root.GetComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(22, 22, 22, 22);
        layout.spacing = 18f;
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = true;

        TMP_Text cardName = CreateText("Name", root.transform, string.Empty, 30f, 54f, FontStyles.Bold);
        cardName.alignment = TextAlignmentOptions.Center;

        GameObject icon = CreateUi("Icon", root.transform, typeof(Image), typeof(LayoutElement));
        icon.GetComponent<Image>().color = new Color(0.02f, 0.42f, 0.45f, 1f);
        icon.GetComponent<LayoutElement>().preferredHeight = 190f;
        GameObject symbolObject = CreateUi("Symbol", icon.transform, typeof(Image), typeof(LayoutElement));
        Image symbol = symbolObject.GetComponent<Image>();
        symbol.color = Color.white;
        symbol.preserveAspect = true;
        symbol.raycastTarget = false;
        symbolObject.GetComponent<LayoutElement>().preferredHeight = 190f;
        Stretch(symbolObject.GetComponent<RectTransform>());

        TMP_Text description = CreateText("Description", root.transform, string.Empty, 25f, 250f);
        description.alignment = TextAlignmentOptions.Center;

        GameObject buttonObject = CreateUi("SelectButton", root.transform, typeof(Image), typeof(Button), typeof(LayoutElement));
        buttonObject.GetComponent<Image>().color = new Color(0.1f, 0.7f, 0.9f, 1f);
        buttonObject.GetComponent<LayoutElement>().preferredHeight = 86f;
        TMP_Text buttonLabel = CreateText("Label", buttonObject.transform, "선택", 28f, 86f, FontStyles.Bold);
        Stretch(buttonLabel.rectTransform);
        buttonLabel.alignment = TextAlignmentOptions.Center;
        return new ChoiceCardRefs(root, cardName, symbol, description, buttonObject.GetComponent<Button>());
    }

    private static TMP_Text CreateText(
        string name,
        Transform parent,
        string value,
        float fontSize,
        float preferredHeight,
        FontStyles style = FontStyles.Normal)
    {
        GameObject result = CreateUi(name, parent, typeof(TextMeshProUGUI), typeof(LayoutElement));
        TMP_Text text = result.GetComponent<TextMeshProUGUI>();
        text.text = value;
        text.fontSize = fontSize;
        text.fontStyle = style;
        text.color = Color.white;
        text.textWrappingMode = TextWrappingModes.Normal;
        text.overflowMode = TextOverflowModes.Overflow;
        text.raycastTarget = false;
        result.GetComponent<LayoutElement>().preferredHeight = preferredHeight;
        return text;
    }

    private static GameObject CreateUi(string name, Transform parent, params System.Type[] components)
    {
        var types = new System.Type[components.Length + 1];
        types[0] = typeof(RectTransform);
        components.CopyTo(types, 1);
        var result = new GameObject(name, types);
        result.layer = LayerMask.NameToLayer("UI");
        result.transform.SetParent(parent, false);
        return result;
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private static void SetReference(SerializedObject target, string propertyName, Object value)
    {
        target.FindProperty(propertyName).objectReferenceValue = value;
    }

    private static Image ConvertChoiceSymbol(Transform symbolTransform)
    {
        if (symbolTransform == null)
            throw new System.InvalidOperationException("Relic choice Symbol object was not found.");

        TMP_Text text = symbolTransform.GetComponent<TMP_Text>();
        if (text != null)
            Object.DestroyImmediate(text, true);

        Image image = GetOrAdd<Image>(symbolTransform.gameObject);
        image.sprite = null;
        image.color = Color.white;
        image.preserveAspect = true;
        image.raycastTarget = false;
        image.enabled = true;
        return image;
    }

    private readonly struct ChoiceCardRefs
    {
        public ChoiceCardRefs(GameObject root, TMP_Text name, Image symbol, TMP_Text description, Button button)
        {
            Root = root;
            Name = name;
            Symbol = symbol;
            Description = description;
            Button = button;
        }

        public GameObject Root { get; }
        public TMP_Text Name { get; }
        public Image Symbol { get; }
        public TMP_Text Description { get; }
        public Button Button { get; }
    }

    private readonly struct ChoiceOverlayRefs
    {
        public ChoiceOverlayRefs(GameObject overlay, ChoiceCardRefs first, ChoiceCardRefs second)
        {
            Overlay = overlay;
            First = first;
            Second = second;
        }

        public GameObject Overlay { get; }
        public ChoiceCardRefs First { get; }
        public ChoiceCardRefs Second { get; }
    }

    private static T GetOrAdd<T>(GameObject target) where T : Component
    {
        T component = target.GetComponent<T>();
        if (component == null)
            component = target.AddComponent<T>();
        return component;
    }
}
#endif
