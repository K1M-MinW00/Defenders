#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class StageUnitInfoSceneBuilder
{
    private const string ScenePath = "Assets/Scenes/GameScene.unity";

    [MenuItem("Tools/Defenders/Build Stage Unit Info UI")]
    public static void Build()
    {
        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        GameObject stageRoot = GameObject.Find("Stage UI Root");
        if (stageRoot == null)
            throw new System.InvalidOperationException("Stage UI Root was not found.");

        Transform existing = stageRoot.transform.Find("UI_StageUnitInfoPanel");
        GameObject root = existing != null
            ? existing.gameObject
            : CreateUi("UI_StageUnitInfoPanel", stageRoot.transform);

        root.transform.SetParent(stageRoot.transform, false);
        while (root.transform.childCount > 0)
            Object.DestroyImmediate(root.transform.GetChild(0).gameObject);

        RectTransform rootRect = root.GetComponent<RectTransform>();
        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.pivot = new Vector2(0.5f, 0.5f);
        rootRect.anchoredPosition = Vector2.zero;
        rootRect.sizeDelta = Vector2.zero;

        RemoveIfPresent<Image>(root);
        RemoveIfPresent<VerticalLayoutGroup>(root);
        Canvas canvas = GetOrAdd<Canvas>(root);
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting = true;
        canvas.sortingOrder = 16;
        CanvasScaler scaler = GetOrAdd<CanvasScaler>(root);
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080f, 1920f);
        scaler.matchWidthOrHeight = 0.5f;
        GetOrAdd<GraphicRaycaster>(root);
        CanvasGroup canvasGroup = GetOrAdd<CanvasGroup>(root);
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        GameObject panelObject = CreateUi("Panel", root.transform);
        RectTransform panelRect = panelObject.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0f, 1f);
        panelRect.anchorMax = new Vector2(0.96f, 1f);
        panelRect.pivot = new Vector2(0f, 1f);
        panelRect.anchoredPosition = new Vector2(12f, -18f);
        panelRect.sizeDelta = new Vector2(-12f, 360f);

        Image background = panelObject.AddComponent<Image>();
        background.color = new Color(0.045f, 0.15f, 0.31f, 0.97f);
        background.raycastTarget = false;
        VerticalLayoutGroup rootLayout = panelObject.AddComponent<VerticalLayoutGroup>();
        rootLayout.padding = new RectOffset(18, 18, 14, 16);
        rootLayout.spacing = 10f;
        rootLayout.childAlignment = TextAnchor.UpperCenter;
        rootLayout.childControlWidth = true;
        rootLayout.childControlHeight = false;
        rootLayout.childForceExpandWidth = true;
        rootLayout.childForceExpandHeight = false;

        GameObject header = CreateUi("Header", panelObject.transform, typeof(LayoutElement), typeof(HorizontalLayoutGroup));
        RectTransform headerRect = header.GetComponent<RectTransform>();
        headerRect.sizeDelta = new Vector2(0f, 56f);
        LayoutElement headerElement = header.GetComponent<LayoutElement>();
        headerElement.minHeight = 56f;
        headerElement.preferredHeight = 56f;
        headerElement.flexibleHeight = 0f;
        HorizontalLayoutGroup headerLayout = header.GetComponent<HorizontalLayoutGroup>();
        headerLayout.spacing = 14f;
        headerLayout.childAlignment = TextAnchor.MiddleLeft;
        headerLayout.childControlWidth = true;
        headerLayout.childControlHeight = true;
        headerLayout.childForceExpandWidth = false;
        headerLayout.childForceExpandHeight = true;

        GameObject starObject = CreateUi("StarIcon", header.transform, typeof(Image), typeof(LayoutElement));
        SetWidth(starObject, 56f, 0f);
        Image starImage = starObject.GetComponent<Image>();
        starImage.preserveAspect = true;
        starImage.raycastTarget = false;
        UnitStarIconView starIcon = starObject.AddComponent<UnitStarIconView>();
        ConfigureStarIcon(starIcon, starImage);

        TMP_Text unitName = CreateText("UnitName", header.transform, 31f, 46f, FontStyles.Bold);
        SetWidth(unitName.gameObject, 430f, 1f);
        unitName.alignment = TextAlignmentOptions.MidlineLeft;

        GameObject content = CreateUi("Content", panelObject.transform, typeof(LayoutElement), typeof(HorizontalLayoutGroup));
        RectTransform contentRect = content.GetComponent<RectTransform>();
        contentRect.sizeDelta = new Vector2(0f, 264f);
        LayoutElement contentElement = content.GetComponent<LayoutElement>();
        contentElement.minHeight = 264f;
        contentElement.preferredHeight = 264f;
        contentElement.flexibleHeight = 0f;
        HorizontalLayoutGroup contentLayout = content.GetComponent<HorizontalLayoutGroup>();
        contentLayout.spacing = 12f;
        contentLayout.childAlignment = TextAnchor.MiddleCenter;
        contentLayout.childControlWidth = true;
        contentLayout.childControlHeight = true;
        contentLayout.childForceExpandWidth = false;
        contentLayout.childForceExpandHeight = true;

        GameObject portraitRoot = CreateUi("Portrait", content.transform, typeof(Image), typeof(LayoutElement));
        SetWidth(portraitRoot, 190f, 0f);
        Image portrait = portraitRoot.GetComponent<Image>();
        portrait.preserveAspect = true;
        portrait.raycastTarget = false;
        SkillRefs active = CreateSkillCard("Active", "액티브", content.transform);
        SkillRefs passive = CreateSkillCard("Passive", "패시브", content.transform);

        StageUnitInfoPanel panel = GetOrAdd<StageUnitInfoPanel>(root);
        SerializedObject serializedPanel = new(panel);
        SetReference(serializedPanel, "panelRect", panelRect);
        SetReference(serializedPanel, "portraitImage", portrait);
        SetReference(serializedPanel, "unitNameText", unitName);
        SetReference(serializedPanel, "starIcon", starIcon);
        SetCard(serializedPanel.FindProperty("activeCard"), active);
        SetCard(serializedPanel.FindProperty("passiveCard"), passive);
        serializedPanel.ApplyModifiedPropertiesWithoutUndo();

        StageUIController controller = stageRoot.GetComponent<StageUIController>();
        if (controller == null)
            throw new System.InvalidOperationException("StageUIController was not found.");
        SerializedObject serializedController = new(controller);
        serializedController.FindProperty("unitInfoPanel").objectReferenceValue = panel;
        serializedController.ApplyModifiedPropertiesWithoutUndo();

        root.SetActive(false);
        EditorUtility.SetDirty(root);
        EditorUtility.SetDirty(controller);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[StageUnitInfoSceneBuilder] Unit info UI created and linked.");
    }

    private static SkillRefs CreateSkillCard(string objectName, string title, Transform parent)
    {
        GameObject card = CreateUi(objectName, parent, typeof(Image), typeof(LayoutElement), typeof(VerticalLayoutGroup));
        SetWidth(card, 260f, 1f);
        Image background = card.GetComponent<Image>();
        background.color = new Color(0.1f, 0.14f, 0.2f, 0.96f);
        background.raycastTarget = false;
        VerticalLayoutGroup layout = card.GetComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(14, 14, 10, 10);
        layout.spacing = 5f;
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = true;

        TMP_Text header = CreateText("Header", card.transform, 23f, 31f, FontStyles.Bold);
        header.text = title;
        TMP_Text lockText = CreateText("Lock", card.transform, 20f, 28f, FontStyles.Bold);
        lockText.color = new Color(1f, 0.76f, 0.2f);
        GameObject iconObject = CreateUi("Icon", card.transform, typeof(Image), typeof(LayoutElement));
        iconObject.GetComponent<LayoutElement>().preferredHeight = 58f;
        Image icon = iconObject.GetComponent<Image>();
        icon.preserveAspect = true;
        icon.raycastTarget = false;
        TMP_Text skillName = CreateText("SkillName", card.transform, 21f, 29f, FontStyles.Bold);
        TMP_Text description = CreateText("Description", card.transform, 17f, 105f, FontStyles.Normal);
        description.alignment = TextAlignmentOptions.TopLeft;
        return new SkillRefs(background, icon, skillName, description, lockText);
    }

    private static TMP_Text CreateText(string name, Transform parent, float size, float height, FontStyles style)
    {
        GameObject result = CreateUi(name, parent, typeof(TextMeshProUGUI), typeof(LayoutElement));
        result.GetComponent<LayoutElement>().preferredHeight = height;
        TMP_Text text = result.GetComponent<TextMeshProUGUI>();
        text.fontSize = size;
        text.fontStyle = style;
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.Center;
        text.textWrappingMode = TextWrappingModes.Normal;
        text.overflowMode = TextOverflowModes.Ellipsis;
        text.raycastTarget = false;
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

    private static void SetWidth(GameObject target, float width, float flexible)
    {
        LayoutElement element = GetOrAdd<LayoutElement>(target);
        element.preferredWidth = width;
        element.flexibleWidth = flexible;
    }

    private static void SetCard(SerializedProperty property, SkillRefs value)
    {
        property.FindPropertyRelative("background").objectReferenceValue = value.Background;
        property.FindPropertyRelative("icon").objectReferenceValue = value.Icon;
        property.FindPropertyRelative("name").objectReferenceValue = value.Name;
        property.FindPropertyRelative("description").objectReferenceValue = value.Description;
        property.FindPropertyRelative("lockText").objectReferenceValue = value.LockText;
    }

    private static void SetReference(SerializedObject target, string propertyName, Object value)
    {
        target.FindProperty(propertyName).objectReferenceValue = value;
    }

    private static void ConfigureStarIcon(UnitStarIconView view, Image image)
    {
        SerializedObject serialized = new(view);
        serialized.FindProperty("targetImage").objectReferenceValue = image;
        SerializedProperty sprites = serialized.FindProperty("starSprites");
        sprites.arraySize = 4;
        for (int i = 0; i < 4; i++)
            sprites.GetArrayElementAtIndex(i).objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Art/UI/Generated/rank_{i + 1}.png");
        serialized.ApplyModifiedPropertiesWithoutUndo();
        view.SetStar(1);
    }

    private static T GetOrAdd<T>(GameObject target) where T : Component
    {
        T component = target.GetComponent<T>();
        return component != null ? component : target.AddComponent<T>();
    }

    private static void RemoveIfPresent<T>(GameObject target) where T : Component
    {
        T component = target.GetComponent<T>();
        if (component != null)
            Object.DestroyImmediate(component);
    }

    private readonly struct SkillRefs
    {
        public SkillRefs(Image background, Image icon, TMP_Text name, TMP_Text description, TMP_Text lockText)
        {
            Background = background;
            Icon = icon;
            Name = name;
            Description = description;
            LockText = lockText;
        }

        public Image Background { get; }
        public Image Icon { get; }
        public TMP_Text Name { get; }
        public TMP_Text Description { get; }
        public TMP_Text LockText { get; }
    }
}
#endif
