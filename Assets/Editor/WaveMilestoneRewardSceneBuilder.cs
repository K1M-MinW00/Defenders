#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class WaveMilestoneRewardSceneBuilder
{
    private const string ScenePath = "Assets/Scenes/LobbyScene.unity";
    private const string OverlayPrefabPath = "Assets/Prefabs/UI/WaveMilestoneRewardOverlay.prefab";
    private const string SlotPrefabPath = "Assets/Prefabs/UI/WaveMilestoneRewardSlot.prefab";
    private const string RewardItemPrefabPath = "Assets/Prefabs/UI/Shop/ShopRewardSlot.prefab";
    private const string PanelSpritePath = "Assets/Art/UI/Generated/panel_common_9slice.png";
    private const string BadgeSpritePath = "Assets/Resources/UI/NotificationBadge.png";
    private static readonly Color Navy = new(0.055f, 0.13f, 0.23f, 1f);
    private static readonly Color NavyLight = new(0.10f, 0.24f, 0.38f, 1f);
    private static readonly Color Cyan = new(0.16f, 0.82f, 0.92f, 1f);
    private static readonly Color Red = new(0.94f, 0.20f, 0.36f, 1f);

    [MenuItem("Tools/Defenders/Build Wave Milestone Reward UI")]
    public static void Build()
    {
        GameObject slotPrefab = BuildSlotPrefab();
        GameObject overlayPrefab = BuildOverlayPrefab();
        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        Button openButton = Find(scene, "Button_Reward")?.GetComponent<Button>();
        if (openButton == null) throw new System.InvalidOperationException("Battle_Panel/Panel_Reward/Button_Reward was not found.");
        Canvas canvas = openButton.GetComponentInParent<Canvas>()?.rootCanvas;
        if (canvas == null) throw new System.InvalidOperationException("Lobby root Canvas was not found.");

        RemoveExisting(scene, "WaveMilestoneRewardOverlay");
        Transform panelReward = openButton.transform.parent;
        WaveMilestoneRewardController oldController = panelReward.GetComponent<WaveMilestoneRewardController>();
        if (oldController != null) Object.DestroyImmediate(oldController);
        Transform oldBadge = openButton.transform.Find("RewardAvailableBadge");
        if (oldBadge != null) Object.DestroyImmediate(oldBadge.gameObject);

        GameObject badge = ImageObject(openButton.transform, "RewardAvailableBadge", Color.white, LoadSprite(BadgeSpritePath));
        RectTransform badgeRect = (RectTransform)badge.transform;
        badgeRect.anchorMin = badgeRect.anchorMax = new Vector2(1f, 1f);
        badgeRect.pivot = new Vector2(0.5f, 0.5f);
        badgeRect.anchoredPosition = new Vector2(-4f, -4f);
        badgeRect.sizeDelta = new Vector2(40f, 40f);
        Image badgeImage = badge.GetComponent<Image>();
        badgeImage.type = Image.Type.Sliced;
        badgeImage.raycastTarget = false;
        badge.SetActive(false);

        GameObject overlay = (GameObject)PrefabUtility.InstantiatePrefab(overlayPrefab, canvas.transform);
        overlay.name = "WaveMilestoneRewardOverlay";
        RectTransform cardRect = Find(overlay.transform, "ModalCard") as RectTransform;
        TMP_Text title = Find(overlay.transform, "Label").GetComponent<TMP_Text>();
        Transform rewardContent = Find(overlay.transform, "Content");
        Button claimAll = Find(overlay.transform, "ClaimAllButton").GetComponent<Button>();
        Button close = Find(overlay.transform, "CloseButton").GetComponent<Button>();

        WaveMilestoneRewardController controller = panelReward.gameObject.AddComponent<WaveMilestoneRewardController>();
        controller.Configure(openButton, badge, overlay, title, rewardContent, slotPrefab, claimAll, claimAll.GetComponent<Image>(), close);

        ModalTransition transition = overlay.GetComponent<ModalTransition>(); transition.Configure(cardRect);
        ModalBackdropDismiss dismiss = overlay.GetComponent<ModalBackdropDismiss>(); dismiss.Configure(cardRect, overlay, controller);
        overlay.SetActive(false);
        EditorUtility.SetDirty(controller); EditorUtility.SetDirty(transition); EditorUtility.SetDirty(dismiss);
        EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene); AssetDatabase.SaveAssets();
        Debug.Log("[WaveMilestoneRewardSceneBuilder] Scene UI and references configured.");
    }

    private static GameObject BuildSlotPrefab()
    {
        GameObject root = UiObject(null, "WaveMilestoneRewardSlot", typeof(Image), typeof(LayoutElement), typeof(WaveMilestoneRewardSlotView));
        root.GetComponent<RectTransform>().sizeDelta = new Vector2(880f, 164f);
        Image bg = root.GetComponent<Image>(); bg.sprite = LoadSprite(PanelSpritePath); bg.type = Image.Type.Sliced; bg.color = new Color(0.92f, 0.94f, 0.96f, 1f);
        root.GetComponent<LayoutElement>().preferredHeight = 164f;
        TMP_Text wave = Text(root.transform, "WaveLabel", "WAVE\n<size=150%>3</size>", 28f, TextAlignmentOptions.Center, Navy);
        SetRect(wave.rectTransform, new Vector2(0f, 0.5f), new Vector2(82f, 0f), new Vector2(132f, 132f));
        GameObject rewardContent = UiObject(root.transform, "RewardItems", typeof(HorizontalLayoutGroup));
        RectTransform itemsRect = (RectTransform)rewardContent.transform;
        itemsRect.anchorMin = new Vector2(0f, 0f); itemsRect.anchorMax = new Vector2(1f, 1f);
        itemsRect.offsetMin = new Vector2(164f, 16f); itemsRect.offsetMax = new Vector2(-204f, -16f);
        HorizontalLayoutGroup itemsLayout = rewardContent.GetComponent<HorizontalLayoutGroup>();
        itemsLayout.spacing = 12f; itemsLayout.childAlignment = TextAnchor.MiddleLeft;
        itemsLayout.childControlWidth = false; itemsLayout.childControlHeight = false;
        itemsLayout.childForceExpandWidth = false; itemsLayout.childForceExpandHeight = false;
        Button claim = ButtonObject(root.transform, "ClaimButton", "수령", new Color(0.32f, 0.34f, 0.38f, 1f), 32f);
        SetRect((RectTransform)claim.transform, new Vector2(1f, 0.5f), new Vector2(-100f, 0f), new Vector2(168f, 88f));
        claim.transition = Selectable.Transition.None;
        TMP_Text state = claim.transform.Find("Label").GetComponent<TMP_Text>();
        GameObject rewardItemPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(RewardItemPrefabPath);
        root.GetComponent<WaveMilestoneRewardSlotView>().Configure(wave, rewardContent.transform, rewardItemPrefab, state, claim, claim.GetComponent<Image>());
        PrefabUtility.SaveAsPrefabAsset(root, SlotPrefabPath); Object.DestroyImmediate(root);
        AssetDatabase.SaveAssets();
        AssetDatabase.ImportAsset(SlotPrefabPath, ImportAssetOptions.ForceSynchronousImport);
        return AssetDatabase.LoadAssetAtPath<GameObject>(SlotPrefabPath)
            ?? throw new System.InvalidOperationException($"Failed to reload slot prefab: {SlotPrefabPath}");
    }

    private static GameObject BuildOverlayPrefab()
    {
        GameObject overlay = UiObject(null, "WaveMilestoneRewardOverlay", typeof(Image), typeof(CanvasGroup), typeof(ModalBackdropDismiss), typeof(ModalTransition), typeof(SafeAreaFitter));
        Stretch((RectTransform)overlay.transform);
        overlay.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.001f);
        GameObject backdrop = ImageObject(overlay.transform, "ModalBackdrop", new Color(0f, 0f, 0f, 0.75f), null);
        Stretch((RectTransform)backdrop.transform);

        GameObject card = ImageObject(overlay.transform, "ModalCard", Navy, LoadSprite(PanelSpritePath));
        card.GetComponent<Image>().type = Image.Type.Sliced;
        RectTransform cardRect = (RectTransform)card.transform;
        cardRect.anchorMin = new Vector2(0.04f, 0.12f); cardRect.anchorMax = new Vector2(0.96f, 0.88f);
        cardRect.offsetMin = cardRect.offsetMax = Vector2.zero;
        overlay.GetComponent<SafeAreaFitter>().Configure(new[] { cardRect });

        GameObject header = ImageObject(card.transform, "Header", NavyLight, null);
        RectTransform headerRect = (RectTransform)header.transform;
        headerRect.anchorMin = new Vector2(0f, 1f); headerRect.anchorMax = Vector2.one;
        headerRect.pivot = new Vector2(0.5f, 1f); headerRect.sizeDelta = new Vector2(0f, 112f); headerRect.anchoredPosition = Vector2.zero;
        TMP_Text title = Text(header.transform, "Label", "웨이브 달성 보상  1-1", 48f, TextAlignmentOptions.Center, Color.white);
        Stretch(title.rectTransform, 96f, 8f, -96f, -8f);
        Button close = ButtonObject(header.transform, "CloseButton", "×", Red, 40f);
        SetRect((RectTransform)close.transform, new Vector2(1f, 0.5f), new Vector2(-52f, 0f), new Vector2(80f, 80f));

        GameObject scrollObject = ImageObject(card.transform, "RewardScroll", new Color(0.02f, 0.08f, 0.16f, 0.72f), null);
        RectTransform scrollRectTransform = (RectTransform)scrollObject.transform;
        scrollRectTransform.anchorMin = Vector2.zero; scrollRectTransform.anchorMax = Vector2.one;
        scrollRectTransform.offsetMin = new Vector2(24f, 144f); scrollRectTransform.offsetMax = new Vector2(-24f, -128f);
        ScrollRect scroll = scrollObject.AddComponent<ScrollRect>(); scroll.horizontal = false;
        GameObject viewport = UiObject(scrollObject.transform, "Viewport", typeof(Image), typeof(RectMask2D));
        Stretch((RectTransform)viewport.transform, 16f, 16f, -16f, -16f);
        viewport.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.001f);
        GameObject rewardContent = UiObject(viewport.transform, "Content", typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        RectTransform rewardContentRect = (RectTransform)rewardContent.transform;
        rewardContentRect.anchorMin = new Vector2(0f, 1f); rewardContentRect.anchorMax = Vector2.one;
        rewardContentRect.pivot = new Vector2(0.5f, 1f); rewardContentRect.sizeDelta = Vector2.zero;
        VerticalLayoutGroup layout = rewardContent.GetComponent<VerticalLayoutGroup>();
        layout.spacing = 16f; layout.padding = new RectOffset(8, 8, 8, 8); layout.childControlHeight = false; layout.childControlWidth = true;
        rewardContent.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        scroll.viewport = (RectTransform)viewport.transform; scroll.content = rewardContentRect;

        Button claimAll = ButtonObject(card.transform, "ClaimAllButton", "전부 수령", new Color(0.32f, 0.34f, 0.38f, 1f), 36f);
        SetRect((RectTransform)claimAll.transform, new Vector2(0.5f, 0f), new Vector2(0f, 68f), new Vector2(360f, 100f));
        claimAll.transition = Selectable.Transition.None;

        overlay.GetComponent<ModalTransition>().Configure(cardRect);
        PrefabUtility.SaveAsPrefabAsset(overlay, OverlayPrefabPath);
        Object.DestroyImmediate(overlay);
        AssetDatabase.SaveAssets();
        AssetDatabase.ImportAsset(OverlayPrefabPath, ImportAssetOptions.ForceSynchronousImport);
        return AssetDatabase.LoadAssetAtPath<GameObject>(OverlayPrefabPath)
            ?? throw new System.InvalidOperationException($"Failed to reload overlay prefab: {OverlayPrefabPath}");
    }

    private static void RemoveExisting(Scene scene, string name) { GameObject existing = Find(scene, name); if (existing != null) Object.DestroyImmediate(existing); }
    private static GameObject Find(Scene scene, string name) { foreach (GameObject root in scene.GetRootGameObjects()) { Transform found = Find(root.transform, name); if (found != null) return found.gameObject; } return null; }
    private static Transform Find(Transform root, string name) { foreach (Transform child in root.GetComponentsInChildren<Transform>(true)) if (child.name == name) return child; return null; }
    private static GameObject UiObject(Transform parent, string name, params System.Type[] components) { GameObject go = new(name, typeof(RectTransform)); if (parent != null) go.transform.SetParent(parent, false); foreach (System.Type type in components) go.AddComponent(type); return go; }
    private static GameObject ImageObject(Transform parent, string name, Color color, Sprite sprite) { GameObject go = UiObject(parent, name, typeof(Image)); Image image = go.GetComponent<Image>(); image.color = color; image.sprite = sprite; return go; }
    private static TMP_Text Text(Transform parent, string name, string value, float size, TextAlignmentOptions alignment, Color color) { GameObject go = UiObject(parent, name, typeof(TextMeshProUGUI)); TMP_Text text = go.GetComponent<TMP_Text>(); text.text = value; text.fontSize = size; text.alignment = alignment; text.color = color; return text; }
    private static Button ButtonObject(Transform parent, string name, string label, Color color, float fontSize) { GameObject go = ImageObject(parent, name, color, LoadSprite(PanelSpritePath)); go.GetComponent<Image>().type = Image.Type.Sliced; Button button = go.AddComponent<Button>(); button.targetGraphic = go.GetComponent<Image>(); TMP_Text text = Text(go.transform, "Label", label, fontSize, TextAlignmentOptions.Center, Color.white); Stretch(text.rectTransform, 8f, 8f, -8f, -8f); return button; }
    private static void Stretch(RectTransform rect, float left = 0f, float bottom = 0f, float right = 0f, float top = 0f) { rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.offsetMin = new Vector2(left, bottom); rect.offsetMax = new Vector2(right, top); }
    private static void SetRect(RectTransform rect, Vector2 anchor, Vector2 position, Vector2 size) { rect.anchorMin = rect.anchorMax = anchor; rect.anchoredPosition = position; rect.sizeDelta = size; }
    private static Sprite LoadSprite(string path) => AssetDatabase.LoadAssetAtPath<Sprite>(path);
    private static void Set(SerializedObject serialized, string name, Object value) { SerializedProperty property = serialized.FindProperty(name) ?? throw new System.InvalidOperationException($"Missing field: {name}"); property.objectReferenceValue = value; }
}
#endif
