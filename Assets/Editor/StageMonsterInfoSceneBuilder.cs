using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class StageMonsterInfoSceneBuilder
{
    private const string ScenePath = "Assets/Scenes/GameScene.unity";
    private const string RootName = "Monster Info Popup";

    [MenuItem("Tools/Defenders/Stage/Build Monster Info Popup")]
    public static void Build()
    {
        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        Transform popupCanvas = RequireTransform(scene, "Stage Popup Canvas");
        Transform topHud = RequireTransform(scene, "Top HUD");
        Button openButton = topHud.Find("Button")?.GetComponent<Button>();
        StageUIController stageUI = Object.FindFirstObjectByType<StageUIController>(FindObjectsInactive.Include);
        TMP_Text fontSource = Object.FindFirstObjectByType<TMP_Text>(FindObjectsInactive.Include);

        if (openButton == null || stageUI == null || fontSource == null)
            throw new MissingReferenceException("Top HUD/Button, StageUIController 또는 TMP 폰트를 찾을 수 없습니다.");

        Transform previous = popupCanvas.Find(RootName);
        if (previous != null)
            Object.DestroyImmediate(previous.gameObject);

        GameObject root = CreateUI(RootName, popupCanvas);
        Stretch(root.GetComponent<RectTransform>());

        GameObject blockerObject = CreateUI("Close Blocker", root.transform, typeof(Image), typeof(Button));
        Stretch(blockerObject.GetComponent<RectTransform>());
        Image dim = blockerObject.GetComponent<Image>();
        dim.color = new Color32(7, 15, 28, 210);

        Button blocker = blockerObject.GetComponent<Button>();
        blocker.targetGraphic = dim;
        blocker.transition = Selectable.Transition.None;

        GameObject panel = CreateUI("Panel", root.transform, typeof(Image));
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = new Vector2(0f, 80f);
        panelRect.sizeDelta = new Vector2(820f, 1000f);
        panel.GetComponent<Image>().color = new Color32(91, 119, 166, 255);
        Outline panelOutline = panel.AddComponent<Outline>();
        panelOutline.effectColor = new Color32(8, 18, 34, 255);
        panelOutline.effectDistance = new Vector2(5f, -7f);

        GameObject header = CreateUI("Header", panel.transform, typeof(Image));
        RectTransform headerRect = header.GetComponent<RectTransform>();
        headerRect.anchorMin = new Vector2(0f, 1f);
        headerRect.anchorMax = Vector2.one;
        headerRect.pivot = new Vector2(0.5f, 1f);
        headerRect.anchoredPosition = Vector2.zero;
        headerRect.sizeDelta = new Vector2(0f, 130f);
        header.GetComponent<Image>().color = new Color32(65, 115, 211, 255);
        TMP_Text title = CreateText("Title", header.transform, fontSource, "몬스터 정보", 48f, FontStyles.Bold);
        Stretch(title.rectTransform, 20f, 20f, 10f, 10f);
        title.alignment = TextAlignmentOptions.Center;

        GameObject scrollView = CreateUI("Scroll View", panel.transform, typeof(ScrollRect));
        RectTransform scrollRectTransform = scrollView.GetComponent<RectTransform>();
        scrollRectTransform.anchorMin = Vector2.zero;
        scrollRectTransform.anchorMax = Vector2.one;
        scrollRectTransform.offsetMin = new Vector2(35f, 45f);
        scrollRectTransform.offsetMax = new Vector2(-35f, -165f);

        GameObject viewport = CreateUI("Viewport", scrollView.transform, typeof(Image), typeof(Mask));
        Stretch(viewport.GetComponent<RectTransform>());
        viewport.GetComponent<Image>().color = new Color32(64, 83, 116, 255);
        viewport.GetComponent<Mask>().showMaskGraphic = true;

        GameObject content = CreateUI("Content", viewport.transform,
            typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        RectTransform contentRect = content.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0f, 1f);
        contentRect.anchorMax = Vector2.one;
        contentRect.pivot = new Vector2(0.5f, 1f);
        contentRect.anchoredPosition = Vector2.zero;
        contentRect.sizeDelta = Vector2.zero;

        VerticalLayoutGroup layout = content.GetComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(25, 25, 25, 25);
        layout.spacing = 22f;
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        content.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        ScrollRect scrollRect = scrollView.GetComponent<ScrollRect>();
        scrollRect.viewport = viewport.GetComponent<RectTransform>();
        scrollRect.content = contentRect;
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;
        scrollRect.scrollSensitivity = 35f;

        StageMonsterInfoItemUI template = BuildItemTemplate(content.transform, fontSource);
        TMP_Text emptyText = CreateText("Empty Text", content.transform, fontSource,
            "이 웨이브에는 표시할 몬스터가 없습니다.", 31f, FontStyles.Normal);
        emptyText.alignment = TextAlignmentOptions.Center;
        LayoutElement emptyLayout = emptyText.gameObject.AddComponent<LayoutElement>();
        emptyLayout.preferredHeight = 150f;
        emptyText.gameObject.SetActive(false);

        TMP_Text instruction = CreateText("Close Instruction", root.transform, fontSource,
            "바깥 영역을 탭하여 닫으세요.", 34f, FontStyles.Bold);
        RectTransform instructionRect = instruction.rectTransform;
        instructionRect.anchorMin = instructionRect.anchorMax = new Vector2(0.5f, 0.5f);
        instructionRect.pivot = new Vector2(0.5f, 0.5f);
        instructionRect.anchoredPosition = new Vector2(0f, -510f);
        instructionRect.sizeDelta = new Vector2(760f, 80f);
        instruction.alignment = TextAlignmentOptions.Center;

        StageMonsterInfoUI infoUI = root.AddComponent<StageMonsterInfoUI>();
        SerializedObject serialized = new(infoUI);
        Set(serialized, "openButton", openButton);
        Set(serialized, "closeBlockerButton", blocker);
        Set(serialized, "panelRoot", root);
        Set(serialized, "scrollRect", scrollRect);
        Set(serialized, "contentRoot", contentRect);
        Set(serialized, "itemTemplate", template);
        Set(serialized, "emptyText", emptyText);
        serialized.ApplyModifiedPropertiesWithoutUndo();

        SerializedObject controllerSerialized = new(stageUI);
        Set(controllerSerialized, "monsterInfoUI", infoUI);
        controllerSerialized.ApplyModifiedPropertiesWithoutUndo();

        root.SetActive(false);
        EditorUtility.SetDirty(stageUI);
        EditorUtility.SetDirty(infoUI);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        Debug.Log("[StageMonsterInfoSceneBuilder] 몬스터 정보 팝업과 Top HUD 버튼을 연결했습니다.");
    }

    private static StageMonsterInfoItemUI BuildItemTemplate(Transform parent, TMP_Text fontSource)
    {
        GameObject item = CreateUI("Monster Info Item Template", parent, typeof(Image), typeof(LayoutElement));
        item.GetComponent<Image>().color = new Color32(246, 248, 252, 255);
        LayoutElement layout = item.GetComponent<LayoutElement>();
        layout.preferredHeight = 230f;
        layout.minHeight = 230f;
        Outline outline = item.AddComponent<Outline>();
        outline.effectColor = new Color32(9, 17, 30, 255);
        outline.effectDistance = new Vector2(3f, -4f);

        GameObject nameBackgroundObject = CreateUI("Name Background", item.transform, typeof(Image));
        RectTransform nameBackgroundRect = nameBackgroundObject.GetComponent<RectTransform>();
        nameBackgroundRect.anchorMin = new Vector2(0f, 1f);
        nameBackgroundRect.anchorMax = new Vector2(0.68f, 1f);
        nameBackgroundRect.pivot = new Vector2(0.5f, 1f);
        nameBackgroundRect.offsetMin = new Vector2(24f, -70f);
        nameBackgroundRect.offsetMax = new Vector2(0f, -12f);
        Image nameBackground = nameBackgroundObject.GetComponent<Image>();
        nameBackground.color = new Color32(4, 25, 61, 255);
        nameBackground.raycastTarget = false;

        TMP_Text name = CreateText("Name", nameBackgroundObject.transform, fontSource, "몬스터 이름", 34f, FontStyles.Bold);
        Stretch(name.rectTransform, 10f, 10f, 2f, 2f);
        name.color = Color.white;
        name.alignment = TextAlignmentOptions.Center;

        TMP_Text description = CreateText("Description", item.transform, fontSource,
            "몬스터 설명", 26f, FontStyles.Normal);
        RectTransform descriptionRect = description.rectTransform;
        descriptionRect.anchorMin = Vector2.zero;
        descriptionRect.anchorMax = new Vector2(0.72f, 1f);
        descriptionRect.offsetMin = new Vector2(28f, 18f);
        descriptionRect.offsetMax = new Vector2(-8f, -82f);
        description.color = new Color32(17, 23, 31, 255);
        description.alignment = TextAlignmentOptions.TopLeft;
        description.textWrappingMode = TextWrappingModes.Normal;
        description.overflowMode = TextOverflowModes.Ellipsis;

        GameObject portraitObject = CreateUI("Portrait", item.transform, typeof(Image));
        RectTransform portraitRect = portraitObject.GetComponent<RectTransform>();
        portraitRect.anchorMin = new Vector2(0.73f, 0.08f);
        portraitRect.anchorMax = new Vector2(0.98f, 0.92f);
        portraitRect.offsetMin = portraitRect.offsetMax = Vector2.zero;
        Image portrait = portraitObject.GetComponent<Image>();
        portrait.preserveAspect = true;
        portrait.raycastTarget = false;

        StageMonsterInfoItemUI view = item.AddComponent<StageMonsterInfoItemUI>();
        SerializedObject serialized = new(view);
        Set(serialized, "nameText", name);
        Set(serialized, "descriptionText", description);
        Set(serialized, "portraitImage", portrait);
        serialized.ApplyModifiedPropertiesWithoutUndo();
        item.SetActive(false);
        return view;
    }

    private static GameObject CreateUI(string name, Transform parent, params System.Type[] components)
    {
        GameObject gameObject = new(name, typeof(RectTransform));
        gameObject.layer = 5;
        gameObject.transform.SetParent(parent, false);
        foreach (System.Type component in components)
            gameObject.AddComponent(component);
        return gameObject;
    }

    private static TMP_Text CreateText(
        string name,
        Transform parent,
        TMP_Text source,
        string value,
        float fontSize,
        FontStyles style)
    {
        GameObject gameObject = CreateUI(name, parent, typeof(TextMeshProUGUI));
        TMP_Text text = gameObject.GetComponent<TMP_Text>();
        text.font = source.font;
        text.fontSharedMaterial = source.fontSharedMaterial;
        text.text = value;
        text.fontSize = fontSize;
        text.fontStyle = style;
        text.color = Color.white;
        text.raycastTarget = false;
        return text;
    }

    private static void Stretch(RectTransform rect, float left = 0f, float right = 0f, float bottom = 0f, float top = 0f)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(left, bottom);
        rect.offsetMax = new Vector2(-right, -top);
    }

    private static Transform RequireTransform(Scene scene, string objectName)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            foreach (Transform candidate in root.GetComponentsInChildren<Transform>(true))
            {
                if (candidate.name == objectName)
                    return candidate;
            }
        }

        throw new MissingReferenceException($"{objectName} 오브젝트를 찾을 수 없습니다.");
    }

    private static void Set(SerializedObject serialized, string propertyName, Object value)
    {
        SerializedProperty property = serialized.FindProperty(propertyName);
        if (property == null)
            throw new MissingReferenceException($"{serialized.targetObject.name}.{propertyName} 필드가 없습니다.");
        property.objectReferenceValue = value;
    }
}
