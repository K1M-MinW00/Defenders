#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class ShopSceneBuilder
{
    private const string ScenePath = "Assets/Scenes/LobbyScene.unity";
    private const string UiFolder = "Assets/Prefabs/UI/Shop";
    private const string ProductFolder = "Assets/GameData/Shops/Products";
    private const string GroupPath = "Assets/GameData/Shops/ShopItems.asset";

    [MenuItem("Tools/Defenders/Build Shop UI")]
    public static void Build()
    {
        EnsureFolder("Assets/Prefabs/UI", "Shop");
        EnsureFolder("Assets/GameData/Shops", "Products");

        ShopRewardSlotView rewardSlot = BuildRewardSlot();
        GameObject limitedPrefab = BuildCard("LimitedShopCard", ShopCardStyle.Limited, rewardSlot);
        GameObject packagePrefab = BuildCard("PackageShopCard", ShopCardStyle.Package, rewardSlot);
        GameObject rechargePrefab = BuildCard("RechargeShopCard", ShopCardStyle.Recharge, rewardSlot);
        GameObject exchangePrefab = BuildCard("ExchangeShopCard", ShopCardStyle.Exchange, rewardSlot);
        ShopProductGroup group = BuildProducts();

        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        GameObject panel = FindInScene(scene, "Shop_Panel");
        if (panel == null)
            throw new InvalidOperationException("Shop_Panel was not found.");

        foreach (LobbyTabView obsolete in panel.GetComponents<LobbyTabView>())
            UnityEngine.Object.DestroyImmediate(obsolete);
        ClearChildren(panel.transform);

        LobbyShopPanelView view = panel.GetComponent<LobbyShopPanelView>() ?? panel.AddComponent<LobbyShopPanelView>();
        view.enabled = true;
        Image background = panel.GetComponent<Image>() ?? panel.AddComponent<Image>();
        background.color = new Color32(6, 43, 68, 255);
        RectTransform root = panel.GetComponent<RectTransform>();

        TMP_Text title = Text(root, "Title", "상점", 50, TextAlignmentOptions.Center);
        Rect(title.rectTransform, new Vector2(.05f, .92f), new Vector2(.48f, .99f));
        TMP_Text gem = Text(root, "GemBalance", "◆ 0", 32, TextAlignmentOptions.Center);
        Rect(gem.rectTransform, new Vector2(.7f, .93f), new Vector2(.96f, .985f));
        TMP_Text feedback = Text(root, "Feedback", "", 22, TextAlignmentOptions.Center);
        feedback.color = new Color32(255, 231, 120, 255);
        Rect(feedback.rectTransform, new Vector2(.05f, .865f), new Vector2(.95f, .92f));

        RectTransform contentRoot = RectObject(root, "Contents");
        Rect(contentRoot, new Vector2(.025f, .13f), new Vector2(.975f, .86f));
        RectTransform tabBar = RectObject(root, "Tabs");
        Rect(tabBar, new Vector2(.025f, .02f), new Vector2(.975f, .115f));
        HorizontalLayoutGroup tabLayout = tabBar.gameObject.AddComponent<HorizontalLayoutGroup>();
        tabLayout.spacing = 10;
        tabLayout.childControlWidth = tabLayout.childControlHeight = true;
        tabLayout.childForceExpandWidth = true;

        ShopTabType[] types = { ShopTabType.Limited, ShopTabType.Package, ShopTabType.Recharge, ShopTabType.Exchange };
        string[] labels = { "한정", "패키지", "충전", "교환소" };
        SerializedObject so = new(view);
        Set(so, "shopProductGroup", group);
        Set(so, "limitedItemPrefab", limitedPrefab);
        Set(so, "packageItemPrefab", packagePrefab);
        Set(so, "rechargeItemPrefab", rechargePrefab);
        Set(so, "exchangeItemPrefab", exchangePrefab);
        Set(so, "gemText", gem);
        Set(so, "feedbackText", feedback);
        ShopPurchaseConfirmPopup purchasePopup = BuildPurchasePopup(root, rewardSlot);
        Set(so, "purchaseConfirmPopup", purchasePopup);
        ShopPurchaseResultPopup resultPopup = BuildResultPopup(root, rewardSlot);
        Set(so, "purchaseResultPopup", resultPopup);
        GameObject mainShopButton = FindInScene(scene, "Shop_Btn");
        GameObject mainBadge = mainShopButton != null
            ? NotificationBadge(mainShopButton.transform, "ShopNotificationBadge")
            : null;
        if (mainShopButton != null)
        {
            ShopMainNotificationBadgeView mainBadgeView =
                mainShopButton.GetComponent<ShopMainNotificationBadgeView>() ??
                mainShopButton.AddComponent<ShopMainNotificationBadgeView>();
            SerializedObject mainBadgeSerialized = new(mainBadgeView);
            Set(mainBadgeSerialized, "shopProductGroup", group);
            Set(mainBadgeSerialized, "badge", mainBadge);
            mainBadgeSerialized.ApplyModifiedPropertiesWithoutUndo();
        }
        SerializedProperty tabs = so.FindProperty("tabs");
        tabs.arraySize = 4;

        for (int i = 0; i < types.Length; i++)
        {
            Button button = Button(tabBar, types[i] + "Tab", labels[i], new Color32(29, 145, 157, 255));
            GameObject selected = ImageObject(button.transform, "Selected", new Color32(255, 194, 48, 255)).gameObject;
            Rect((RectTransform)selected.transform, Vector2.zero, new Vector2(1, .08f));
            GameObject badge = NotificationBadge(button.transform, "NotificationBadge");

            GameObject tabPanel;
            Transform content;
            if (types[i] == ShopTabType.Package)
                tabPanel = PackagePanel(contentRoot, so, out content);
            else
                tabPanel = GridScroll(contentRoot, types[i] + "Panel", labels[i], Columns(types[i]), out content);

            SerializedProperty entry = tabs.GetArrayElementAtIndex(i);
            entry.FindPropertyRelative("Type").enumValueIndex = (int)types[i];
            entry.FindPropertyRelative("Button").objectReferenceValue = button;
            entry.FindPropertyRelative("Selected").objectReferenceValue = selected;
            entry.FindPropertyRelative("Badge").objectReferenceValue = badge;
            entry.FindPropertyRelative("Panel").objectReferenceValue = tabPanel;
            entry.FindPropertyRelative("Content").objectReferenceValue = content;
        }

        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(view);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[ShopSceneBuilder] Reference-style shop UI rebuilt.");
    }

    private static ShopPurchaseConfirmPopup BuildPurchasePopup(
        Transform parent,
        ShopRewardSlotView rewardSlot)
    {
        GameObject overlay = ImageObject(parent, "PurchaseConfirmPopup", new Color32(0, 12, 28, 220)).gameObject;
        Rect((RectTransform)overlay.transform, Vector2.zero, Vector2.one);

        GameObject card = ImageObject(overlay.transform, "Card", new Color32(35, 91, 139, 255)).gameObject;
        Rect((RectTransform)card.transform, new Vector2(.08f, .25f), new Vector2(.92f, .75f));
        Outline outline = card.AddComponent<Outline>();
        outline.effectColor = new Color32(8, 30, 55, 255);
        outline.effectDistance = new Vector2(5, -5);

        TMP_Text title = Text(card.transform, "Title", "상품 구매", 38, TextAlignmentOptions.Center);
        Rect(title.rectTransform, new Vector2(.08f, .80f), new Vector2(.92f, .96f));

        RectTransform rewards = RectObject(card.transform, "Rewards");
        Rect(rewards, new Vector2(.08f, .53f), new Vector2(.92f, .78f));
        HorizontalLayoutGroup rewardLayout = rewards.gameObject.AddComponent<HorizontalLayoutGroup>();
        rewardLayout.spacing = 12;
        rewardLayout.childAlignment = TextAnchor.MiddleCenter;
        rewardLayout.childControlWidth = rewardLayout.childControlHeight = false;

        TMP_Text currentGem = Text(card.transform, "CurrentGem", "보유  ◆ 0", 25, TextAlignmentOptions.Center);
        Rect(currentGem.rectTransform, new Vector2(.08f, .40f), new Vector2(.48f, .51f));
        TMP_Text cost = Text(card.transform, "Cost", "가격  ◆ 0", 25, TextAlignmentOptions.Center);
        Rect(cost.rectTransform, new Vector2(.52f, .40f), new Vector2(.92f, .51f));
        TMP_Text remaining = Text(card.transform, "RemainingGem", "구매 후  ◆ 0", 28, TextAlignmentOptions.Center);
        remaining.color = new Color32(255, 232, 92, 255);
        Rect(remaining.rectTransform, new Vector2(.08f, .29f), new Vector2(.92f, .40f));
        TMP_Text warning = Text(card.transform, "Warning", "이 상품을 구매하시겠습니까?", 23, TextAlignmentOptions.Center);
        Rect(warning.rectTransform, new Vector2(.08f, .20f), new Vector2(.92f, .29f));

        Button cancel = Button(card.transform, "Cancel", "취소", new Color32(92, 109, 126, 255));
        Rect((RectTransform)cancel.transform, new Vector2(.08f, .04f), new Vector2(.47f, .18f));
        Button confirm = Button(card.transform, "Confirm", "구매", new Color32(45, 190, 73, 255));
        Rect((RectTransform)confirm.transform, new Vector2(.53f, .04f), new Vector2(.92f, .18f));
        Button recharge = Button(card.transform, "Recharge", "Gem 충전", new Color32(214, 75, 112, 255));
        Rect((RectTransform)recharge.transform, new Vector2(.53f, .04f), new Vector2(.92f, .18f));

        ShopPurchaseConfirmPopup popup = overlay.AddComponent<ShopPurchaseConfirmPopup>();
        SerializedObject serialized = new(popup);
        Set(serialized, "titleText", title);
        Set(serialized, "rewardContainer", rewards);
        Set(serialized, "rewardSlotPrefab", rewardSlot);
        Set(serialized, "currentGemText", currentGem);
        Set(serialized, "costText", cost);
        Set(serialized, "remainingGemText", remaining);
        Set(serialized, "warningText", warning);
        Set(serialized, "confirmButton", confirm);
        Set(serialized, "cancelButton", cancel);
        Set(serialized, "rechargeButton", recharge);
        serialized.ApplyModifiedPropertiesWithoutUndo();

        overlay.SetActive(false);
        return popup;
    }

    private static GameObject NotificationBadge(Transform parent, string name)
    {
        Transform existing = parent.Find(name);
        if (existing != null)
            UnityEngine.Object.DestroyImmediate(existing.gameObject);

        GameObject badge = ImageObject(parent, name, new Color32(244, 57, 70, 255)).gameObject;
        Rect((RectTransform)badge.transform, new Vector2(.76f, .64f), new Vector2(1.03f, 1.03f));
        Outline outline = badge.AddComponent<Outline>();
        outline.effectColor = new Color32(108, 15, 24, 255);
        outline.effectDistance = new Vector2(2, -2);
        TMP_Text label = Text(badge.transform, "Label", "!", 24, TextAlignmentOptions.Center);
        Rect(label.rectTransform, Vector2.zero, Vector2.one);
        badge.SetActive(false);
        return badge;
    }

    private static ShopPurchaseResultPopup BuildResultPopup(
        Transform parent,
        ShopRewardSlotView rewardSlot)
    {
        GameObject overlay = ImageObject(parent, "PurchaseResultPopup", new Color32(0, 12, 28, 220)).gameObject;
        Rect((RectTransform)overlay.transform, Vector2.zero, Vector2.one);

        GameObject card = ImageObject(overlay.transform, "Card", new Color32(44, 106, 157, 255)).gameObject;
        Rect((RectTransform)card.transform, new Vector2(.10f, .28f), new Vector2(.90f, .72f));
        Outline outline = card.AddComponent<Outline>();
        outline.effectColor = new Color32(8, 30, 55, 255);
        outline.effectDistance = new Vector2(5, -5);

        TMP_Text resultTitle = Text(card.transform, "ResultTitle", "구매 완료!", 43, TextAlignmentOptions.Center);
        resultTitle.color = new Color32(255, 229, 76, 255);
        Rect(resultTitle.rectTransform, new Vector2(.08f, .79f), new Vector2(.92f, .96f));

        TMP_Text productName = Text(card.transform, "ProductName", "상품명", 29, TextAlignmentOptions.Center);
        Rect(productName.rectTransform, new Vector2(.08f, .67f), new Vector2(.92f, .79f));

        RectTransform rewards = RectObject(card.transform, "Rewards");
        Rect(rewards, new Vector2(.08f, .34f), new Vector2(.92f, .66f));
        HorizontalLayoutGroup rewardLayout = rewards.gameObject.AddComponent<HorizontalLayoutGroup>();
        rewardLayout.spacing = 12;
        rewardLayout.childAlignment = TextAnchor.MiddleCenter;
        rewardLayout.childControlWidth = rewardLayout.childControlHeight = false;

        TMP_Text message = Text(card.transform, "Message", "구매가 완료되었습니다.", 23, TextAlignmentOptions.Center);
        Rect(message.rectTransform, new Vector2(.08f, .20f), new Vector2(.92f, .34f));

        Button close = Button(card.transform, "Close", "확인", new Color32(45, 190, 73, 255));
        Rect((RectTransform)close.transform, new Vector2(.25f, .04f), new Vector2(.75f, .18f));

        ShopPurchaseResultPopup popup = overlay.AddComponent<ShopPurchaseResultPopup>();
        SerializedObject serialized = new(popup);
        Set(serialized, "resultTitleText", resultTitle);
        Set(serialized, "productNameText", productName);
        Set(serialized, "messageText", message);
        Set(serialized, "rewardContainer", rewards);
        Set(serialized, "rewardSlotPrefab", rewardSlot);
        Set(serialized, "closeButton", close);
        serialized.ApplyModifiedPropertiesWithoutUndo();

        overlay.SetActive(false);
        return popup;
    }

    private static GameObject PackagePanel(Transform parent, SerializedObject so, out Transform rootContent)
    {
        GameObject panel = ScrollBase(parent, "PackagePanel", out RectTransform content);
        VerticalLayoutGroup vertical = content.gameObject.AddComponent<VerticalLayoutGroup>();
        vertical.spacing = 18;
        vertical.padding = new RectOffset(12, 12, 12, 12);
        vertical.childControlWidth = true;
        vertical.childForceExpandWidth = true;
        vertical.childControlHeight = true;
        vertical.childForceExpandHeight = false;
        ContentSizeFitter fitter = content.gameObject.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        Transform daily = PackageSection(content, "일간 패키지");
        Transform weekly = PackageSection(content, "주간 패키지");
        Transform monthly = PackageSection(content, "월간 패키지");
        Set(so, "dailyPackageContent", daily);
        Set(so, "weeklyPackageContent", weekly);
        Set(so, "monthlyPackageContent", monthly);
        rootContent = content;
        return panel;
    }

    private static Transform PackageSection(Transform parent, string label)
    {
        GameObject section = new(label, typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter), typeof(LayoutElement));
        section.transform.SetParent(parent, false);
        VerticalLayoutGroup vertical = section.GetComponent<VerticalLayoutGroup>();
        vertical.spacing = 10;
        vertical.childControlWidth = true;
        vertical.childForceExpandWidth = true;
        vertical.childControlHeight = true;
        vertical.childForceExpandHeight = false;
        section.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        section.GetComponent<LayoutElement>().minHeight = 480;

        GameObject header = SectionHeader(section.transform, label);
        header.transform.SetAsFirstSibling();

        RectTransform grid = RectObject(section.transform, "Grid");
        GridLayoutGroup layout = grid.gameObject.AddComponent<GridLayoutGroup>();
        layout.cellSize = new Vector2(400, 340);
        layout.spacing = new Vector2(20, 15);
        layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        layout.constraintCount = 2;
        layout.childAlignment = TextAnchor.UpperCenter;
        ShopResponsiveGrid responsive = grid.gameObject.AddComponent<ShopResponsiveGrid>();
        responsive.Configure(2, 400f / 340f, 20f, 4f);
        ContentSizeFitter fit = grid.gameObject.AddComponent<ContentSizeFitter>();
        fit.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        return grid;
    }

    private static GameObject GridScroll(Transform parent, string name, string headerLabel, int columns, out Transform content)
    {
        GameObject panel = ScrollBase(parent, name, out RectTransform rootContent);
        VerticalLayoutGroup vertical = rootContent.gameObject.AddComponent<VerticalLayoutGroup>();
        vertical.spacing = 18;
        vertical.padding = new RectOffset(12, 12, 12, 12);
        vertical.childControlWidth = true;
        vertical.childForceExpandWidth = true;
        vertical.childControlHeight = true;
        vertical.childForceExpandHeight = false;
        rootContent.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        SectionHeader(rootContent, headerLabel);

        RectTransform grid = RectObject(rootContent, "Grid");
        GridLayoutGroup layout = grid.gameObject.AddComponent<GridLayoutGroup>();
        layout.cellSize = columns switch
        {
            1 => new Vector2(840, 430),
            3 => new Vector2(255, 340),
            _ => new Vector2(400, 340),
        };
        layout.spacing = new Vector2(20, 20);
        layout.padding = new RectOffset(4, 4, 4, 16);
        layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        layout.constraintCount = columns;
        layout.childAlignment = TextAnchor.UpperCenter;
        float aspectRatio = columns == 1 ? 840f / 430f : columns == 3 ? 255f / 340f : 400f / 340f;
        ShopResponsiveGrid responsive = grid.gameObject.AddComponent<ShopResponsiveGrid>();
        responsive.Configure(columns, aspectRatio, 20f, 4f);
        grid.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        content = grid;
        return panel;
    }

    private static GameObject SectionHeader(Transform parent, string label)
    {
        GameObject header = ImageObject(parent, "SectionHeader", new Color32(65, 143, 219, 255)).gameObject;
        header.AddComponent<LayoutElement>().preferredHeight = 100;
        Outline outline = header.AddComponent<Outline>();
        outline.effectColor = new Color32(9, 32, 59, 255);
        outline.effectDistance = new Vector2(0, -6);
        Image topLine = ImageObject(header.transform, "TopLine", new Color32(116, 190, 255, 255));
        Rect(topLine.rectTransform, new Vector2(.015f, .86f), new Vector2(.985f, .94f));
        TMP_Text headerText = Text(header.transform, "Label", label, 40, TextAlignmentOptions.Center);
        Rect(headerText.rectTransform, Vector2.zero, Vector2.one);
        return header;
    }
    private static GameObject ScrollBase(Transform parent, string name, out RectTransform content)
    {
        GameObject panel = new(name, typeof(RectTransform), typeof(Image), typeof(ScrollRect));
        panel.transform.SetParent(parent, false);
        Rect((RectTransform)panel.transform, Vector2.zero, Vector2.one);
        panel.GetComponent<Image>().color = new Color32(5, 34, 55, 255);
        RectTransform viewport = RectObject(panel.transform, "Viewport");
        Rect(viewport, Vector2.zero, Vector2.one, new Vector2(10, 10), new Vector2(-10, -10));
        viewport.gameObject.AddComponent<RectMask2D>();
        content = RectObject(viewport, "Content");
        content.anchorMin = new Vector2(0, 1);
        content.anchorMax = new Vector2(1, 1);
        content.pivot = new Vector2(.5f, 1);
        content.sizeDelta = Vector2.zero;
        ScrollRect scroll = panel.GetComponent<ScrollRect>();
        scroll.viewport = viewport;
        scroll.content = content;
        scroll.horizontal = false;
        return panel;
    }

    private static ShopRewardSlotView BuildRewardSlot()
    {
        string path = UiFolder + "/ShopRewardSlot.prefab";
        GameObject root = new("RewardSlot", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
        root.GetComponent<RectTransform>().sizeDelta = new Vector2(92, 92);
        root.GetComponent<Image>().color = new Color32(255, 225, 113, 255);
        Outline slotOutline = root.AddComponent<Outline>();
        slotOutline.effectColor = new Color32(94, 55, 22, 255);
        slotOutline.effectDistance = new Vector2(3, -3);
        root.GetComponent<LayoutElement>().preferredWidth = root.GetComponent<LayoutElement>().preferredHeight = 92;
        Image icon = ImageObject(root.transform, "Icon", new Color32(255, 247, 204, 255));
        Rect(icon.rectTransform, new Vector2(.12f, .18f), new Vector2(.88f, .9f));
        TMP_Text type = Text(root.transform, "Type", "ITEM", 15, TextAlignmentOptions.Center);
        type.color = new Color32(77, 49, 24, 255);
        Rect(type.rectTransform, new Vector2(.08f, .42f), new Vector2(.92f, .8f));
        TMP_Text amount = Text(root.transform, "Amount", "0", 21, TextAlignmentOptions.BottomRight);
        Rect(amount.rectTransform, Vector2.zero, Vector2.one);
        ShopRewardSlotView view = root.AddComponent<ShopRewardSlotView>();
        SerializedObject so = new(view);
        Set(so, "frameImage", root.GetComponent<Image>());
        Set(so, "iconImage", icon);
        Set(so, "typeText", type);
        Set(so, "amountText", amount);
        so.ApplyModifiedPropertiesWithoutUndo();
        PrefabUtility.SaveAsPrefabAsset(root, path);
        UnityEngine.Object.DestroyImmediate(root);
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        return AssetDatabase.LoadAssetAtPath<GameObject>(path).GetComponent<ShopRewardSlotView>();
    }

    private enum ShopCardStyle { Limited, Package, Recharge, Exchange }

    private static GameObject BuildCard(string name, ShopCardStyle style, ShopRewardSlotView slot)
    {
        bool limited = style == ShopCardStyle.Limited;
        bool recharge = style == ShopCardStyle.Recharge;
        bool exchange = style == ShopCardStyle.Exchange;
        Vector2 size = limited ? new Vector2(840, 430) : recharge ? new Vector2(255, 340) : new Vector2(400, 340);
        Color color = limited ? new Color32(238, 130, 166, 255) :
            recharge ? new Color32(247, 157, 177, 255) :
            exchange ? new Color32(63, 162, 220, 255) :
            new Color32(255, 215, 166, 255);

        string path = UiFolder + "/" + name + ".prefab";
        GameObject root = new(name, typeof(RectTransform), typeof(Image), typeof(LayoutElement));
        root.GetComponent<RectTransform>().sizeDelta = size;
        root.GetComponent<Image>().color = color;
        LayoutElement layoutElement = root.GetComponent<LayoutElement>();
        layoutElement.preferredWidth = size.x;
        layoutElement.preferredHeight = size.y;
        Outline cardOutline = root.AddComponent<Outline>();
        cardOutline.effectColor = limited ? new Color32(95, 33, 58, 255) :
            exchange ? new Color32(9, 55, 99, 255) : new Color32(101, 58, 30, 255);
        cardOutline.effectDistance = new Vector2(4, -4);

        TMP_Text title = Text(root.transform, "Name", "상품명", limited ? 34 : 27, TextAlignmentOptions.Center);
        Rect(title.rectTransform, new Vector2(.05f, limited ? .79f : .77f), new Vector2(.95f, .98f));

        Image portraitImage = null;
        if (limited)
        {
            portraitImage = ImageObject(root.transform, "Portrait", new Color32(255, 255, 255, 35));
            Rect(portraitImage.rectTransform, new Vector2(.03f, .22f), new Vector2(.43f, .78f));
        }

        Image icon = null;
        if (!limited)
        {
            icon = ImageObject(root.transform, "ProductImage", new Color32(255, 255, 255, 35));
            Rect(icon.rectTransform,
                recharge ? new Vector2(.2f, .25f) : new Vector2(.3f, .4f),
                recharge ? new Vector2(.8f, .7f) : new Vector2(.7f, .7f));
        }

        RectTransform rewards = null;
        if (!recharge)
        {
            rewards = RectObject(root.transform, "RewardSlots");
            Rect(rewards, limited ? new Vector2(.47f, .28f) : new Vector2(.08f, .24f),
                limited ? new Vector2(.95f, .49f) : new Vector2(.92f, .54f));
            HorizontalLayoutGroup rewardLayout = rewards.gameObject.AddComponent<HorizontalLayoutGroup>();
            rewardLayout.spacing = 8;
            rewardLayout.childAlignment = TextAnchor.MiddleCenter;
            rewardLayout.childControlWidth = rewardLayout.childControlHeight = false;
        }

        TMP_Text description = null;
        if (limited)
        {
            description = Text(root.transform, "Description", "", 20, TextAlignmentOptions.Center);
            Rect(description.rectTransform, new Vector2(.04f, .04f), new Vector2(.45f, .22f));
        }

        TMP_Text remain = Text(root.transform, "Remain", "", 18, TextAlignmentOptions.Center);
        Rect(remain.rectTransform, limited ? new Vector2(.49f, .15f) : new Vector2(.04f, .16f),
            limited ? new Vector2(.95f, .23f) : new Vector2(.96f, .23f));
        TMP_Text timer = Text(root.transform, "Timer", "", 17, TextAlignmentOptions.Center);
        Rect(timer.rectTransform, new Vector2(.04f, .70f), new Vector2(.96f, .78f));

        Button buy = Button(root.transform, "Buy", "가격", new Color32(39, 189, 52, 255));
        if (recharge)
        {
            buy.GetComponent<Image>().color = new Color32(190, 43, 105, 255);
            Rect((RectTransform)buy.transform, Vector2.zero, new Vector2(1, .20f));
        }
        else if (exchange)
        {
            buy.GetComponent<Image>().color = new Color32(28, 91, 191, 255);
            Rect((RectTransform)buy.transform, Vector2.zero, new Vector2(1, .22f));
        }
        else if (limited)
        {
            Rect((RectTransform)buy.transform, new Vector2(.51f, .02f), new Vector2(.92f, .15f));
        }
        else
        {
            buy.GetComponent<Image>().color = new Color32(120, 82, 43, 255);
            Rect((RectTransform)buy.transform, Vector2.zero, new Vector2(1, .20f));
        }

        TMP_Text status = Text(root.transform, "Status", "", 22, TextAlignmentOptions.Center);
        status.color = new Color32(255, 245, 80, 255);
        Rect(status.rectTransform, new Vector2(.62f, .68f), new Vector2(.98f, .8f));

        GameObject bonusPanel = null;
        TMP_Text bonusText = null;
        if (recharge)
        {
            bonusPanel = ImageObject(root.transform, "BonusPanel", new Color32(255, 208, 0, 255)).gameObject;
            Rect((RectTransform)bonusPanel.transform, new Vector2(.02f, .79f), new Vector2(.98f, .95f));
            bonusText = Text(bonusPanel.transform, "BonusText", "첫 구매 2배", 20, TextAlignmentOptions.Center);
            Rect(bonusText.rectTransform, Vector2.zero, Vector2.one);
        }

        ShopProductView view = style switch
        {
            ShopCardStyle.Limited => root.AddComponent<LimitedProductView>(),
            ShopCardStyle.Package => root.AddComponent<PackageProductView>(),
            ShopCardStyle.Recharge => root.AddComponent<RechargeProductView>(),
            ShopCardStyle.Exchange => root.AddComponent<ExchangeProductView>(),
            _ => throw new ArgumentOutOfRangeException(nameof(style), style, null),
        };

        SerializedObject serialized = new(view);
        Set(serialized, "titleText", title);
        Set(serialized, "costText", buy.GetComponentInChildren<TMP_Text>());
        Set(serialized, "remainText", remain);
        Set(serialized, "timerText", timer);
        Set(serialized, "statusText", status);
        Set(serialized, "purchaseButton", buy);

        switch (style)
        {
            case ShopCardStyle.Limited:
                Set(serialized, "portraitImage", portraitImage);
                Set(serialized, "descriptionText", description);
                Set(serialized, "rewardContainer", rewards);
                Set(serialized, "rewardSlotPrefab", slot);
                break;
            case ShopCardStyle.Package:
                Set(serialized, "packageImage", icon);
                Set(serialized, "rewardContainer", rewards);
                Set(serialized, "rewardSlotPrefab", slot);
                break;
            case ShopCardStyle.Recharge:
                Set(serialized, "gemImage", icon);
                Set(serialized, "bonusPanel", bonusPanel);
                Set(serialized, "bonusText", bonusText);
                break;
            case ShopCardStyle.Exchange:
                Set(serialized, "productImage", icon);
                Set(serialized, "rewardContainer", rewards);
                Set(serialized, "rewardSlotPrefab", slot);
                break;
        }

        serialized.ApplyModifiedPropertiesWithoutUndo();

        PrefabUtility.SaveAsPrefabAsset(root, path);
        UnityEngine.Object.DestroyImmediate(root);
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        return AssetDatabase.LoadAssetAtPath<GameObject>(path);
    }
    private static ShopProductGroup BuildProducts()
    {
        ShopProductGroup group = AssetDatabase.LoadAssetAtPath<ShopProductGroup>(GroupPath);
        group.LimitedProducts = new() { Product("limited_preview", "연료 소모 이벤트 지원 패키지", ShopTabType.Limited, ShopPurchaseType.InAppPurchase, 25000, ShopResetPeriod.None, 1, RewardType.Gem, 2100, "") };
        group.DailyPackages = new() { Product("daily_preview", "일일 성장 팩", ShopTabType.Package, ShopPurchaseType.InAppPurchase, 1300, ShopResetPeriod.Daily, 1, RewardType.Gold, 5000, "") };
        group.WeeklyPackages = new() { Product("weekly_preview", "주간 연료 팩", ShopTabType.Package, ShopPurchaseType.InAppPurchase, 6500, ShopResetPeriod.Weekly, 1, RewardType.Fuel, 700, "") };
        group.MonthlyPackages = new() { Product("monthly_preview", "월간 성장 팩", ShopTabType.Package, ShopPurchaseType.InAppPurchase, 13000, ShopResetPeriod.Monthly, 1, RewardType.Gem, 500, "") };
        group.RechargeProducts = new()
        {
            Product("gem_180", "180 Gem", ShopTabType.Recharge, ShopPurchaseType.InAppPurchase, 1300, ShopResetPeriod.None, 0, RewardType.Gem, 180, "첫 구매 2배"),
            Product("gem_635", "635 Gem", ShopTabType.Recharge, ShopPurchaseType.InAppPurchase, 3900, ShopResetPeriod.None, 0, RewardType.Gem, 635, "첫 구매 2배"),
            Product("gem_1325", "1,325 Gem", ShopTabType.Recharge, ShopPurchaseType.InAppPurchase, 6500, ShopResetPeriod.None, 0, RewardType.Gem, 1325, "첫 구매 2배"),
        };
        group.ExchangeProducts = new()
        {
            Product("exchange_gold_4500", "크레딧", ShopTabType.Exchange, ShopPurchaseType.Gem, 10, ShopResetPeriod.None, 0, RewardType.Gold, 4500, ""),
            Product("exchange_fuel_50", "연료", ShopTabType.Exchange, ShopPurchaseType.Gem, 100, ShopResetPeriod.None, 0, RewardType.Fuel, 50, ""),
        };
        EditorUtility.SetDirty(group);
        return group;
    }

    private static ShopProductData Product(string id, string name, ShopTabType tab, ShopPurchaseType type, int price, ShopResetPeriod reset, int limit, RewardType reward, int amount, string bonus)
    {
        string path = ProductFolder + "/" + id + ".asset";
        ShopProductData p = AssetDatabase.LoadAssetAtPath<ShopProductData>(path);
        Type expectedType = tab switch
        {
            ShopTabType.Limited => typeof(LimitedShopProductData),
            ShopTabType.Package => typeof(PackageShopProductData),
            ShopTabType.Recharge => typeof(RechargeShopProductData),
            ShopTabType.Exchange => typeof(ExchangeShopProductData),
            _ => typeof(ShopProductData),
        };
        if (p != null && p.GetType() != expectedType)
        {
            AssetDatabase.DeleteAsset(path);
            p = null;
        }
        if (p == null)
        {
            p = (ShopProductData)ScriptableObject.CreateInstance(expectedType);
            AssetDatabase.CreateAsset(p, path);
        }

        p.ProductId = id; p.DisplayName = name; p.Description = ""; p.Tab = tab; p.PurchaseType = type;
        p.Price = price; p.CostAmount = type == ShopPurchaseType.Gem ? price : 0;
        p.ResetPeriod = reset; p.PurchaseLimit = limit; p.IsEnabled = true;
        p.Rewards = new() { new RewardData { Type = reward, Amount = amount } };
        if (p is LimitedShopProductData limited)
            limited.Introduction = "기간 한정 특별 구성 상품입니다.";
        if (p is RechargeShopProductData recharge)
        {
            recharge.HasFirstPurchaseBonus = !string.IsNullOrWhiteSpace(bonus);
            recharge.FirstPurchaseBonusLabel = bonus;
        }
        EditorUtility.SetDirty(p);
        return p;
    }

    private static int Columns(ShopTabType type) => type == ShopTabType.Limited ? 1 : type == ShopTabType.Recharge ? 3 : 2;
    private static void Set(SerializedObject so, string name, UnityEngine.Object value) => so.FindProperty(name).objectReferenceValue = value;
    private static Button Button(Transform parent, string name, string label, Color color) { GameObject go = new(name, typeof(RectTransform), typeof(Image), typeof(Button)); go.transform.SetParent(parent, false); go.GetComponent<Image>().color = color; TMP_Text text = Text(go.transform, "Label", label, 27, TextAlignmentOptions.Center); Rect(text.rectTransform, Vector2.zero, Vector2.one); return go.GetComponent<Button>(); }
    private static TMP_Text Text(Transform parent, string name, string value, float size, TextAlignmentOptions align) { GameObject go = new(name, typeof(RectTransform), typeof(TextMeshProUGUI)); go.transform.SetParent(parent, false); TMP_Text t = go.GetComponent<TMP_Text>(); t.text = value; t.fontSize = size; t.alignment = align; t.color = Color.white; t.textWrappingMode = TextWrappingModes.Normal; return t; }
    private static Image ImageObject(Transform parent, string name, Color color) { GameObject go = new(name, typeof(RectTransform), typeof(Image)); go.transform.SetParent(parent, false); go.GetComponent<Image>().color = color; return go.GetComponent<Image>(); }
    private static RectTransform RectObject(Transform parent, string name) { GameObject go = new(name, typeof(RectTransform)); go.transform.SetParent(parent, false); return (RectTransform)go.transform; }
    private static void Rect(RectTransform rect, Vector2 min, Vector2 max, Vector2? offMin = null, Vector2? offMax = null) { rect.anchorMin = min; rect.anchorMax = max; rect.offsetMin = offMin ?? Vector2.zero; rect.offsetMax = offMax ?? Vector2.zero; }
    private static void ClearChildren(Transform parent) { for (int i = parent.childCount - 1; i >= 0; i--) UnityEngine.Object.DestroyImmediate(parent.GetChild(i).gameObject); }
    private static GameObject FindInScene(Scene scene, string name) => scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<Transform>(true)).FirstOrDefault(t => t.name == name)?.gameObject;
    private static void EnsureFolder(string parent, string name) { string path = parent + "/" + name; if (!AssetDatabase.IsValidFolder(path)) AssetDatabase.CreateFolder(parent, name); }
}
#endif





