using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class BossHudSetup
{
    private const string GameScenePath = "Assets/Scenes/GameScene.unity";

    [MenuItem("Tools/Game/Bosses/Build Boss HUD")]
    public static void Build()
    {
        Scene scene = EditorSceneManager.OpenScene(GameScenePath, OpenSceneMode.Single);
        GameObject canvas = FindSceneObject("Stage HUD Canvas");
        StageUIController stageUI = Object.FindFirstObjectByType<StageUIController>(FindObjectsInactive.Include);
        if (canvas == null || stageUI == null)
            throw new System.InvalidOperationException("Stage HUD Canvas or StageUIController was not found.");

        RectTransform container = GetOrCreateRect("BossBattleHUD", canvas.transform);
        Stretch(container);

        RectTransform infoRoot = GetOrCreateRect("BossInfoRoot", container);
        SetTopCenter(infoRoot, new Vector2(0f, -18f), new Vector2(640f, 82f));
        UnityEngine.UI.Image infoBackground = GetOrAdd<UnityEngine.UI.Image>(infoRoot.gameObject);
        infoBackground.color = new Color(0.035f, 0.035f, 0.055f, 0.78f);
        infoBackground.raycastTarget = false;

        TMP_Text nameText = CreateText("BossName", infoRoot, new Vector2(0f, 23f), new Vector2(450f, 28f), 22f);
        nameText.text = "BOSS";
        nameText.fontStyle = FontStyles.Bold;

        RectTransform enrageIcon = GetOrCreateRect("EnrageIcon", infoRoot);
        SetCenter(enrageIcon, new Vector2(267f, 23f), new Vector2(72f, 26f));
        UnityEngine.UI.Image enrageImage = GetOrAdd<UnityEngine.UI.Image>(enrageIcon.gameObject);
        StatusEffectIconSetSO iconSet = StatusEffectIconSetSO.Load();
        enrageImage.sprite = iconSet != null ? iconSet.berserk : null;
        enrageImage.preserveAspect = true;
        enrageImage.color = enrageImage.sprite != null
            ? Color.white
            : new Color(0.8f, 0.08f, 0.06f, 1f);
        enrageImage.raycastTarget = false;
        TMP_Text enrageText = CreateText("Label", enrageIcon, Vector2.zero, new Vector2(72f, 26f), 15f);
        enrageText.text = "광폭";
        enrageText.fontStyle = FontStyles.Bold;
        enrageText.gameObject.SetActive(enrageImage.sprite == null);

        UnityEngine.UI.Slider hpSlider = CreateSlider(
            "BossHP", infoRoot, new Vector2(0f, -5f), new Vector2(580f, 22f),
            new Color(0.12f, 0.04f, 0.04f, 1f), new Color(0.85f, 0.12f, 0.1f, 1f));
        UnityEngine.UI.Slider shieldSlider = CreateSlider(
            "BossShield", infoRoot, new Vector2(0f, -24f), new Vector2(580f, 8f),
            new Color(0.03f, 0.08f, 0.12f, 1f), new Color(0.15f, 0.65f, 1f, 1f));

        BossHudView hudView = GetOrAdd<BossHudView>(infoRoot.gameObject);
        SerializedObject hudObject = new(hudView);
        hudObject.FindProperty("root").objectReferenceValue = infoRoot.gameObject;
        hudObject.FindProperty("nameText").objectReferenceValue = nameText;
        hudObject.FindProperty("hpSlider").objectReferenceValue = hpSlider;
        hudObject.FindProperty("shieldSlider").objectReferenceValue = shieldSlider;
        hudObject.FindProperty("enrageIcon").objectReferenceValue = enrageIcon.gameObject;
        hudObject.ApplyModifiedPropertiesWithoutUndo();

        RectTransform timerRoot = GetOrCreateRect("BossTimerRoot", container);
        SetTopLeft(timerRoot, new Vector2(40f, -440f), new Vector2(190f, 64f));
        UnityEngine.UI.Image timerBackground = GetOrAdd<UnityEngine.UI.Image>(timerRoot.gameObject);
        timerBackground.color = new Color(0.035f, 0.035f, 0.055f, 0.92f);
        timerBackground.raycastTarget = false;
        CanvasGroup timerGroup = GetOrAdd<CanvasGroup>(timerRoot.gameObject);
        TMP_Text timerText = CreateText("TimerText", timerRoot, Vector2.zero, new Vector2(180f, 60f), 34f);
        timerText.text = "01:30";
        timerText.fontStyle = FontStyles.Bold;

        BossBattleTimerView timerView = GetOrAdd<BossBattleTimerView>(timerRoot.gameObject);
        SerializedObject timerObject = new(timerView);
        timerObject.FindProperty("root").objectReferenceValue = timerRoot.gameObject;
        timerObject.FindProperty("timerText").objectReferenceValue = timerText;
        timerObject.FindProperty("blinkGroup").objectReferenceValue = timerGroup;
        timerObject.ApplyModifiedPropertiesWithoutUndo();

        SerializedObject stageUIObject = new(stageUI);
        stageUIObject.FindProperty("bossBattleTimerView").objectReferenceValue = timerView;
        stageUIObject.FindProperty("bossHudView").objectReferenceValue = hudView;
        stageUIObject.ApplyModifiedPropertiesWithoutUndo();

        enrageIcon.gameObject.SetActive(false);
        shieldSlider.gameObject.SetActive(false);
        infoRoot.gameObject.SetActive(false);
        timerRoot.gameObject.SetActive(false);

        EditorUtility.SetDirty(stageUI);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        Debug.Log("Boss HUD was built and wired in GameScene.");
    }

    private static UnityEngine.UI.Slider CreateSlider(
        string name,
        Transform parent,
        Vector2 position,
        Vector2 size,
        Color backgroundColor,
        Color fillColor)
    {
        RectTransform root = GetOrCreateRect(name, parent);
        SetCenter(root, position, size);
        UnityEngine.UI.Image background = GetOrAdd<UnityEngine.UI.Image>(root.gameObject);
        background.color = backgroundColor;
        background.raycastTarget = false;

        RectTransform fillArea = GetOrCreateRect("FillArea", root);
        Stretch(fillArea, 4f);
        RectTransform fill = GetOrCreateRect("Fill", fillArea);
        Stretch(fill);
        UnityEngine.UI.Image fillImage = GetOrAdd<UnityEngine.UI.Image>(fill.gameObject);
        fillImage.color = fillColor;
        fillImage.raycastTarget = false;

        UnityEngine.UI.Slider slider = GetOrAdd<UnityEngine.UI.Slider>(root.gameObject);
        slider.transition = UnityEngine.UI.Selectable.Transition.None;
        slider.interactable = false;
        slider.fillRect = fill;
        slider.handleRect = null;
        slider.direction = UnityEngine.UI.Slider.Direction.LeftToRight;
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 1f;
        return slider;
    }

    private static TMP_Text CreateText(
        string name,
        Transform parent,
        Vector2 position,
        Vector2 size,
        float fontSize)
    {
        RectTransform rect = GetOrCreateRect(name, parent);
        SetCenter(rect, position, size);
        TextMeshProUGUI text = GetOrAdd<TextMeshProUGUI>(rect.gameObject);
        text.font = TMP_Settings.defaultFontAsset;
        text.fontSize = fontSize;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.white;
        text.raycastTarget = false;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        return text;
    }

    private static RectTransform GetOrCreateRect(string name, Transform parent)
    {
        Transform existing = parent.Find(name);
        if (existing != null)
            return existing.GetComponent<RectTransform>();

        GameObject child = new(name, typeof(RectTransform));
        RectTransform rect = child.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        return rect;
    }

    private static T GetOrAdd<T>(GameObject target) where T : Component
    {
        T component = target.GetComponent<T>();
        return component != null ? component : target.AddComponent<T>();
    }

    private static void SetTopCenter(RectTransform rect, Vector2 position, Vector2 size)
    {
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
    }

    private static void SetCenter(RectTransform rect, Vector2 position, Vector2 size)
    {
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
    }

    private static void SetTopLeft(RectTransform rect, Vector2 position, Vector2 size)
    {
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
    }

    private static void Stretch(RectTransform rect, float inset = 0f)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.offsetMin = new Vector2(inset, inset);
        rect.offsetMax = new Vector2(-inset, -inset);
    }

    private static GameObject FindSceneObject(string name)
    {
        foreach (GameObject candidate in Object.FindObjectsByType<GameObject>(
                     FindObjectsInactive.Include,
                     FindObjectsSortMode.None))
        {
            if (candidate.name == name)
                return candidate;
        }

        return null;
    }
}
