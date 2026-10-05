using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class StageEconomyHudSceneBuilder
{
    private const string ScenePath = "Assets/Scenes/GameScene.unity";
    private const string GeneratedUiPath = "Assets/Art/UI/Generated";

    [MenuItem("Tools/Stage/Build Economy HUD")]
    public static void Build()
    {
        ConfigureGeneratedSprites();

        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        StageHudPresenter hud = Object.FindFirstObjectByType<StageHudPresenter>();
        StageTopControlUI topControl = Object.FindFirstObjectByType<StageTopControlUI>();
        StageTimeController timeController = Object.FindFirstObjectByType<StageTimeController>();
        StageWaveTrackUI waveTrack = Object.FindFirstObjectByType<StageWaveTrackUI>();
        GameObject currencyView = GameObject.Find("Currency View");
        GameObject bonusCurrencyView = GameObject.Find("BonusCurrency View");
        GameObject speedButton = GameObject.Find("Speed_Btn");
        GameObject increaseButton = GameObject.Find("Increase Button");

        if (hud == null || topControl == null || timeController == null ||
            currencyView == null || bonusCurrencyView == null ||
            speedButton == null || increaseButton == null)
        {
            Debug.LogError("[StageEconomyHudSceneBuilder] Required GameScene objects are missing.");
            return;
        }

        TextMeshProUGUI goldText = currencyView.transform.Find("Gold_Text")?.GetComponent<TextMeshProUGUI>() ??
                                   currencyView.transform.Find("GoldAmountGroup/Gold_Text")?.GetComponent<TextMeshProUGUI>();
        if (goldText == null)
        {
            Debug.LogError("[StageEconomyHudSceneBuilder] Gold_Text is missing.");
            return;
        }

        RectTransform amountGroup = GetOrCreateRect(currencyView.transform, "GoldAmountGroup");
        amountGroup.SetSiblingIndex(1);
        amountGroup.sizeDelta = new Vector2(130f, 90f);

        VerticalLayoutGroup layout = GetOrAdd<VerticalLayoutGroup>(amountGroup.gameObject);
        layout.padding = new RectOffset(0, 0, 5, 5);
        layout.spacing = 0f;
        layout.childAlignment = TextAnchor.MiddleLeft;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        goldText.rectTransform.SetParent(amountGroup, false);
        ConfigureLayout(goldText.gameObject, 50f);
        goldText.alignment = TextAlignmentOptions.MidlineLeft;
        goldText.raycastTarget = false;

        StageInterestIndicator interestIndicator = bonusCurrencyView.GetComponent<StageInterestIndicator>();
        if (interestIndicator == null)
        {
            Debug.LogError("[StageEconomyHudSceneBuilder] BonusCurrency View has no StageInterestIndicator.");
            return;
        }

        interestIndicator.SetInterest(0);

        Transform oldSpeedText = speedButton.transform.Find("Speed_Text");
        if (oldSpeedText != null)
            Object.DestroyImmediate(oldSpeedText.gameObject);

        Image speedIcon = speedButton.transform.Find("Image")?.GetComponent<Image>();
        if (speedIcon == null)
        {
            Debug.LogError("[StageEconomyHudSceneBuilder] Speed button icon is missing.");
            return;
        }

        speedIcon.gameObject.SetActive(true);
        SetReference(hud, "interestIndicator", interestIndicator);
        SetReference(hud, "increasePricePanel", increaseButton.transform.Find("Price_Panel")?.gameObject);
        SetReference(topControl, "speedIcon", speedIcon);

        Sprite normalSpeedIcon = AssetDatabase.LoadAssetAtPath<Sprite>($"{GeneratedUiPath}/speed_normal.png");
        Sprite fastSpeedIcon = AssetDatabase.LoadAssetAtPath<Sprite>($"{GeneratedUiPath}/speed_fast.png");
        SerializedObject topControlSerialized = new(topControl);
        topControlSerialized.FindProperty("normalSpeedSprite").objectReferenceValue = normalSpeedIcon;
        topControlSerialized.FindProperty("fastSpeedSprite").objectReferenceValue = fastSpeedIcon;
        topControlSerialized.ApplyModifiedPropertiesWithoutUndo();
        speedIcon.sprite = normalSpeedIcon;

        if (waveTrack != null)
        {
            SetReference(waveTrack, "normalWaveSprite",
                AssetDatabase.LoadAssetAtPath<Sprite>($"{GeneratedUiPath}/wave_normal.png"));
            SetReference(waveTrack, "eliteWaveSprite",
                AssetDatabase.LoadAssetAtPath<Sprite>($"{GeneratedUiPath}/wave_elite.png"));
            SetReference(waveTrack, "bossWaveSprite",
                AssetDatabase.LoadAssetAtPath<Sprite>($"{GeneratedUiPath}/wave_boss.png"));
            EditorUtility.SetDirty(waveTrack);
        }

        SerializedObject timeSerialized = new(timeController);
        timeSerialized.FindProperty("fastSpeed").floatValue = 1.5f;
        timeSerialized.ApplyModifiedPropertiesWithoutUndo();

        EditorUtility.SetDirty(hud);
        EditorUtility.SetDirty(topControl);
        EditorUtility.SetDirty(timeController);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[StageEconomyHudSceneBuilder] Economy and speed HUD updated.");
    }

    private static RectTransform GetOrCreateRect(Transform parent, string name)
    {
        Transform existing = parent.Find(name);
        if (existing != null)
            return existing.GetComponent<RectTransform>();

        GameObject child = new(name, typeof(RectTransform));
        child.layer = parent.gameObject.layer;
        RectTransform rect = child.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        return rect;
    }

    private static Image GetOrCreateInterestSlot(Transform parent, int number)
    {
        string name = $"InterestSlot_{number}";
        Transform existing = parent.Find(name);
        Image image = existing != null ? existing.GetComponent<Image>() : null;
        if (image == null)
        {
            GameObject child = new(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            child.layer = parent.gameObject.layer;
            child.transform.SetParent(parent, false);
            image = child.GetComponent<Image>();
        }

        image.rectTransform.sizeDelta = new Vector2(18f, 18f);
        image.preserveAspect = true;
        image.raycastTarget = false;
        return image;
    }

    private static void ConfigureLayout(GameObject target, float preferredHeight)
    {
        LayoutElement element = GetOrAdd<LayoutElement>(target);
        element.minHeight = preferredHeight;
        element.preferredHeight = preferredHeight;
        element.flexibleHeight = 0f;
    }

    private static T GetOrAdd<T>(GameObject target) where T : Component
    {
        return target.TryGetComponent(out T component) ? component : target.AddComponent<T>();
    }

    private static void SetReference(Object target, string propertyName, Object value)
    {
        SerializedObject serialized = new(target);
        serialized.FindProperty(propertyName).objectReferenceValue = value;
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetImageArray(Object target, string propertyName, Image[] images)
    {
        SerializedObject serialized = new(target);
        SerializedProperty array = serialized.FindProperty(propertyName);
        array.arraySize = images.Length;
        for (int i = 0; i < images.Length; i++)
            array.GetArrayElementAtIndex(i).objectReferenceValue = images[i];
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void ConfigureGeneratedSprites()
    {
        string[] icons =
        {
            "speed_normal", "speed_fast", "currency_gem", "currency_gem_empty",
            "wave_normal", "wave_elite", "wave_boss",
            "rank_1", "rank_2", "rank_3", "rank_4",
            "icon_population", "icon_reroll", "icon_sell", "icon_pause", "icon_monster",
            "result_clear_background", "result_fail_background", "coming_soon_background"
        };

        foreach (string icon in icons)
            ConfigureSprite($"{GeneratedUiPath}/{icon}.png", Vector4.zero);

        Vector4 panelBorder = new(60f, 50f, 60f, 50f);
        ConfigureSprite($"{GeneratedUiPath}/hp_panel_ally_9slice.png", panelBorder);
        ConfigureSprite($"{GeneratedUiPath}/hp_panel_enemy_9slice.png", panelBorder);
        ConfigureSprite($"{GeneratedUiPath}/panel_common_9slice.png", panelBorder);
    }

    private static void ConfigureSprite(string path, Vector4 border)
    {
        if (AssetImporter.GetAtPath(path) is not TextureImporter importer)
            return;

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = false;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.spritePixelsPerUnit = 100f;
        importer.spriteBorder = border;
        importer.SaveAndReimport();
    }
}
