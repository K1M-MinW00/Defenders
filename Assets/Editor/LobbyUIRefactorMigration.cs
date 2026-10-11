using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class LobbyUIRefactorMigration
{
    private const string ScenePath = "Assets/Scenes/LobbyScene.unity";
    private const string RecruitUnitIconPrefabPath = "Assets/Prefabs/UI/UI_UnitIcon.prefab";

    [MenuItem("Tools/UI/Refactor Lobby Modals")]
    public static void Run()
    {
        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        NormalizeFeatureRootNames();
        NormalizeKnownModalHierarchy();
        NormalizePersistentFeatureHierarchy();
        NormalizeItemDetailOverlay();
        NormalizeRootPanel(FindAll("InventoryOverlay").FirstOrDefault()?.GetComponent<InventoryPanelView>(), "Inventory_Popup");
        NormalizeRootPanel(FindAll("MailboxOverlay").FirstOrDefault()?.GetComponent<MailboxPanelView>(), "Mailbox_Popup");
        ConsolidateEquipmentDetailOverlay();
        ConfigureUnitEquipmentViews();
        ConfigureEquipmentGrids();
        NormalizeModal("EquipmentChangeOverlay");
        NormalizeModal("EquipmentEnhancementOverlay");
        NormalizeModal("EquipmentDismantleOverlay");
        NormalizeModal("EquipmentFilterOverlay");
        NormalizeEquipmentRewardOverlay();
        NormalizeLegacyLobbyModals();
        ConfigureFuelProductSlots();
        ConfigureSettingsPanelReferences();
        ConfigureRecruitPresentationController();
        ConfigureRecruitPresentationOverlay();
        ConfigureRecruitUnitIconPrefab();
        NormalizeEmbeddedLobbyModals();
        ConfigureShopGrids();
        EnsureExitConfirmationOverlay();
        NormalizeRemainingLobbyOverlays();
        EnsureModalTransitions();
        ConfigureModalRuntimeReferences();
        ConfigureLobbyModalStack();
        NormalizeModalChildNames();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[LobbyUIRefactorMigration] Modal dismiss paths and equipment detail overlay consolidated.");
    }

    [MenuItem("Tools/UI/Configure Fuel Product Slots")]
    public static void ConfigureFuelProductSlotsInScene()
    {
        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        ConfigureFuelProductSlots();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[LobbyUIRefactorMigration] Fuel product slots configured.");
    }

    [MenuItem("Tools/UI/Configure Settings Panel References")]
    public static void ConfigureSettingsPanelReferencesInScene()
    {
        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        ConfigureSettingsPanelReferences();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[LobbyUIRefactorMigration] Settings panel references configured.");
    }

    [MenuItem("Tools/UI/Configure Modal Runtime References")]
    public static void ConfigureModalRuntimeReferencesInScene()
    {
        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        EnsureModalTransitions();
        ConfigureModalRuntimeReferences();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[LobbyUIRefactorMigration] Modal runtime references configured.");
    }

    [MenuItem("Tools/UI/Configure Lobby Modal Stack")]
    public static void ConfigureLobbyModalStackInScene()
    {
        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        ConfigureLobbyModalStack();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[LobbyUIRefactorMigration] Lobby modal stack configured.");
    }

    [MenuItem("Tools/UI/Configure Recruit Presentation Controller")]
    public static void ConfigureRecruitPresentationControllerInScene()
    {
        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        ConfigureRecruitPresentationController();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[LobbyUIRefactorMigration] Recruit presentation controller configured.");
    }

    [MenuItem("Tools/UI/Configure Recruit Presentation Overlay")]
    public static void ConfigureRecruitPresentationOverlayInScene()
    {
        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        ConfigureRecruitPresentationController();
        ConfigureRecruitPresentationOverlay();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[LobbyUIRefactorMigration] Recruit presentation overlay configured.");
    }

    [MenuItem("Tools/UI/Configure Recruit Unit Icon Prefab")]
    public static void ConfigureRecruitUnitIconPrefabFromMenu()
    {
        ConfigureRecruitUnitIconPrefab();
        Debug.Log("[LobbyUIRefactorMigration] Recruit unit icon prefab configured.");
    }

    private static void NormalizeFeatureRootNames()
    {
        RenameRoot("Inventory_Panel", "InventoryOverlay");
        RenameRoot("MailBox_Panel", "MailboxOverlay");
        RenameRoot("Setting_Panel", "SettingsOverlay");
        RenameRoot("FuelDetail_Panel", "FuelDetailOverlay");
        RenameRoot("RatePopup_Panel", "RecruitRateOverlay");
        RenameRoot("GemConfirm_Panel", "RecruitConfirmOverlay");
        RenameRoot("EditProfile_Panel", "ProfileEditOverlay");
        RenameRoot("EditNickname_Panel", "NicknameEditOverlay");
        RenameRoot("IdleRewardPopup", "IdleRewardOverlay");
        RenameRoot("UnitDetail_Panel", "UnitDetailPanel");
        RenameRoot("Shop_Panel", "ShopPanel");
        RenameRoot("ResultPopup_Panel", "RecruitResultOverlay");
        RenameRoot("LinkAccount_Panel", "AccountLinkOverlay");
        RenameRoot("Lab_OfferOverlay", "LabOfferOverlay");
        RenameRoot("Lab_DetailOverlay", "LabDetailOverlay");
    }

    private static void RenameRoot(string legacyName, string normalizedName)
    {
        Transform target = FindAll(normalizedName).FirstOrDefault() ?? FindAll(legacyName).FirstOrDefault();
        if (target != null) target.name = normalizedName;
    }

    private static void NormalizeKnownModalHierarchy()
    {
        RenameWithin("RecruitRateOverlay", "Back_Button", "CloseButton");
        RenameWithin("RecruitRateOverlay", "Scroll View", "RateScroll");
        RenameWithin("RecruitResultOverlay", "Back_Button", "CloseButton");

        RenameWithin("InventoryOverlay", "Buttons_Group", "InventoryTabs");
        RenameWithin("MailboxOverlay", "Buttons_Group", "ActionButtons");
        RenameWithin("RecruitConfirmOverlay", "Buttons_Group", "ActionButtons");
        RenameWithin("ProfileEditOverlay", "Buttons_Group", "ActionButtons");
        RenameWithin("NicknameEditOverlay", "Buttons_Group", "ActionButtons");
        RenameWithin("AccountLinkOverlay", "Buttons_Group", "ActionButtons");
        RenameWithin("AccountLinkOverlay", "Sign_Google", "GoogleSignInSection");
        RenameWithin("AccountLinkOverlay", "Sign_Apple", "AppleSignInSection");
        RenameWithin("FuelDetailOverlay", "Fuel_Group", "FuelProducts");

        NormalizeLooseHeader("RecruitRateOverlay");
        NormalizeLooseHeader("RecruitResultOverlay");

        Transform confirm = FindAll("RecruitConfirmOverlay").FirstOrDefault();
        RenameDirectSemanticChild(Find(confirm, "Message"), "Text (TMP)", "Label");

        Transform nickname = FindAll("NicknameEditOverlay").FirstOrDefault();
        Transform input = Find(nickname, "InputField (TMP)");
        if (input != null) input.name = "NicknameInput";
        RenameDirectSemanticChild(input, "Text (TMP)", "InputText");

        Transform account = FindAll("AccountLinkOverlay").FirstOrDefault();
        NormalizeProviderSection(Find(account, "GoogleSignInSection"));
        NormalizeProviderSection(Find(account, "AppleSignInSection"));

        Transform equipmentDetail = FindAll("EquipmentDetailOverlay").FirstOrDefault();
        RenameDirectSemanticChild(Find(equipmentDetail, "BaseEffect"), "Image", "EffectIcon");

        Transform equipmentFilter = FindAll("EquipmentFilterOverlay").FirstOrDefault();
        foreach (string cardName in new[] { "Main", "Sub", "Helmet", "Armor" })
        {
            Transform card = Find(equipmentFilter, cardName);
            RenameDirectSemanticChild(Find(card, "Icon"), "Image", "Artwork");
        }

        Transform rate = FindAll("RecruitRateOverlay").FirstOrDefault();
        foreach (string groupName in new[] { "Normal_Panel", "Rare_Panel", "Legend_Panel" })
        {
            Transform group = Find(rate, groupName);
            Transform bar = DirectChild(group, "Image");
            if (bar == null) continue;
            bar.name = "RateBar";
            RenameDirectSemanticChild(bar, "Text (TMP)", "RateLabel");
        }
    }

    private static void NormalizePersistentFeatureHierarchy()
    {
        Transform main = FindAll("MainPanel").FirstOrDefault(IsCanvasChild) ??
                         FindAll("Main_Panel").FirstOrDefault(IsCanvasChild);
        if (main != null) main.name = "MainPanel";
        RenameDirectSemanticChild(main, "Battle_Panel", "BattlePanel");
        RenameDirectSemanticChild(main, "Unit_Panel", "UnitPanel");
        RenameDirectSemanticChild(main, "Recruit_Panel", "RecruitPanel");
        RenameDirectSemanticChild(main, "Lab_Panel", "LabPanel");
        RenameDirectSemanticChild(main, "Under_Bar", "NavigationBar");

        Transform unitDetail = FindAll("UnitDetailPanel").FirstOrDefault();
        Transform detailContent = DirectChild(unitDetail, "Main_Panel") ?? DirectChild(unitDetail, "ContentPanel");
        if (detailContent != null) detailContent.name = "ContentPanel";
        RenameDirectSemanticChild(unitDetail, "Under_Bar", "NavigationBar");
        RenameDirectSemanticChild(detailContent, "Common_Panel", "CommonPanel");
        RenameDirectSemanticChild(detailContent, "Training_Panel", "TrainingPanel");
        RenameDirectSemanticChild(detailContent, "Promotion_Panel", "PromotionPanel");
        RenameDirectSemanticChild(detailContent, "LimitBreak_Panel", "LimitBreakPanel");
        RenameDirectSemanticChild(detailContent, "Equipment_Panel", "EquipmentPanel");

        NormalizeAmbiguousLeafNames(main);
        NormalizeAmbiguousLeafNames(unitDetail);
    }

    private static bool IsCanvasChild(Transform value) => value?.parent != null && value.parent.GetComponent<Canvas>() != null;

    private static void NormalizeAmbiguousLeafNames(Transform root)
    {
        if (root == null) return;

        foreach (Button button in root.GetComponentsInChildren<Button>(true))
        {
            TMP_Text[] labels = button.transform.Cast<Transform>()
                .Where(x => x.name == "Text (TMP)")
                .Select(x => x.GetComponent<TMP_Text>())
                .Where(x => x != null)
                .ToArray();
            for (int i = 0; i < labels.Length; i++)
                labels[i].name = i == 0 ? "Label" : i == 1 ? "CostLabel" : $"SecondaryLabel{i}";

            Image[] icons = button.transform.Cast<Transform>()
                .Where(x => x.name == "Image")
                .Select(x => x.GetComponent<Image>())
                .Where(x => x != null)
                .ToArray();
            for (int i = 0; i < icons.Length; i++)
                icons[i].name = i == 0 ? "Icon" : $"SecondaryIcon{i}";
        }

        foreach (TMP_Text text in root.GetComponentsInChildren<TMP_Text>(true).Where(x => x.name == "Text (TMP)").ToArray())
        {
            Transform parent = text.transform.parent;
            if (parent != null && parent.name == "Title")
                text.name = "Label";
            else if (parent != null && parent.name == "Text")
            {
                parent.name = "LabelContainer";
                text.name = "Label";
            }
            else
                text.name = "ValueLabel";
        }

        foreach (Image image in root.GetComponentsInChildren<Image>(true).Where(x => x.name == "Image").ToArray())
            image.name = "Icon";
    }

    private static void NormalizeLooseHeader(string rootName)
    {
        Transform root = FindAll(rootName).FirstOrDefault();
        Transform title = DirectChild(root, "Title");
        if (title == null) return;
        title.name = "Header";
        TMP_Text label = title.GetComponentInChildren<TMP_Text>(true);
        if (label != null) label.name = "Label";
    }

    private static void NormalizeProviderSection(Transform section)
    {
        RenameDirectSemanticChild(section, "Image", "ProviderIcon");
        RenameDirectSemanticChild(section, "Text (TMP)", "ProviderLabel");
    }

    private static void RenameDirectSemanticChild(Transform parent, string oldName, string newName)
    {
        Transform child = DirectChild(parent, oldName);
        if (child != null) child.name = newName;
    }

    private static void RenameWithin(string rootName, string oldName, string newName)
    {
        Transform root = FindAll(rootName).FirstOrDefault();
        Transform target = Find(root, oldName);
        if (target != null) target.name = newName;
    }

    private static void EnsureModalTransitions()
    {
        foreach (ModalBackdropDismiss dismiss in Object.FindObjectsByType<ModalBackdropDismiss>(
                     FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            Transform card = DirectChild(dismiss.transform, "ModalCard");
            ModalTransition transition = dismiss.GetComponent<ModalTransition>() ??
                                         dismiss.gameObject.AddComponent<ModalTransition>();
            transition.Configure(card as RectTransform);
            EditorUtility.SetDirty(transition);
        }
    }

    private static void NormalizeRemainingLobbyOverlays()
    {
        RatePopupPanelView rate = Object.FindFirstObjectByType<RatePopupPanelView>(FindObjectsInactive.Include);
        NormalizeFullScreenOverlay(rate?.transform, rate, allowBackdropDismiss: false);

        RecruitResultPopupView result = Object.FindFirstObjectByType<RecruitResultPopupView>(FindObjectsInactive.Include);
        NormalizeFullScreenOverlay(result?.transform, result, allowBackdropDismiss: false);

        GemConfirmPopupView gem = Object.FindFirstObjectByType<GemConfirmPopupView>(FindObjectsInactive.Include);
        if (gem != null)
        {
            Transform card = DirectChild(gem.transform, "ModalCard") ?? DirectChild(gem.transform, "Popup");
            if (card != null) card.name = "ModalCard";
            NormalizeFullScreenOverlay(gem.transform, gem, allowBackdropDismiss: false);
        }

        LabPanelView lab = Object.FindFirstObjectByType<LabPanelView>(FindObjectsInactive.Include);
        Transform offer = FindAll("LabOfferOverlay").FirstOrDefault() ?? FindAll("Lab_OfferOverlay").FirstOrDefault();
        Transform detail = FindAll("LabDetailOverlay").FirstOrDefault() ?? FindAll("Lab_DetailPopup").FirstOrDefault() ?? FindAll("Lab_DetailOverlay").FirstOrDefault();
        NormalizeFullScreenOverlay(offer, lab, allowBackdropDismiss: false);
        NormalizeFullScreenOverlay(detail, lab, allowBackdropDismiss: true);
        if (detail != null) detail.name = "LabDetailOverlay";

        UnitDetailView unitDetail = Object.FindFirstObjectByType<UnitDetailView>(FindObjectsInactive.Include);
        NormalizeFullScreenOverlay(unitDetail?.transform, unitDetail, allowBackdropDismiss: false);
    }

    private static void ConfigureModalRuntimeReferences()
    {
        foreach (ModalBackdropDismiss dismiss in Object.FindObjectsByType<ModalBackdropDismiss>(
                     FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            CanvasGroup group = dismiss.GetComponent<CanvasGroup>() ?? dismiss.gameObject.AddComponent<CanvasGroup>();
            group.interactable = true;
            group.blocksRaycasts = true;
            ModalTransition transition = dismiss.GetComponent<ModalTransition>();

            SerializedObject serialized = new SerializedObject(dismiss);
            serialized.FindProperty("canvasGroup").objectReferenceValue = group;
            serialized.FindProperty("transition").objectReferenceValue = transition;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            EditorUtility.SetDirty(group);
            EditorUtility.SetDirty(dismiss);
        }
    }

    private static void EnsureExitConfirmationOverlay()
    {
        Canvas canvas = Object.FindFirstObjectByType<Canvas>(FindObjectsInactive.Include);
        if (canvas == null) return;

        Transform existing = DirectChild(canvas.transform, "ExitConfirmationOverlay");
        GameObject overlay = existing != null
            ? existing.gameObject
            : CreateUiObject("ExitConfirmationOverlay", canvas.transform);
        Stretch(overlay.transform as RectTransform);
        Image backdrop = overlay.GetComponent<Image>() ?? overlay.AddComponent<Image>();
        backdrop.color = new Color(0f, 0f, 0f, 0.75f);
        backdrop.raycastTarget = true;

        Transform cardTransform = DirectChild(overlay.transform, "ModalCard");
        GameObject card = cardTransform != null ? cardTransform.gameObject : CreateUiObject("ModalCard", overlay.transform);
        RectTransform cardRect = card.transform as RectTransform;
        cardRect.anchorMin = new Vector2(0.14f, 0.37f);
        cardRect.anchorMax = new Vector2(0.86f, 0.63f);
        cardRect.offsetMin = cardRect.offsetMax = Vector2.zero;
        Image cardImage = card.GetComponent<Image>() ?? card.AddComponent<Image>();
        cardImage.color = new Color32(222, 238, 245, 255);

        Transform headerTransform = DirectChild(card.transform, "Header");
        GameObject header = headerTransform != null ? headerTransform.gameObject : CreateUiObject("Header", card.transform);
        SetAnchors(header.transform as RectTransform, new Vector2(0f, 0.72f), Vector2.one);
        Image headerImage = header.GetComponent<Image>() ?? header.AddComponent<Image>();
        headerImage.color = new Color32(55, 118, 213, 255);
        TMP_Text title = EnsureText(header.transform, "Label", "게임 종료", 44, TextAlignmentOptions.Center);
        Stretch(title.rectTransform);

        TMP_Text message = EnsureText(card.transform, "Message", "게임을 종료하시겠습니까?", 34, TextAlignmentOptions.Center);
        SetAnchors(message.rectTransform, new Vector2(0.08f, 0.34f), new Vector2(0.92f, 0.7f));

        Button cancel = EnsureButton(card.transform, "CancelButton", "취소", new Color32(91, 112, 130, 255));
        SetAnchors(cancel.transform as RectTransform, new Vector2(0.08f, 0.08f), new Vector2(0.47f, 0.3f));
        Button confirm = EnsureButton(card.transform, "ConfirmButton", "종료", new Color32(230, 57, 70, 255));
        SetAnchors(confirm.transform as RectTransform, new Vector2(0.53f, 0.08f), new Vector2(0.92f, 0.3f));

        LobbyExitConfirmationView view = overlay.GetComponent<LobbyExitConfirmationView>() ?? overlay.AddComponent<LobbyExitConfirmationView>();
        view.Configure(cancel, confirm);
        ModalBackdropDismiss dismiss = overlay.GetComponent<ModalBackdropDismiss>() ?? overlay.AddComponent<ModalBackdropDismiss>();
        dismiss.Configure(cardRect, overlay, view, false);
        overlay.SetActive(false);
        EditorUtility.SetDirty(view);
        EditorUtility.SetDirty(dismiss);
    }

    private static void ConfigureLobbyModalStack()
    {
        LobbyModalStack stack = Object.FindFirstObjectByType<LobbyModalStack>(FindObjectsInactive.Include);
        if (stack == null)
        {
            GameObject stackObject = new GameObject(nameof(LobbyModalStack));
            stack = stackObject.AddComponent<LobbyModalStack>();
        }

        LobbyExitConfirmationView exitConfirmation =
            Object.FindFirstObjectByType<LobbyExitConfirmationView>(FindObjectsInactive.Include);
        SerializedObject serialized = new SerializedObject(stack);
        serialized.FindProperty("exitConfirmation").objectReferenceValue = exitConfirmation;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(stack);
    }

    private static GameObject CreateUiObject(string name, Transform parent)
    {
        GameObject value = new(name, typeof(RectTransform));
        value.transform.SetParent(parent, false);
        return value;
    }

    private static void SetAnchors(RectTransform rect, Vector2 min, Vector2 max)
    {
        rect.anchorMin = min;
        rect.anchorMax = max;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
    }

    private static TMP_Text EnsureText(Transform parent, string name, string value, float size, TextAlignmentOptions alignment)
    {
        Transform child = DirectChild(parent, name);
        GameObject target = child != null ? child.gameObject : CreateUiObject(name, parent);
        TMP_Text text = target.GetComponent<TMP_Text>() ?? target.AddComponent<TextMeshProUGUI>();
        text.text = value;
        text.fontSize = size;
        text.alignment = alignment;
        text.color = Color.white;
        return text;
    }

    private static Button EnsureButton(Transform parent, string name, string label, Color color)
    {
        Transform child = DirectChild(parent, name);
        GameObject target = child != null ? child.gameObject : CreateUiObject(name, parent);
        Image image = target.GetComponent<Image>() ?? target.AddComponent<Image>();
        image.color = color;
        Button button = target.GetComponent<Button>() ?? target.AddComponent<Button>();
        button.targetGraphic = image;
        TMP_Text text = EnsureText(target.transform, "Label", label, 34, TextAlignmentOptions.Center);
        Stretch(text.rectTransform);
        return button;
    }

    private static void ConfigureShopGrids()
    {
        Transform shopPanel = FindAll("ShopPanel").FirstOrDefault() ?? FindAll("Shop_Panel").FirstOrDefault();
        if (shopPanel == null) return;

        foreach (ShopResponsiveGrid responsive in shopPanel.GetComponentsInChildren<ShopResponsiveGrid>(true))
        {
            GridLayoutGroup grid = responsive.GetComponent<GridLayoutGroup>();
            int columns = grid != null && grid.constraint == GridLayoutGroup.Constraint.FixedColumnCount
                ? Mathf.Max(1, grid.constraintCount)
                : 2;
            float width = columns == 1 ? 840f : columns == 3 ? 255f : 400f;
            float height = columns == 1 ? 430f : 340f;
            responsive.Configure(columns, width / height, 20f, 4f, width);
            if (grid != null)
            {
                grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
                grid.startAxis = GridLayoutGroup.Axis.Horizontal;
                grid.childAlignment = TextAnchor.UpperCenter;
                EditorUtility.SetDirty(grid);
            }
            EditorUtility.SetDirty(responsive);
        }
    }

    private static void NormalizeEmbeddedLobbyModals()
    {
        ShopPurchaseResultPopup result = Object.FindFirstObjectByType<ShopPurchaseResultPopup>(FindObjectsInactive.Include);
        NormalizeFullScreenOverlay(result?.transform, result, allowBackdropDismiss: true);

        // 구매 확인은 파괴적/결제 성격의 확인창이므로 바깥 클릭 닫기를 의도적으로 넣지 않는다.
        ShopPurchaseConfirmPopup confirm = Object.FindFirstObjectByType<ShopPurchaseConfirmPopup>(FindObjectsInactive.Include);
        NormalizeFullScreenOverlay(confirm?.transform, confirm, allowBackdropDismiss: false);

        IdleRewardPanelView idleView = Object.FindFirstObjectByType<IdleRewardPanelView>(FindObjectsInactive.Include);
        Transform idleOverlay = FindAll("IdleRewardOverlay").FirstOrDefault() ?? FindAll("IdleRewardPopup").FirstOrDefault();
        NormalizeFullScreenOverlay(idleOverlay, idleView, allowBackdropDismiss: true);
    }

    private static void NormalizeFullScreenOverlay(Transform overlay, MonoBehaviour handler, bool allowBackdropDismiss)
    {
        if (overlay == null || handler == null) return;
        Stretch(overlay as RectTransform);

        Transform card = DirectChild(overlay, "ModalCard") ?? DirectChild(overlay, "Card") ?? DirectChild(overlay, "Window");
        if (card != null) card.name = "ModalCard";

        if (!allowBackdropDismiss)
        {
            ModalBackdropDismiss stackEntry = overlay.GetComponent<ModalBackdropDismiss>() ?? overlay.gameObject.AddComponent<ModalBackdropDismiss>();
            stackEntry.Configure(card as RectTransform, overlay.gameObject, handler, false);
            EditorUtility.SetDirty(stackEntry);
            return;
        }

        Image backdrop = overlay.GetComponent<Image>();
        if (backdrop != null) backdrop.raycastTarget = true;
        Button duplicateButton = overlay.GetComponent<Button>();
        if (duplicateButton != null) Object.DestroyImmediate(duplicateButton);
        ModalBackdropDismiss dismiss = overlay.GetComponent<ModalBackdropDismiss>() ?? overlay.gameObject.AddComponent<ModalBackdropDismiss>();
        dismiss.Configure(card as RectTransform, overlay.gameObject, handler);
        EditorUtility.SetDirty(handler);
        EditorUtility.SetDirty(dismiss);
    }

    private static void NormalizeLegacyLobbyModals()
    {
        NormalizeLegacyLobbyModal("SettingsOverlay", Object.FindFirstObjectByType<SettingsPanelView>(FindObjectsInactive.Include));
        NormalizeLegacyLobbyModal("FuelDetailOverlay", Object.FindFirstObjectByType<FuelPanelView>(FindObjectsInactive.Include));
        NormalizeLegacyLobbyModal("AccountLinkOverlay", FindAll("AccountLinkOverlay").FirstOrDefault()?.GetComponent<SimplePopupView>());
        NormalizeLegacyLobbyModal("ProfileEditOverlay", Object.FindFirstObjectByType<EditProfilePanelView>(FindObjectsInactive.Include));
        NormalizeLegacyLobbyModal("NicknameEditOverlay", Object.FindFirstObjectByType<EditNicknamePanelView>(FindObjectsInactive.Include));
    }

    private static void ConfigureFuelProductSlots()
    {
        FuelPanelView view = Object.FindFirstObjectByType<FuelPanelView>(FindObjectsInactive.Include);
        if (view == null) return;

        Transform products = Find(view.transform, "FuelProducts") ?? Find(view.transform, "Fuel_Group");
        CommonSlotUI adSlot = ConfigureFuelSlot(Find(Find(products, "Fuel10_Slot"), "ItemSlot"));
        CommonSlotUI gemSlot = ConfigureFuelSlot(Find(Find(products, "Fuel50_Slot"), "ItemSlot"));
        Transform buyButton = Find(Find(products, "Fuel50_Slot"), "Buy_Button") ??
                              Find(Find(products, "Fuel50_Slot"), "BuyButton");
        TMP_Text gemCostLabel = Find(buyButton, "Label")?.GetComponent<TMP_Text>() ??
                                Find(buyButton, "Text (TMP)")?.GetComponent<TMP_Text>();

        SerializedObject serialized = new(view);
        serialized.FindProperty("rewardAdRewardSlot").objectReferenceValue = adSlot;
        serialized.FindProperty("gemPurchaseRewardSlot").objectReferenceValue = gemSlot;
        serialized.FindProperty("gemPurchaseButtonText").objectReferenceValue = gemCostLabel;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(view);
    }

    private static void ConfigureSettingsPanelReferences()
    {
        SettingsPanelView view = Object.FindFirstObjectByType<SettingsPanelView>(FindObjectsInactive.Include);
        if (view == null) return;

        SerializedObject serialized = new(view);
        serialized.FindProperty("panelRoot").objectReferenceValue = view.gameObject;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(view);
    }

    private static void ConfigureRecruitPresentationController()
    {
        LobbyRecruitPanelView view = Object.FindFirstObjectByType<LobbyRecruitPanelView>(FindObjectsInactive.Include);
        if (view == null) return;

        RecruitPresentationController controller = view.GetComponent<RecruitPresentationController>() ??
                                                   view.gameObject.AddComponent<RecruitPresentationController>();
        SerializedObject serialized = new(view);
        serialized.FindProperty("presentationController").objectReferenceValue = controller;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(view);
        EditorUtility.SetDirty(controller);
    }

    private static void ConfigureRecruitPresentationOverlay()
    {
        RecruitPresentationController controller = Object.FindFirstObjectByType<RecruitPresentationController>(FindObjectsInactive.Include);
        RecruitResultPopupView resultPopup = Object.FindFirstObjectByType<RecruitResultPopupView>(FindObjectsInactive.Include);
        if (controller == null || resultPopup?.transform.parent == null) return;

        RecruitPresentationOverlayView overlay = Object.FindFirstObjectByType<RecruitPresentationOverlayView>(FindObjectsInactive.Include);
        if (overlay == null)
            overlay = RecruitPresentationOverlayView.Create(resultPopup.transform.parent);
        overlay.name = "RecruitPresentationOverlay";

        SerializedObject serialized = new(controller);
        serialized.FindProperty("overlayView").objectReferenceValue = overlay;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(controller);
        EditorUtility.SetDirty(overlay);
    }

    private static void ConfigureRecruitUnitIconPrefab()
    {
        GameObject root = PrefabUtility.LoadPrefabContents(RecruitUnitIconPrefabPath);
        try
        {
            RecruitUnitIconView view = root.GetComponent<RecruitUnitIconView>();
            if (view == null)
                throw new System.InvalidOperationException("UI_UnitIcon prefab has no RecruitUnitIconView.");

            CanvasGroup canvasGroup = root.GetComponent<CanvasGroup>();
            if (!canvasGroup)
                canvasGroup = root.AddComponent<CanvasGroup>();
            SerializedObject serialized = new(view);
            serialized.FindProperty("revealCanvasGroup").objectReferenceValue = canvasGroup;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(view);
            PrefabUtility.SaveAsPrefabAsset(root, RecruitUnitIconPrefabPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    private static CommonSlotUI ConfigureFuelSlot(Transform slotTransform)
    {
        if (slotTransform == null) return null;

        CommonSlotUI slot = slotTransform.GetComponent<CommonSlotUI>() ??
                            slotTransform.gameObject.AddComponent<CommonSlotUI>();
        Button button = slotTransform.GetComponent<Button>() ?? slotTransform.gameObject.AddComponent<Button>();
        Image frame = slotTransform.GetComponent<Image>();
        Image icon = slotTransform.GetComponentsInChildren<Image>(true)
            .FirstOrDefault(image => image.transform != slotTransform);
        TMP_Text count = slotTransform.GetComponentInChildren<TMP_Text>(true);
        slot.Configure(icon, frame, count, button);
        EditorUtility.SetDirty(slot);
        EditorUtility.SetDirty(button);
        return slot;
    }

    private static void NormalizeLegacyLobbyModal(string rootName, MonoBehaviour handler)
    {
        Transform root = FindAll(rootName).FirstOrDefault();
        if (root == null || handler == null) return;

        Transform backdrop = DirectChild(root, "ModalBackdrop") ?? DirectChild(root, "BG");
        Transform card = DirectChild(root, "ModalCard") ?? DirectChild(root, "Popup");
        if (card == null) return;

        if (backdrop != null)
        {
            backdrop.name = "ModalBackdrop";
            Button legacyButton = backdrop.GetComponent<Button>();
            if (legacyButton != null) Object.DestroyImmediate(legacyButton);
            Image image = backdrop.GetComponent<Image>();
            if (image != null) image.raycastTarget = true;
        }

        card.name = "ModalCard";
        Transform title = DirectChild(card, "Title") ?? DirectChild(card, "Header");
        if (title != null)
        {
            title.name = "Header";
            TMP_Text label = title.GetComponentInChildren<TMP_Text>(true);
            if (label != null) label.name = "Label";
        }

        ModalBackdropDismiss dismiss = root.GetComponent<ModalBackdropDismiss>() ?? root.gameObject.AddComponent<ModalBackdropDismiss>();
        dismiss.Configure(card as RectTransform, root.gameObject, handler);
        EditorUtility.SetDirty(handler);
        EditorUtility.SetDirty(dismiss);
    }

    [MenuItem("Tools/UI/Report Lobby UI Conventions")]
    public static void Report()
    {
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        Canvas canvas = Object.FindFirstObjectByType<Canvas>(FindObjectsInactive.Include);
        if (canvas == null) { Debug.LogWarning("[LobbyUIReport] Canvas not found."); return; }

        Debug.Log("[LobbyUIReport] === Top-level panels and overlays ===");
        foreach (Transform child in canvas.transform.Cast<Transform>())
        {
            if (child.name.EndsWith("Panel") || child.name.EndsWith("_Panel") || child.name.EndsWith("Overlay"))
                Debug.Log($"[LobbyUIReport] ROOT {Path(child)} active={child.gameObject.activeSelf}");
        }

        Debug.Log("[LobbyUIReport] === Persistent button callbacks ===");
        foreach (Button button in Object.FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            int count = button.onClick.GetPersistentEventCount();
            for (int i = 0; i < count; i++)
                Debug.Log($"[LobbyUIReport] EVENT {Path(button.transform)} -> {button.onClick.GetPersistentTarget(i)?.GetType().Name}.{button.onClick.GetPersistentMethodName(i)}");
        }

        Debug.Log("[LobbyUIReport] === Modal dismiss components ===");
        foreach (ModalBackdropDismiss dismiss in Object.FindObjectsByType<ModalBackdropDismiss>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            Debug.Log($"[LobbyUIReport] DISMISS {Path(dismiss.transform)}");

        string[] ambiguous = { "BG", "Popup", "Card", "GameObject", "Image", "Text (TMP)" };
        Debug.Log("[LobbyUIReport] === Ambiguous names ===");
        foreach (Transform transform in Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                     .Where(x => ambiguous.Contains(x.name)))
            Debug.Log($"[LobbyUIReport] NAME {Path(transform)}");
    }

    private static void NormalizeRootPanel(MonoBehaviour view, string legacyCardName)
    {
        if (view == null) return;
        Transform root = view.transform;
        Transform backdrop = DirectChild(root, "BG") ?? DirectChild(root, "ModalBackdrop");
        Transform card = DirectChild(root, legacyCardName) ?? DirectChild(root, "ModalCard");
        if (backdrop != null)
        {
            backdrop.name = "ModalBackdrop";
            Button duplicateButton = backdrop.GetComponent<Button>();
            if (duplicateButton != null) Object.DestroyImmediate(duplicateButton);
            Image image = backdrop.GetComponent<Image>();
            if (image != null) image.raycastTarget = true;
        }
        if (card != null) card.name = "ModalCard";
        ModalBackdropDismiss dismiss = root.GetComponent<ModalBackdropDismiss>() ?? root.gameObject.AddComponent<ModalBackdropDismiss>();
        dismiss.Configure(card as RectTransform, root.gameObject, view);
        EditorUtility.SetDirty(view);
        EditorUtility.SetDirty(dismiss);
    }

    private static void ConfigureEquipmentGrids()
    {
        Transform dismantleOverlay = FindAll("EquipmentDismantleOverlay").FirstOrDefault();
        Transform dismantleScroll = Find(dismantleOverlay, "DismantleScroll");
        Transform viewport = DirectChild(dismantleScroll, "Viewport");
        Transform dismantleContent = DirectChild(viewport, "Content");
        if (dismantleContent == null) return;
        GridLayoutGroup grid = dismantleContent.GetComponent<GridLayoutGroup>();
        if (grid == null) return;
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 5;
        grid.childAlignment = TextAnchor.UpperLeft;
        grid.cellSize = new Vector2(125f, 125f);
        ResponsiveSquareGrid responsive = dismantleContent.GetComponent<ResponsiveSquareGrid>() ?? dismantleContent.gameObject.AddComponent<ResponsiveSquareGrid>();
        responsive.Configure(5, 125f);
        EditorUtility.SetDirty(grid);
        EditorUtility.SetDirty(responsive);
    }

    private static void ConfigureUnitEquipmentViews()
    {
        UnitEquipmentPanel panel = Object.FindFirstObjectByType<UnitEquipmentPanel>(FindObjectsInactive.Include);
        Transform changeRoot = FindAll("EquipmentChangeOverlay").FirstOrDefault();
        Transform enhancementRoot = FindAll("EquipmentEnhancementOverlay").FirstOrDefault();
        if (panel == null || changeRoot == null || enhancementRoot == null)
            return;

        EquipmentChangeOverlayView changeView = changeRoot.GetComponent<EquipmentChangeOverlayView>()
            ?? changeRoot.gameObject.AddComponent<EquipmentChangeOverlayView>();
        changeView.Configure(
            Find(changeRoot, "Preview")?.GetComponent<EquipmentSlotView>(),
            Find(changeRoot, "PreviewInfo")?.GetComponent<TMP_Text>(),
            Find(Find(changeRoot, "EquipmentScroll"), "Content"),
            Find(changeRoot, "Template")?.GetComponent<EquipmentSlotView>(),
            Find(changeRoot, "Equip")?.GetComponent<Button>(),
            Find(changeRoot, "CloseButton")?.GetComponent<Button>());

        EquipmentEnhancementOverlayView enhancementView = enhancementRoot.GetComponent<EquipmentEnhancementOverlayView>()
            ?? enhancementRoot.gameObject.AddComponent<EquipmentEnhancementOverlayView>();
        Slider slider = Find(enhancementRoot, "ExpSlider")?.GetComponent<Slider>();
        enhancementView.Configure(
            Find(enhancementRoot, "Slot")?.GetComponent<EquipmentSlotView>(),
            Find(enhancementRoot, "Level")?.GetComponent<TMP_Text>(),
            Find(enhancementRoot, "NextLevel")?.GetComponent<TMP_Text>(),
            Find(enhancementRoot, "Effect")?.GetComponent<TMP_Text>(),
            slider,
            slider?.fillRect?.GetComponent<Image>(),
            Find(enhancementRoot, "Exp")?.GetComponent<TMP_Text>(),
            Find(Find(enhancementRoot, "Materials"), "Content"),
            Find(enhancementRoot, "Template")?.GetComponent<TrainingMaterialSlot>(),
            Find(enhancementRoot, "Reset")?.GetComponent<Button>(),
            Find(enhancementRoot, "Auto")?.GetComponent<Button>(),
            Find(enhancementRoot, "Confirm")?.GetComponent<Button>(),
            Find(enhancementRoot, "CloseButton")?.GetComponent<Button>());

        panel.ConfigureChange(changeView);
        panel.ConfigureEnhancement(enhancementView);
        EditorUtility.SetDirty(changeView);
        EditorUtility.SetDirty(enhancementView);
        EditorUtility.SetDirty(panel);
    }

    private static void NormalizeItemDetailOverlay()
    {
        ItemDetailPanelView view = Object.FindFirstObjectByType<ItemDetailPanelView>(FindObjectsInactive.Include);
        if (view == null) return;

        Transform overlay = view.transform;
        overlay.name = "ItemDetailOverlay";
        Transform backdrop = DirectChild(overlay, "BG") ?? DirectChild(overlay, "ModalBackdrop");
        Transform card = DirectChild(overlay, "Popup") ?? DirectChild(overlay, "ModalCard");
        if (backdrop != null)
        {
            backdrop.name = "ModalBackdrop";
            Button duplicateButton = backdrop.GetComponent<Button>();
            if (duplicateButton != null) Object.DestroyImmediate(duplicateButton);
            Image image = backdrop.GetComponent<Image>();
            if (image != null) image.raycastTarget = true;
        }
        if (card != null) card.name = "ModalCard";

        Image overlayImage = overlay.GetComponent<Image>();
        if (overlayImage != null) overlayImage.raycastTarget = true;
        Button overlayButton = overlay.GetComponent<Button>();
        if (overlayButton != null) Object.DestroyImmediate(overlayButton);
        ModalBackdropDismiss dismiss = overlay.GetComponent<ModalBackdropDismiss>() ?? overlay.gameObject.AddComponent<ModalBackdropDismiss>();
        dismiss.Configure(card as RectTransform, overlay.gameObject, view);
        EditorUtility.SetDirty(view);
        EditorUtility.SetDirty(dismiss);
    }

    private static void ConsolidateEquipmentDetailOverlay()
    {
        Transform shared = Object.FindObjectsByType<EquipmentDetailOverlayView>(FindObjectsInactive.Include, FindObjectsSortMode.None)
            .Select(x => x.transform)
            .FirstOrDefault(x => x.parent != null && x.parent.GetComponent<Canvas>() != null);
        if (shared == null) shared = FindAll("EquippedDetailOverlay").FirstOrDefault();
        if (shared == null) shared = FindAll("EquipmentDetailOverlay").FirstOrDefault();
        if (shared == null) return;

        Canvas canvas = shared.GetComponentInParent<Canvas>(true);
        if (canvas != null && shared.parent != canvas.transform)
            shared.SetParent(canvas.transform, false);
        shared.name = "EquipmentDetailOverlay";
        Stretch(shared as RectTransform);

        Transform card = DirectChild(shared, "Card") ?? DirectChild(shared, "ModalCard");
        if (card == null) return;
        card.name = "ModalCard";

        Transform header = Find(card, "Header");
        TMP_Text headerLabel = header?.GetComponentInChildren<TMP_Text>(true);
        if (headerLabel != null) { headerLabel.name = "Label"; headerLabel.text = "장비"; }

        Transform rarityTransform = Find(card, "RarityHeader") ?? Find(card, "Rarity");
        if (rarityTransform != null) rarityTransform.name = "RarityHeader";
        Image rarityHeader = rarityTransform?.GetComponent<Image>();
        TMP_Text rarityText = rarityTransform?.GetComponentInChildren<TMP_Text>(true);
        if (rarityText != null) rarityText.name = "Rarity";

        Transform nameTransform = Find(card, "ItemName") ?? Find(card, "Name");
        if (nameTransform != null) nameTransform.name = "ItemName";
        TMP_Text itemName = nameTransform?.GetComponent<TMP_Text>();
        Transform slotTransform = Find(card, "EquipmentSlot") ?? Find(card, "Slot");
        if (slotTransform != null) slotTransform.name = "EquipmentSlot";
        EquipmentSlotView slot = slotTransform?.GetComponent<EquipmentSlotView>();

        Transform baseEffect = Find(card, "BaseEffect") ?? Find(card, "EffectBg");
        if (baseEffect != null) baseEffect.name = "BaseEffect";
        Image effectIcon = Find(baseEffect, "EffectIcon")?.GetComponent<Image>();
        TMP_Text effectText = Find(baseEffect, "EffectText")?.GetComponent<TMP_Text>() ?? Find(baseEffect, "Effect")?.GetComponent<TMP_Text>();
        if (effectText != null) effectText.name = "EffectText";

        Transform descriptionTransform = Find(card, "Description");
        TMP_Text description = descriptionTransform?.GetComponent<TMP_Text>();
        if (descriptionTransform?.parent != null && descriptionTransform.parent != card && descriptionTransform.parent.name == "GameObject")
            descriptionTransform.parent.name = "DescriptionSection";
        Transform bonus = Find(card, "BonusEffect") ?? Find(card, "Level10Bonus") ??
            FindAll("BonusEffect").FirstOrDefault(x => x.IsChildOf(shared));
        if (bonus != null) bonus.name = "BonusEffect";
        Image bonusIcon = Find(bonus, "EffectIcon")?.GetComponent<Image>();
        TMP_Text bonusText = Find(bonus, "EffectText")?.GetComponent<TMP_Text>();

        Button left = RenameButton(card, "Unequip", "LeftActionButton");
        Button primary = RenameButton(card, "Enhance", "PrimaryActionButton");
        Button right = RenameButton(card, "Replace", "RightActionButton");
        Button close = RenameButton(card, "Close", "CloseButton");

        EquipmentDetailOverlayView detailView = shared.GetComponent<EquipmentDetailOverlayView>() ?? shared.gameObject.AddComponent<EquipmentDetailOverlayView>();
        detailView.Configure(headerLabel, rarityHeader, rarityText, itemName, slot, effectIcon, effectText, description,
            bonus?.gameObject, bonusIcon, bonusText, left, primary, right, close);
        SerializedObject detailSerialized = new(detailView);
        detailSerialized.FindProperty("bonusEffectRoot").objectReferenceValue = bonus?.gameObject;
        detailSerialized.FindProperty("bonusEffectIcon").objectReferenceValue = bonusIcon;
        detailSerialized.FindProperty("bonusEffectText").objectReferenceValue = bonusText;
        detailSerialized.ApplyModifiedPropertiesWithoutUndo();
        AssignStatIcons(detailSerialized);

        EquipmentInventoryPanelView inventory = Object.FindFirstObjectByType<EquipmentInventoryPanelView>(FindObjectsInactive.Include);
        UnitEquipmentPanel unit = Object.FindFirstObjectByType<UnitEquipmentPanel>(FindObjectsInactive.Include);
        if (inventory != null) { inventory.ConfigureDetail(detailView); EditorUtility.SetDirty(inventory); }
        if (unit != null) { unit.ConfigureDetail(detailView); EditorUtility.SetDirty(unit); }

        Button duplicateRootButton = shared.GetComponent<Button>();
        if (duplicateRootButton != null) Object.DestroyImmediate(duplicateRootButton);
        Image background = shared.GetComponent<Image>();
        if (background != null) background.raycastTarget = true;
        ModalBackdropDismiss dismiss = shared.GetComponent<ModalBackdropDismiss>() ?? shared.gameObject.AddComponent<ModalBackdropDismiss>();
        dismiss.Configure(card as RectTransform, shared.gameObject, detailView);
        EditorUtility.SetDirty(detailView);
        EditorUtility.SetDirty(dismiss);

        foreach (Transform duplicate in FindAll("EquipmentDetailOverlay").Where(x => x != shared).ToArray())
            Object.DestroyImmediate(duplicate.gameObject);
        shared.gameObject.SetActive(false);
    }

    private static void NormalizeModal(string overlayName)
    {
        foreach (Transform overlay in FindAll(overlayName))
        {
            Transform card = DirectChild(overlay, "Card") ?? DirectChild(overlay, "FilterCard") ?? DirectChild(overlay, "ModalCard");
            if (card == null) continue;
            Stretch(overlay as RectTransform);
            card.name = "ModalCard";
            RenameButton(card, "Close", "CloseButton");
            Button duplicateRootButton = overlay.GetComponent<Button>();
            if (duplicateRootButton != null) Object.DestroyImmediate(duplicateRootButton);
            Image background = overlay.GetComponent<Image>();
            if (background != null) background.raycastTarget = true;
            MonoBehaviour handler = overlayName == "EquipmentDismantleOverlay" || overlayName == "EquipmentFilterOverlay"
                ? Object.FindFirstObjectByType<EquipmentInventoryPanelView>(FindObjectsInactive.Include)
                : Object.FindFirstObjectByType<UnitEquipmentPanel>(FindObjectsInactive.Include);
            ModalBackdropDismiss dismiss = overlay.GetComponent<ModalBackdropDismiss>() ?? overlay.gameObject.AddComponent<ModalBackdropDismiss>();
            dismiss.Configure(card as RectTransform, overlay.gameObject, handler);
            EditorUtility.SetDirty(dismiss);
        }
    }

    private static void NormalizeEquipmentRewardOverlay()
    {
        Transform overlay = FindAll("EquipmentRewardOverlay").FirstOrDefault();
        EquipmentInventoryPanelView handler = Object.FindFirstObjectByType<EquipmentInventoryPanelView>(FindObjectsInactive.Include);
        if (overlay == null || handler == null) return;

        Transform card = DirectChild(overlay, "ModalCard") ?? DirectChild(overlay, "RewardCard");
        NormalizeFullScreenOverlay(overlay, handler, allowBackdropDismiss: true);
        if (card != null) card.name = "ModalCard";
    }

    private static void NormalizeModalChildNames()
    {
        foreach (ModalBackdropDismiss dismiss in Object.FindObjectsByType<ModalBackdropDismiss>(
                     FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            Transform root = dismiss.transform;
            Transform backdrop = DirectChild(root, "BG");
            if (backdrop != null) backdrop.name = "ModalBackdrop";

            Transform card = DirectChild(root, "ModalCard");
            Transform title = DirectChild(card, "Title");
            if (title != null)
            {
                title.name = "Header";
                TMP_Text titleLabel = title.GetComponentInChildren<TMP_Text>(true);
                if (titleLabel != null) titleLabel.name = "Label";
            }

            foreach (Button button in root.GetComponentsInChildren<Button>(true))
            {
                foreach (Transform child in button.transform.Cast<Transform>())
                {
                    if (child.name == "Text (TMP)" && child.GetComponent<TMP_Text>() != null)
                        child.name = "Label";
                    else if (child.name == "Image" && child.GetComponent<Image>() != null)
                        child.name = "Icon";
                }
            }

            foreach (TMP_Text hint in root.GetComponentsInChildren<TMP_Text>(true)
                         .Where(x => x.transform.parent == root && x.name == "Text (TMP)"))
                hint.name = "DismissHint";
        }
    }

    private static Button RenameButton(Transform root, string oldName, string newName)
    {
        Transform value = Find(root, newName) ?? Find(root, oldName);
        if (value != null) value.name = newName;
        return value?.GetComponent<Button>();
    }

    private static void AssignStatIcons(SerializedObject serialized)
    {
        Set("attackStatIcon", "stat_attack");
        Set("criticalStatIcon", "stat_critical");
        Set("maxHealthStatIcon", "stat_max_health");
        Set("damageReductionStatIcon", "stat_damage_reduction");
        serialized.ApplyModifiedPropertiesWithoutUndo();
        void Set(string propertyName, string assetName)
        {
            SerializedProperty property = serialized.FindProperty(propertyName);
            if (property != null)
                property.objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Art/UI/Generated/Lobby/{assetName}.png");
        }
    }

    private static Transform DirectChild(Transform root, string name) => root?.Cast<Transform>().FirstOrDefault(x => x.name == name);
    private static Transform[] FindAll(string name) => Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None).Where(x => x.name == name).ToArray();
    private static Transform Find(Transform root, string name)
    {
        if (root == null) return null;
        if (root.name == name) return root;
        foreach (Transform child in root)
        {
            Transform result = Find(child, name);
            if (result != null) return result;
        }
        return null;
    }
    private static void Stretch(RectTransform rect)
    {
        if (rect == null) return;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
    private static string Path(Transform transform)
    {
        string path = transform.name;
        while (transform.parent != null)
        {
            transform = transform.parent;
            path = transform.name + "/" + path;
        }
        return path;
    }
}
