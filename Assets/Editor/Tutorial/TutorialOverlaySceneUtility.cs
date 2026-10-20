using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class TutorialOverlaySceneUtility
{
    public static TutorialOverlayView GetOrCreate()
    {
        TutorialOverlayView existing = Object.FindFirstObjectByType<TutorialOverlayView>(FindObjectsInactive.Include);
        if (existing != null)
        {
            ConfigureExisting(existing);
            return existing;
        }

        GameObject canvasObject = CreateUIObject("Tutorial Overlay Canvas", null);
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;
        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080f, 1920f);
        scaler.matchWidthOrHeight = 0.5f;
        canvasObject.AddComponent<GraphicRaycaster>();
        CanvasGroup group = canvasObject.AddComponent<CanvasGroup>();
        TutorialOverlayView view = canvasObject.AddComponent<TutorialOverlayView>();

        GameObject blockerObject = CreateUIObject("Input Blocker", canvasObject.transform);
        Stretch(blockerObject.GetComponent<RectTransform>());
        Image blocker = blockerObject.AddComponent<Image>();
        blocker.color = Color.clear;
        blocker.raycastTarget = false;
        TutorialRaycastFilter filter = blockerObject.AddComponent<TutorialRaycastFilter>();

        RectTransform shadeTop = CreateShade("Spotlight Top", canvasObject.transform);
        RectTransform shadeBottom = CreateShade("Spotlight Bottom", canvasObject.transform);
        RectTransform shadeLeft = CreateShade("Spotlight Left", canvasObject.transform);
        RectTransform shadeRight = CreateShade("Spotlight Right", canvasObject.transform);

        GameObject focusObject = CreateUIObject("Focus Frame", canvasObject.transform);
        Image focusImage = focusObject.AddComponent<Image>();
        focusImage.color = new Color(1f, 0.78f, 0.12f, 0.18f);
        focusImage.raycastTarget = false;
        Outline outline = focusObject.AddComponent<Outline>();
        outline.effectColor = new Color(1f, 0.75f, 0.05f, 1f);
        outline.effectDistance = new Vector2(4f, -4f);
        focusObject.SetActive(false);

        GameObject panelObject = CreateUIObject("Guide Panel", canvasObject.transform);
        RectTransform panelRect = panelObject.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.05f, 0.72f);
        panelRect.anchorMax = new Vector2(0.95f, 0.94f);
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        Image panelImage = panelObject.AddComponent<Image>();
        panelImage.color = new Color(0.035f, 0.12f, 0.22f, 0.97f);
        Outline panelOutline = panelObject.AddComponent<Outline>();
        panelOutline.effectColor = Color.white;
        panelOutline.effectDistance = new Vector2(3f, -3f);

        GameObject messageObject = CreateUIObject("Message", panelObject.transform);
        RectTransform messageRect = messageObject.GetComponent<RectTransform>();
        messageRect.anchorMin = new Vector2(0.06f, 0.25f);
        messageRect.anchorMax = new Vector2(0.78f, 0.82f);
        messageRect.offsetMin = Vector2.zero;
        messageRect.offsetMax = Vector2.zero;
        TextMeshProUGUI message = messageObject.AddComponent<TextMeshProUGUI>();
        message.fontSize = 34f;
        message.color = Color.white;
        message.alignment = TextAlignmentOptions.MidlineLeft;
        message.textWrappingMode = TextWrappingModes.Normal;
        message.raycastTarget = false;

        RectTransform arrow = CreateArrow(canvasObject.transform);
        arrow.gameObject.SetActive(false);
        CreateGuideBadge(panelObject.transform);

        SerializedObject serialized = new(view);
        serialized.FindProperty("canvasGroup").objectReferenceValue = group;
        serialized.FindProperty("blocker").objectReferenceValue = blocker;
        serialized.FindProperty("raycastFilter").objectReferenceValue = filter;
        serialized.FindProperty("focusFrame").objectReferenceValue = focusObject.GetComponent<RectTransform>();
        serialized.FindProperty("shadeTop").objectReferenceValue = shadeTop;
        serialized.FindProperty("shadeBottom").objectReferenceValue = shadeBottom;
        serialized.FindProperty("shadeLeft").objectReferenceValue = shadeLeft;
        serialized.FindProperty("shadeRight").objectReferenceValue = shadeRight;
        serialized.FindProperty("guidePanel").objectReferenceValue = panelRect;
        serialized.FindProperty("arrow").objectReferenceValue = arrow;
        serialized.FindProperty("messageText").objectReferenceValue = message;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        return view;
    }

    private static void ConfigureExisting(TutorialOverlayView view)
    {
        Transform root = view.transform;
        Transform blockerTransform = root.Find("Input Blocker");
        Transform focusTransform = root.Find("Focus Frame");
        Transform panelTransform = root.Find("Guide Panel");
        if (blockerTransform == null || focusTransform == null || panelTransform == null)
            throw new System.InvalidOperationException("Tutorial overlay hierarchy is incomplete.");

        UnityEngine.UI.Image blocker = blockerTransform.GetComponent<UnityEngine.UI.Image>();
        blocker.color = Color.clear;
        blocker.raycastTarget = false;
        focusTransform.gameObject.SetActive(false);
        UnityEngine.UI.Image panelImage = panelTransform.GetComponent<UnityEngine.UI.Image>();
        panelImage.color = new Color(0.035f, 0.12f, 0.22f, 0.97f);
        UnityEngine.UI.Outline outline = panelTransform.GetComponent<UnityEngine.UI.Outline>();
        if (outline == null)
            outline = panelTransform.gameObject.AddComponent<UnityEngine.UI.Outline>();
        outline.effectColor = Color.white;
        outline.effectDistance = new Vector2(3f, -3f);

        Transform arrowTransform = root.Find("Action Arrow");
        RectTransform arrow = arrowTransform != null
            ? arrowTransform as RectTransform
            : CreateArrow(root);
        arrow.gameObject.SetActive(false);
        RectTransform shadeTop = GetOrCreateShade(root, "Spotlight Top");
        RectTransform shadeBottom = GetOrCreateShade(root, "Spotlight Bottom");
        RectTransform shadeLeft = GetOrCreateShade(root, "Spotlight Left");
        RectTransform shadeRight = GetOrCreateShade(root, "Spotlight Right");
        MakeShadeTransparent(shadeTop);
        MakeShadeTransparent(shadeBottom);
        MakeShadeTransparent(shadeLeft);
        MakeShadeTransparent(shadeRight);
        int shadeIndex = blockerTransform.GetSiblingIndex() + 1;
        shadeTop.SetSiblingIndex(shadeIndex++);
        shadeBottom.SetSiblingIndex(shadeIndex++);
        shadeLeft.SetSiblingIndex(shadeIndex++);
        shadeRight.SetSiblingIndex(shadeIndex);
        if (panelTransform.Find("Guide Badge") == null)
            CreateGuideBadge(panelTransform);

        RemoveLegacyButton(panelTransform, "Continue Button");
        RemoveLegacyButton(panelTransform, "Skip Button");

        SerializedObject serialized = new(view);
        serialized.FindProperty("canvasGroup").objectReferenceValue = view.GetComponent<CanvasGroup>();
        serialized.FindProperty("blocker").objectReferenceValue = blocker;
        serialized.FindProperty("raycastFilter").objectReferenceValue = blockerTransform.GetComponent<TutorialRaycastFilter>();
        serialized.FindProperty("focusFrame").objectReferenceValue = focusTransform as RectTransform;
        serialized.FindProperty("shadeTop").objectReferenceValue = shadeTop;
        serialized.FindProperty("shadeBottom").objectReferenceValue = shadeBottom;
        serialized.FindProperty("shadeLeft").objectReferenceValue = shadeLeft;
        serialized.FindProperty("shadeRight").objectReferenceValue = shadeRight;
        serialized.FindProperty("guidePanel").objectReferenceValue = panelTransform as RectTransform;
        serialized.FindProperty("arrow").objectReferenceValue = arrow;
        serialized.FindProperty("messageText").objectReferenceValue = panelTransform.Find("Message")?.GetComponent<TextMeshProUGUI>();
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    private static RectTransform CreateArrow(Transform parent)
    {
        GameObject arrowObject = CreateUIObject("Action Arrow", parent);
        RectTransform rect = arrowObject.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(100f, 100f);
        TextMeshProUGUI text = arrowObject.AddComponent<TextMeshProUGUI>();
        text.text = "▼";
        text.fontSize = 72f;
        text.fontStyle = FontStyles.Bold;
        text.color = new Color(1f, 0.82f, 0.08f, 1f);
        text.alignment = TextAlignmentOptions.Center;
        text.raycastTarget = false;
        return rect;
    }

    private static RectTransform GetOrCreateShade(Transform parent, string name)
    {
        Transform existing = parent.Find(name);
        return existing != null ? existing as RectTransform : CreateShade(name, parent);
    }

    private static RectTransform CreateShade(string name, Transform parent)
    {
        GameObject shadeObject = CreateUIObject(name, parent);
        RectTransform rect = shadeObject.GetComponent<RectTransform>();
        UnityEngine.UI.Image image = shadeObject.AddComponent<UnityEngine.UI.Image>();
        image.color = Color.clear;
        image.raycastTarget = false;
        shadeObject.SetActive(false);
        return rect;
    }

    private static void MakeShadeTransparent(RectTransform shade)
    {
        if (shade == null)
            return;

        UnityEngine.UI.Image image = shade.GetComponent<UnityEngine.UI.Image>();
        if (image != null)
        {
            image.color = Color.clear;
            image.raycastTarget = false;
        }
        shade.gameObject.SetActive(false);
    }

    private static void CreateGuideBadge(Transform parent)
    {
        GameObject badgeObject = CreateUIObject("Guide Badge", parent);
        RectTransform rect = badgeObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.82f, 0.78f);
        rect.anchorMax = new Vector2(0.94f, 1.16f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        UnityEngine.UI.Image image = badgeObject.AddComponent<UnityEngine.UI.Image>();
        image.color = new Color(0.08f, 0.56f, 0.95f, 1f);
        image.raycastTarget = false;

        GameObject labelObject = CreateUIObject("Label", badgeObject.transform);
        Stretch(labelObject.GetComponent<RectTransform>());
        TextMeshProUGUI label = labelObject.AddComponent<TextMeshProUGUI>();
        label.text = "!";
        label.fontSize = 54f;
        label.fontStyle = FontStyles.Bold;
        label.color = Color.white;
        label.alignment = TextAlignmentOptions.Center;
        label.raycastTarget = false;
    }

    private static void RemoveLegacyButton(Transform parent, string name)
    {
        Transform child = parent.Find(name);
        if (child != null)
            Object.DestroyImmediate(child.gameObject);
    }

    private static GameObject CreateUIObject(string name, Transform parent)
    {
        GameObject instance = new(name, typeof(RectTransform));
        instance.transform.SetParent(parent, false);
        return instance;
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}
