using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.SceneManagement;

public static class CombatStatisticsHudSetup
{
    private const string GameScenePath = "Assets/Scenes/GameScene.unity";

    [MenuItem("Tools/Game/Stage/Build Combat Statistics HUD")]
    public static void Build()
    {
        Build(GameScenePath);
    }

    public static void Build(string scenePath)
    {
        LocalizationProjectSetup.Setup();
        Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        GameObject canvas = FindSceneObject("Stage HUD Canvas");
        StageUIController stageUI = Object.FindFirstObjectByType<StageUIController>(FindObjectsInactive.Include);
        if (canvas == null || stageUI == null)
            throw new System.InvalidOperationException("Stage HUD Canvas or StageUIController was not found.");

        RectTransform panel = GetOrCreateRect("Combat Statistics View", canvas.transform);
        SetBottomCenter(panel, new Vector2(0f, 28f), new Vector2(960f, 390f));
        UnityEngine.UI.Image panelImage = GetOrAdd<UnityEngine.UI.Image>(panel.gameObject);
        panelImage.color = new Color(0.055f, 0.045f, 0.04f, 0.88f);
        panelImage.raycastTarget = false;

        RectTransform header = GetOrCreateRect("Header", panel);
        SetTopCenter(header, new Vector2(0f, -8f), new Vector2(920f, 42f));
        BindLocalizedText(CreateText("Rank", header, new Vector2(-431f, 0f), new Vector2(50f, 34f), "순위", 20f, TextAlignmentOptions.Center), "combat.statistics.rank");
        BindLocalizedText(CreateText("Unit", header, new Vector2(-337f, 0f), new Vector2(120f, 34f), "유닛", 20f, TextAlignmentOptions.Center), "combat.statistics.unit");
        BindLocalizedText(CreateText("Damage", header, new Vector2(-150f, 0f), new Vector2(190f, 34f), "피해량", 20f, TextAlignmentOptions.Center), "combat.statistics.damage");
        BindLocalizedText(CreateText("Healing", header, new Vector2(75f, 0f), new Vector2(190f, 34f), "회복량", 20f, TextAlignmentOptions.Center), "combat.statistics.healing");
        BindLocalizedText(CreateText("Taken", header, new Vector2(300f, 0f), new Vector2(190f, 34f), "받은 피해", 20f, TextAlignmentOptions.Center), "combat.statistics.damage_taken");

        StageCombatStatisticsRowUI[] rows = new StageCombatStatisticsRowUI[3];
        for (int i = 0; i < rows.Length; i++)
            rows[i] = BuildRow(panel, i, 1 + i);

        RectTransform footer = GetOrCreateRect("Sort Buttons", panel);
        SetBottomCenter(footer, new Vector2(0f, 12f), new Vector2(920f, 68f));
        UnityEngine.UI.Button damageButton = BuildButton("Damage Button", footer, new Vector2(-305f, 0f), "피해량", out TMP_Text damageLabel);
        UnityEngine.UI.Button healingButton = BuildButton("Healing Button", footer, Vector2.zero, "회복량", out TMP_Text healingLabel);
        UnityEngine.UI.Button takenButton = BuildButton("Taken Button", footer, new Vector2(305f, 0f), "받은 피해", out TMP_Text takenLabel);
        BindLocalizedText(damageLabel, "combat.statistics.damage");
        BindLocalizedText(healingLabel, "combat.statistics.healing");
        BindLocalizedText(takenLabel, "combat.statistics.damage_taken");

        StageCombatStatisticsUI view = GetOrAdd<StageCombatStatisticsUI>(panel.gameObject);
        SerializedObject viewObject = new(view);
        SerializedProperty rowProperty = viewObject.FindProperty("rows");
        rowProperty.arraySize = rows.Length;
        for (int i = 0; i < rows.Length; i++)
            rowProperty.GetArrayElementAtIndex(i).objectReferenceValue = rows[i];
        Assign(viewObject, "damageButton", damageButton);
        Assign(viewObject, "healingButton", healingButton);
        Assign(viewObject, "takenButton", takenButton);
        Assign(viewObject, "damageLabel", damageLabel);
        Assign(viewObject, "healingLabel", healingLabel);
        Assign(viewObject, "takenLabel", takenLabel);
        viewObject.ApplyModifiedPropertiesWithoutUndo();

        SerializedObject stageObject = new(stageUI);
        Assign(stageObject, "combatStatisticsUI", view);
        stageObject.ApplyModifiedPropertiesWithoutUndo();

        panel.gameObject.SetActive(false);
        EditorUtility.SetDirty(view);
        EditorUtility.SetDirty(stageUI);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        Debug.Log($"Combat statistics HUD was built and wired in {scenePath}.");
    }

    private static StageCombatStatisticsRowUI BuildRow(RectTransform panel, int index, int rank)
    {
        RectTransform root = GetOrCreateRect($"Rank {rank}", panel);
        SetTopCenter(root, new Vector2(0f, -55f - index * 82f), new Vector2(920f, 74f));
        UnityEngine.UI.Image background = GetOrAdd<UnityEngine.UI.Image>(root.gameObject);
        background.color = index % 2 == 0 ? new Color(0.14f, 0.11f, 0.08f, 0.96f) : new Color(0.1f, 0.08f, 0.065f, 0.96f);
        background.raycastTarget = false;

        TMP_Text rankText = CreateText("Rank", root, new Vector2(-431f, 0f), new Vector2(50f, 66f), rank.ToString(), 28f, TextAlignmentOptions.Center);
        RectTransform portraitRect = GetOrCreateRect("Portrait", root);
        SetCenter(portraitRect, new Vector2(-365f, 0f), new Vector2(60f, 60f));
        UnityEngine.UI.Image portrait = GetOrAdd<UnityEngine.UI.Image>(portraitRect.gameObject);
        portrait.color = new Color(1f, 1f, 1f, 0.12f);
        portrait.preserveAspect = true;
        portrait.raycastTarget = false;
        Transform oldStar = root.Find("Star");
        if (oldStar != null)
            oldStar.gameObject.SetActive(false);
        SetChildActive(root, "Damage", false);
        SetChildActive(root, "Healing", false);
        SetChildActive(root, "Taken", false);

        UnityEngine.UI.Slider damageSlider = BuildMeter(
            "Damage Meter", root, new Vector2(-150f, 0f),
            "Assets/Art/UI/Lobby/stat_attack.png",
            new Color(1f, 0.58f, 0.08f, 1f), out TMP_Text damageText);
        UnityEngine.UI.Slider healingSlider = BuildMeter(
            "Healing Meter", root, new Vector2(75f, 0f),
            "Assets/Art/UI/Lobby/stat_max_health.png",
            new Color(0.48f, 0.92f, 0.28f, 1f), out TMP_Text healingText);
        UnityEngine.UI.Slider takenSlider = BuildMeter(
            "Taken Meter", root, new Vector2(300f, 0f),
            "Assets/Art/UI/Lobby/stat_damage_reduction.png",
            new Color(0.9f, 0.18f, 0.48f, 1f), out TMP_Text takenText);

        StageCombatStatisticsRowUI row = GetOrAdd<StageCombatStatisticsRowUI>(root.gameObject);
        SerializedObject rowObject = new(row);
        Assign(rowObject, "rankText", rankText);
        Assign(rowObject, "portraitImage", portrait);
        Assign(rowObject, "damageText", damageText);
        Assign(rowObject, "healingText", healingText);
        Assign(rowObject, "takenText", takenText);
        Assign(rowObject, "damageSlider", damageSlider);
        Assign(rowObject, "healingSlider", healingSlider);
        Assign(rowObject, "takenSlider", takenSlider);
        rowObject.ApplyModifiedPropertiesWithoutUndo();
        return row;
    }

    private static UnityEngine.UI.Slider BuildMeter(
        string name,
        Transform parent,
        Vector2 position,
        string iconPath,
        Color fillColor,
        out TMP_Text valueText)
    {
        RectTransform root = GetOrCreateRect(name, parent);
        SetCenter(root, position, new Vector2(190f, 58f));
        UnityEngine.UI.Image background = GetOrAdd<UnityEngine.UI.Image>(root.gameObject);
        background.color = Color.clear;
        background.raycastTarget = false;

        RectTransform track = GetOrCreateRect("Track", root);
        SetCenter(track, new Vector2(0f, -18f), new Vector2(182f, 14f));
        UnityEngine.UI.Image trackImage = GetOrAdd<UnityEngine.UI.Image>(track.gameObject);
        trackImage.color = new Color(0.035f, 0.028f, 0.025f, 0.9f);
        trackImage.raycastTarget = false;
        track.SetAsFirstSibling();

        RectTransform fillArea = GetOrCreateRect("Fill Area", root);
        SetCenter(fillArea, new Vector2(0f, -18f), new Vector2(182f, 14f));
        RectTransform fill = GetOrCreateRect("Fill", fillArea);
        Stretch(fill, 1f);
        UnityEngine.UI.Image fillImage = GetOrAdd<UnityEngine.UI.Image>(fill.gameObject);
        fillImage.color = fillColor;
        fillImage.raycastTarget = false;

        UnityEngine.UI.Slider slider = GetOrAdd<UnityEngine.UI.Slider>(root.gameObject);
        slider.transition = UnityEngine.UI.Selectable.Transition.None;
        slider.interactable = false;
        slider.fillRect = fill;
        slider.handleRect = null;
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 0f;

        RectTransform iconRect = GetOrCreateRect("Icon", root);
        SetCenter(iconRect, new Vector2(-72f, 11f), new Vector2(28f, 28f));
        UnityEngine.UI.Image icon = GetOrAdd<UnityEngine.UI.Image>(iconRect.gameObject);
        icon.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(iconPath);
        icon.preserveAspect = true;
        icon.color = icon.sprite != null ? Color.white : fillColor;
        icon.raycastTarget = false;

        valueText = CreateText("Value", root, new Vector2(16f, 11f), new Vector2(130f, 28f), "0", 20f, TextAlignmentOptions.Center);
        valueText.fontStyle = FontStyles.Bold;
        return slider;
    }

    private static UnityEngine.UI.Button BuildButton(string name, Transform parent, Vector2 position, string label, out TMP_Text labelText)
    {
        RectTransform rect = GetOrCreateRect(name, parent);
        SetCenter(rect, position, new Vector2(290f, 58f));
        UnityEngine.UI.Image image = GetOrAdd<UnityEngine.UI.Image>(rect.gameObject);
        image.color = new Color(0.22f, 0.18f, 0.12f, 1f);
        image.raycastTarget = true;
        UnityEngine.UI.Button button = GetOrAdd<UnityEngine.UI.Button>(rect.gameObject);
        button.targetGraphic = image;
        labelText = CreateText("Label", rect, Vector2.zero, new Vector2(280f, 54f), label, 22f, TextAlignmentOptions.Center);
        return button;
    }

    private static TMP_Text CreateText(string name, Transform parent, Vector2 position, Vector2 size, string value, float fontSize, TextAlignmentOptions alignment)
    {
        RectTransform rect = GetOrCreateRect(name, parent);
        SetCenter(rect, position, size);
        TextMeshProUGUI text = GetOrAdd<TextMeshProUGUI>(rect.gameObject);
        text.font = TMP_Settings.defaultFontAsset;
        text.text = value;
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = Color.white;
        text.raycastTarget = false;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        return text;
    }

    private static void BindLocalizedText(TMP_Text text, string key)
    {
        LocalizeStringEvent localizer = GetOrAdd<LocalizeStringEvent>(text.gameObject);
        localizer.StringReference = new LocalizedString(LocalizationTables.Stage, key);

        while (localizer.OnUpdateString.GetPersistentEventCount() > 0)
            UnityEventTools.RemovePersistentListener(localizer.OnUpdateString, 0);

        UnityAction<string> setText = text.SetText;
        UnityEventTools.AddPersistentListener(localizer.OnUpdateString, setText);
        int listenerIndex = localizer.OnUpdateString.GetPersistentEventCount() - 1;
        localizer.OnUpdateString.SetPersistentListenerState(listenerIndex, UnityEventCallState.EditorAndRuntime);
        EditorUtility.SetDirty(localizer);
    }

    private static void Assign(SerializedObject target, string propertyName, Object value) => target.FindProperty(propertyName).objectReferenceValue = value;
    private static void SetChildActive(Transform parent, string name, bool active)
    {
        Transform child = parent.Find(name);
        if (child != null)
            child.gameObject.SetActive(active);
    }
    private static T GetOrAdd<T>(GameObject target) where T : Component => target.GetComponent<T>() ?? target.AddComponent<T>();
    private static RectTransform GetOrCreateRect(string name, Transform parent)
    {
        Transform existing = parent.Find(name);
        if (existing != null) return existing.GetComponent<RectTransform>();
        GameObject child = new(name, typeof(RectTransform));
        RectTransform rect = child.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        return rect;
    }

    private static void SetCenter(RectTransform rect, Vector2 position, Vector2 size)
    {
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position; rect.sizeDelta = size;
    }
    private static void SetTopCenter(RectTransform rect, Vector2 position, Vector2 size)
    {
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = position; rect.sizeDelta = size;
    }
    private static void SetBottomCenter(RectTransform rect, Vector2 position, Vector2 size)
    {
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0f);
        rect.anchoredPosition = position; rect.sizeDelta = size;
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
        foreach (GameObject candidate in Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            if (candidate.name == name) return candidate;
        return null;
    }
}
