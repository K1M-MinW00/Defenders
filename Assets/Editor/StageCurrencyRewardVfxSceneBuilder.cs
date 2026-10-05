#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class StageCurrencyRewardVfxSceneBuilder
{
    private const string ScenePath = "Assets/Scenes/GameScene.unity";
    private const string CurrencySpritePath = "Assets/Art/UI/Generated/currency_gem.png";
    private const int PooledIconCount = 10;

    [MenuItem("Tools/Defenders/Build Stage Currency Reward VFX")]
    public static void Build()
    {
        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        GameObject stageRoot = GameObject.Find("Stage UI Root");
        GameObject currencyView = GameObject.Find("Currency View");
        StageRewardService rewardService = Object.FindFirstObjectByType<StageRewardService>();

        if (stageRoot == null || currencyView == null || rewardService == null)
            throw new System.InvalidOperationException("Stage currency reward VFX dependencies are missing.");

        RectTransform currencyTarget = currencyView.transform.Find("Gold_Img") as RectTransform;
        TMP_Text goldText = currencyView.GetComponentInChildren<TMP_Text>(true);
        if (currencyTarget == null || goldText == null)
            throw new System.InvalidOperationException("Currency View target or font reference is missing.");

        RectTransform root = GetOrCreateRect(stageRoot.transform, "UI_StageCurrencyRewardVFX");
        StretchToParent(root);
        root.gameObject.layer = LayerMask.NameToLayer("UI");
        root.SetAsLastSibling();
        ConfigureCanvas(root.gameObject);

        StageCurrencyRewardVFX view = GetOrAdd<StageCurrencyRewardVFX>(root.gameObject);
        Sprite currencySprite = AssetDatabase.LoadAssetAtPath<Sprite>(CurrencySpritePath);
        Image[] icons = new Image[PooledIconCount];
        for (int i = 0; i < icons.Length; i++)
            icons[i] = GetOrCreateIcon(root, i + 1, currencySprite);

        TMP_Text gainText = GetOrCreateGainText(currencyView.transform, goldText);
        CanvasGroup gainTextGroup = GetOrAdd<CanvasGroup>(gainText.gameObject);
        gainTextGroup.interactable = false;
        gainTextGroup.blocksRaycasts = false;
        gainText.gameObject.SetActive(false);

        Transform legacyGainText = root.Find("GainAmountText");
        if (legacyGainText != null && legacyGainText != gainText.transform)
            Object.DestroyImmediate(legacyGainText.gameObject);

        SerializedObject serializedView = new(view);
        serializedView.FindProperty("effectsRoot").objectReferenceValue = root;
        serializedView.FindProperty("currencyTarget").objectReferenceValue = currencyTarget;
        serializedView.FindProperty("gainText").objectReferenceValue = gainText;
        serializedView.FindProperty("gainTextGroup").objectReferenceValue = gainTextGroup;
        serializedView.FindProperty("waveRewardTextColor").colorValue = Color.white;
        serializedView.FindProperty("bonusRewardTextColor").colorValue =
            new Color(1f, 0.82f, 0.18f, 1f);

        SerializedProperty iconArray = serializedView.FindProperty("currencyIcons");
        iconArray.arraySize = icons.Length;
        for (int i = 0; i < icons.Length; i++)
            iconArray.GetArrayElementAtIndex(i).objectReferenceValue = icons[i];
        serializedView.ApplyModifiedPropertiesWithoutUndo();

        SerializedObject serializedReward = new(rewardService);
        serializedReward.FindProperty("currencyRewardVfx").objectReferenceValue = view;
        serializedReward.ApplyModifiedPropertiesWithoutUndo();

        EditorUtility.SetDirty(view);
        EditorUtility.SetDirty(rewardService);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[StageCurrencyRewardVfxSceneBuilder] Currency reward VFX added to GameScene.");
    }

    private static Image GetOrCreateIcon(Transform parent, int number, Sprite sprite)
    {
        RectTransform rect = GetOrCreateRect(parent, $"RewardCurrency_{number:00}");
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(74f, 74f);

        Image image = GetOrAdd<Image>(rect.gameObject);
        image.sprite = sprite;
        image.preserveAspect = true;
        image.raycastTarget = false;
        image.gameObject.SetActive(false);
        return image;
    }

    private static TMP_Text GetOrCreateGainText(Transform parent, TMP_Text fontSource)
    {
        RectTransform rect = GetOrCreateRect(parent, "WaveRewardGainText");
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 0f);
        rect.anchoredPosition = new Vector2(0f, 18f);
        rect.sizeDelta = new Vector2(280f, 70f);

        LayoutElement layout = GetOrAdd<LayoutElement>(rect.gameObject);
        layout.ignoreLayout = true;

        TextMeshProUGUI text = GetOrAdd<TextMeshProUGUI>(rect.gameObject);
        text.font = fontSource.font;
        text.fontSharedMaterial = fontSource.fontSharedMaterial;
        text.fontSize = 38f;
        text.fontStyle = FontStyles.Bold;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.white;
        text.raycastTarget = false;
        text.text = "+ 10";
        return text;
    }

    private static RectTransform GetOrCreateRect(Transform parent, string objectName)
    {
        Transform existing = parent.Find(objectName);
        if (existing is RectTransform existingRect)
            return existingRect;

        GameObject child = new(objectName, typeof(RectTransform));
        child.layer = parent.gameObject.layer;
        RectTransform rect = child.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        return rect;
    }

    private static void StretchToParent(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = Vector2.zero;
        rect.localScale = Vector3.one;
    }

    private static void ConfigureCanvas(GameObject target)
    {
        Canvas canvas = GetOrAdd<Canvas>(target);
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting = true;
        canvas.sortingOrder = 12;

        CanvasScaler scaler = GetOrAdd<CanvasScaler>(target);
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080f, 1920f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
    }

    private static T GetOrAdd<T>(GameObject target) where T : Component
    {
        return target.TryGetComponent(out T component) ? component : target.AddComponent<T>();
    }
}
#endif
