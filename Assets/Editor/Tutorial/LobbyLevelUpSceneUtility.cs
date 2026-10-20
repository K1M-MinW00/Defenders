using TMPro;
using UnityEditor;
using UnityEngine;

public static class LobbyLevelUpSceneUtility
{
    public static LobbyLevelUpView GetOrCreate(TutorialOverlayView overlay)
    {
        LobbyLevelUpView view = overlay.GetComponent<LobbyLevelUpView>();
        if (view == null)
            view = overlay.gameObject.AddComponent<LobbyLevelUpView>();

        Transform existing = overlay.transform.Find("Level Up Overlay");
        if (existing != null)
            Object.DestroyImmediate(existing.gameObject);

        RectTransform root = CreateRect("Level Up Overlay", overlay.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        UnityEngine.UI.Image backdrop = root.gameObject.AddComponent<UnityEngine.UI.Image>();
        backdrop.color = new Color(0.015f, 0.025f, 0.055f, 0.88f);
        backdrop.raycastTarget = true;
        LobbyLevelUpPointerRelay pointerRelay = root.gameObject.AddComponent<LobbyLevelUpPointerRelay>();

        RectTransform content = CreateRect("Content", root, Vector2.one * 0.5f, Vector2.one * 0.5f, new Vector2(760f, 1050f), Vector2.zero);
        TMP_Text titleText = CreateText("Title", content, 82f, new Color(1f, 0.87f, 0.12f), new Vector2(0f, 405f), new Vector2(620f, 120f), FontStyles.Bold);

        RectTransform portrait = CreateRect("Portrait", content, Vector2.one * 0.5f, Vector2.one * 0.5f, new Vector2(185f, 185f), new Vector2(0f, 105f));
        UnityEngine.UI.Image portraitFrame = portrait.gameObject.AddComponent<UnityEngine.UI.Image>();
        portraitFrame.color = new Color(0.82f, 0.91f, 1f, 1f);
        portraitFrame.raycastTarget = false;
        RectTransform portraitIcon = CreateRect("Icon", portrait, Vector2.one * 0.5f, Vector2.one * 0.5f, new Vector2(155f, 155f), Vector2.zero);
        UnityEngine.UI.Image portraitImage = portraitIcon.gameObject.AddComponent<UnityEngine.UI.Image>();
        portraitImage.preserveAspect = true;
        portraitImage.raycastTarget = false;

        RectTransform previousBadge = CreateLevelBadge("Previous Level", content, out TMP_Text previousLevelText, Vector2.zero);
        RectTransform arrow = CreateRect("Arrow", content, Vector2.one * 0.5f, Vector2.one * 0.5f, new Vector2(90f, 100f), new Vector2(0f, -15f));
        UnityEngine.UI.Image arrowImage = arrow.gameObject.AddComponent<UnityEngine.UI.Image>();
        arrowImage.sprite = LoadArrowSprite();
        arrowImage.color = Color.white;
        arrowImage.preserveAspect = true;
        arrowImage.raycastTarget = false;
        arrow.localRotation = Quaternion.Euler(0f, 180f, 0f);
        RectTransform currentBadge = CreateLevelBadge("Current Level", content, out TMP_Text currentLevelText, new Vector2(105f, -15f));

        RectTransform fuel = CreateRect("Fuel Panel", content, Vector2.one * 0.5f, Vector2.one * 0.5f, new Vector2(650f, 150f), new Vector2(0f, -230f));
        CanvasGroup fuelPanel = fuel.gameObject.AddComponent<CanvasGroup>();
        UnityEngine.UI.Image fuelBackground = fuel.gameObject.AddComponent<UnityEngine.UI.Image>();
        fuelBackground.color = new Color(0.09f, 0.12f, 0.17f, 0.96f);
        fuelBackground.raycastTarget = false;
        TMP_Text fuelText = CreateText("Fuel Text", fuel, 38f, Color.white, Vector2.zero, new Vector2(620f, 130f), FontStyles.Bold);

        RectTransform research = CreateRect("Research Notice", content, Vector2.one * 0.5f, Vector2.one * 0.5f, new Vector2(720f, 105f), new Vector2(0f, -390f));
        CanvasGroup researchNotice = research.gameObject.AddComponent<CanvasGroup>();
        RectTransform labIcon = CreateRect("Lab Icon", research, Vector2.one * 0.5f, Vector2.one * 0.5f, new Vector2(70f, 70f), new Vector2(-285f, 0f));
        UnityEngine.UI.Image researchIcon = labIcon.gameObject.AddComponent<UnityEngine.UI.Image>();
        researchIcon.preserveAspect = true;
        researchIcon.raycastTarget = false;
        TMP_Text researchText = CreateText("Research Text", research, 34f, new Color(0.25f, 1f, 0.48f), new Vector2(45f, 0f), new Vector2(570f, 90f), FontStyles.Bold);

        TMP_Text dismissHintText = CreateText("Dismiss Hint", root, 30f, new Color(1f, 1f, 1f, 0.75f), new Vector2(0f, 95f), new Vector2(650f, 80f), FontStyles.Normal, new Vector2(0.5f, 0f));

        SerializedObject serialized = new(view);
        Assign(serialized, "root", root);
        Assign(serialized, "content", content);
        Assign(serialized, "portrait", portrait);
        Assign(serialized, "portraitImage", portraitImage);
        Assign(serialized, "previousLevelBadge", previousBadge);
        Assign(serialized, "previousLevelText", previousLevelText);
        Assign(serialized, "arrow", arrow);
        Assign(serialized, "currentLevelBadge", currentBadge);
        Assign(serialized, "currentLevelText", currentLevelText);
        Assign(serialized, "fuelPanel", fuelPanel);
        Assign(serialized, "fuelText", fuelText);
        Assign(serialized, "researchNotice", researchNotice);
        Assign(serialized, "researchIcon", researchIcon);
        Assign(serialized, "titleText", titleText);
        Assign(serialized, "researchText", researchText);
        Assign(serialized, "dismissHintText", dismissHintText);
        Assign(serialized, "pointerRelay", pointerRelay);
        serialized.ApplyModifiedPropertiesWithoutUndo();

        root.gameObject.SetActive(false);
        return view;
    }

    private static Sprite LoadArrowSprite()
    {
        Object[] sprites = AssetDatabase.LoadAllAssetsAtPath("Assets/Art/ThirdParty/RPG/Images/UnitTab_Icons.png");
        foreach (Object asset in sprites)
        {
            if (asset is Sprite candidate && candidate.name == "UnitTab_Icons_0")
                return candidate;
        }

        return null;
    }

    private static RectTransform CreateLevelBadge(string name, Transform parent, out TMP_Text label, Vector2 position)
    {
        RectTransform badge = CreateRect(name, parent, Vector2.one * 0.5f, Vector2.one * 0.5f, new Vector2(150f, 150f), position);
        UnityEngine.UI.Image image = badge.gameObject.AddComponent<UnityEngine.UI.Image>();
        image.color = new Color(0.35f, 0.72f, 0.91f, 1f);
        image.raycastTarget = false;
        label = CreateText("Level", badge, 58f, Color.white, Vector2.zero, new Vector2(130f, 120f), FontStyles.Bold);
        return badge;
    }

    private static RectTransform CreateRect(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 size, Vector2 position)
    {
        GameObject instance = new(name, typeof(RectTransform));
        RectTransform rect = instance.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.sizeDelta = size;
        rect.anchoredPosition = position;
        return rect;
    }

    private static TMP_Text CreateText(string name, Transform parent, float size, Color color, Vector2 position,
        Vector2 dimensions, FontStyles style, Vector2? anchor = null)
    {
        Vector2 textAnchor = anchor ?? Vector2.one * 0.5f;
        RectTransform rect = CreateRect(name, parent, textAnchor, textAnchor, dimensions, position);
        TextMeshProUGUI text = rect.gameObject.AddComponent<TextMeshProUGUI>();
        text.fontSize = size;
        text.color = color;
        text.fontStyle = style;
        text.alignment = TextAlignmentOptions.Center;
        text.raycastTarget = false;
        return text;
    }

    private static void Assign(SerializedObject serialized, string propertyName, Object value) =>
        serialized.FindProperty(propertyName).objectReferenceValue = value;
}
