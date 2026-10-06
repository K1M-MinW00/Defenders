#if UNITY_EDITOR
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class LabSceneBuilder
{
    private const string ScenePath = "Assets/Scenes/LobbyScene.unity";
    private const string ConfigPath = "Assets/Resources/GameData/Configs/LabConfig.asset";
    private const string CardFolder = "Assets/Resources/GameData/LabCards";
    private const string PrefabFolder = "Assets/Prefabs/UI/Lab";

    [InitializeOnLoadMethod]
    private static void BuildOnceAfterImport()
    {
        EditorApplication.delayCall += () =>
        {
            if (!EditorApplication.isCompiling)
            {
                try
                {
                    SyncGeneratedAssets();
                    if (!IsSceneWired())
                        Build();
                }
                catch (System.Exception exception)
                {
                    Debug.LogError($"[LabSceneBuilder] Automatic build failed: {exception}");
                }
            }
        };
    }

    private static void SyncGeneratedAssets()
    {
        EnsureFolder(CardFolder);
        EnsureFolder(PrefabFolder);
        CreateConfig();
        CreateCardPrefab("LabCard", 180f, 240f);
        CreateCardPrefab("LabOfferCard", 220f, 300f);
        AssetDatabase.SaveAssets();
    }

    [MenuItem("Tools/Defenders/Build Lab System")]
    public static void Build()
    {
        EnsureFolder(CardFolder);
        EnsureFolder(PrefabFolder);
        LabConfigSO config = CreateConfig();
        LabCardView prefab = CreateCardPrefab("LabCard", 180f, 240f);
        LabCardView offerPrefab = CreateCardPrefab("LabOfferCard", 220f, 300f);

        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        GameObject lab = FindSceneObject(scene, "Lab_Panel");
        if (lab == null) throw new System.InvalidOperationException("Lab_Panel was not found.");

        LabPanelView view = GetOrAdd<LabPanelView>(lab);
        Transform upperBar = Find(lab.transform, "Upper_Bar");
        Transform goldPanel = upperBar != null ? Find(upperBar, "Gold_Panel") : null;
        TMP_Text currency = FindText(lab.transform, "Gold_Text") ?? goldPanel?.GetComponentInChildren<TMP_Text>(true);
        Button develop = FindComponent<Button>(lab.transform, "Upgrade_Button");
        TMP_Text price = develop != null ? FindText(develop.transform, "Price_Text") : null;
        if (currency == null) throw new System.InvalidOperationException("Lab_Panel/Upper_Bar/Gold_Panel currency text was not found.");
        if (develop == null || price == null) throw new System.InvalidOperationException("Lab Upgrade_Button or Price_Text was not found.");
        if (prefab == null || offerPrefab == null) throw new System.InvalidOperationException("Lab card prefabs could not be loaded.");
        Transform panel = Find(lab.transform, "Panel");
        if (panel == null) throw new System.InvalidOperationException("Lab_Panel/Panel was not found.");
        Transform scrollTransform = Find(panel, "Scroll View");
        ScrollRect scroll = scrollTransform?.GetComponent<ScrollRect>();
        if (scroll == null) throw new System.InvalidOperationException("Lab_Panel/Panel/Scroll View has no ScrollRect.");
        Transform content = scroll.content != null ? scroll.content : Find(scroll.transform, "Content");
        if (content == null) throw new System.InvalidOperationException("Lab Scroll View Content was not found.");
        GridLayoutGroup grid = GetOrAdd<GridLayoutGroup>(content.gameObject);
        grid.cellSize = new Vector2(180, 240); grid.spacing = new Vector2(15, 15);
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount; grid.constraintCount = 5;
        grid.childAlignment = TextAnchor.UpperCenter;
        GetOrAdd<ContentSizeFitter>(content.gameObject).verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        Transform oldLimit = Find(panel, "Limit_Text") ?? Find(panel, "LimitText");
        if (oldLimit != null) Object.DestroyImmediate(oldLimit.gameObject);
        TMP_Text limit = Text(panel, "LimitText", "", 24, TextAlignmentOptions.Center);
        Anchor(limit.rectTransform, new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, -45), new Vector2(0, 35));

        Transform mainPanel = FindAncestor(lab.transform, "Main_Panel");
        Transform overlayParent = mainPanel?.parent;
        if (overlayParent == null) throw new System.InvalidOperationException("The parent outside Main_Panel was not found.");
        DestroySceneObject(scene, "Lab_OfferOverlay");
        DestroySceneObject(scene, "Lab_DetailPopup");

        GameObject offer = Panel(overlayParent, "Lab_OfferOverlay", new Color(0.02f, 0.03f, 0.05f, 0.94f));
        Stretch((RectTransform)offer.transform, 0, 0, 0, 0);
        Transform offerContent = ChildRect(offer.transform, "Cards");
        Anchor((RectTransform)offerContent, new Vector2(.08f, .33f), new Vector2(.92f, .7f), Vector2.zero, Vector2.zero);
        HorizontalLayoutGroup row = GetOrAdd<HorizontalLayoutGroup>(offerContent.gameObject);
        row.spacing = 18; row.childAlignment = TextAnchor.MiddleCenter; row.childControlWidth = true; row.childControlHeight = true; row.childForceExpandWidth = true;
        Button offerDismiss = GetOrAdd<Button>(offer);
        offerDismiss.targetGraphic = offer.GetComponent<Image>();
        offerDismiss.transition = Selectable.Transition.None;
        offer.SetActive(false);

        GameObject detail = Panel(overlayParent, "Lab_DetailPopup", new Color(0, 0, 0, .82f));
        Stretch((RectTransform)detail.transform, 0, 0, 0, 0);
        GameObject detailCard = Panel(detail.transform, "Card", Color.white);
        LabCardGradient detailGradient = detailCard.AddComponent<LabCardGradient>();
        Anchor((RectTransform)detailCard.transform, new Vector2(.18f, .25f), new Vector2(.82f, .75f), Vector2.zero, Vector2.zero);
        Image detailIcon = ImageObject(detailCard.transform, "Icon", Color.white);
        Anchor((RectTransform)detailIcon.transform, new Vector2(.34f, .55f), new Vector2(.66f, .86f), Vector2.zero, Vector2.zero);
        TMP_Text detailName = Text(detailCard.transform, "Name", "", 40, TextAlignmentOptions.Center);
        Anchor(detailName.rectTransform, new Vector2(.05f, .82f), new Vector2(.95f, .98f), Vector2.zero, Vector2.zero);
        TMP_Text detailDescription = Text(detailCard.transform, "Description", "", 26, TextAlignmentOptions.Center);
        Anchor(detailDescription.rectTransform, new Vector2(.08f, .23f), new Vector2(.92f, .52f), Vector2.zero, Vector2.zero);
        TMP_Text detailValue = Text(detailCard.transform, "Value", "", 52, TextAlignmentOptions.Center);
        detailValue.color = new Color(.25f, 1f, .35f);
        Anchor(detailValue.rectTransform, new Vector2(.08f, .05f), new Vector2(.92f, .25f), Vector2.zero, Vector2.zero);
        Button detailDismiss = GetOrAdd<Button>(detail); detailDismiss.targetGraphic = detail.GetComponent<Image>();
        detail.SetActive(false);

        SerializedObject so = new(view);
        Set(so, "currencyText", currency); Set(so, "priceText", price); Set(so, "limitText", limit);
        Set(so, "developButton", develop); Set(so, "cardContent", content); Set(so, "cardGrid", grid); Set(so, "cardPrefab", prefab);
        Set(so, "offerOverlay", offer); Set(so, "offerContent", offerContent); Set(so, "offerCardPrefab", offerPrefab); Set(so, "offerDismissButton", offerDismiss);
        Set(so, "detailPopup", detail); Set(so, "detailBackground", detailCard.GetComponent<Image>()); Set(so, "detailGradient", detailGradient); Set(so, "detailIcon", detailIcon); Set(so, "detailName", detailName);
        Set(so, "detailDescription", detailDescription); Set(so, "detailValue", detailValue); Set(so, "detailDismissButton", detailDismiss);
        so.ApplyModifiedPropertiesWithoutUndo();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        Debug.Log($"[LabSceneBuilder] Built {config.Cards.Count} cards and wired LobbyScene.");
    }

    private static LabConfigSO CreateConfig()
    {
        LabConfigSO config = AssetDatabase.LoadAssetAtPath<LabConfigSO>(ConfigPath);
        if (config == null) { config = ScriptableObject.CreateInstance<LabConfigSO>(); AssetDatabase.CreateAsset(config, ConfigPath); }
        config.BaseDevelopmentCost = 100; config.CostGrowth = 1.5f; config.ChoiceCount = 4; config.Cards.Clear();
        Add(config, "damage_reduction", "강화 장갑", "아군 유닛이 받는 피해가 감소합니다.", LabEffectType.DamageReduction, 3, LabCardRarity.Common);
        Add(config, "max_hp", "생체 강화", "아군 유닛의 최대 체력이 증가합니다.", LabEffectType.MaxHpPercent, 8, LabCardRarity.Common);
        Add(config, "attack", "화력 증폭", "아군 유닛의 공격력이 증가합니다.", LabEffectType.AttackPercent, 7, LabCardRarity.Common);
        Add(config, "critical_chance", "정밀 조준", "아군 유닛의 치명타 확률이 증가합니다.", LabEffectType.CriticalChance, 3, LabCardRarity.Common);
        Add(config, "critical_damage", "약점 분석", "아군 유닛의 치명타 피해가 증가합니다.", LabEffectType.CriticalDamagePercent, 12, LabCardRarity.Common);
        Add(config, "starting_minerals", "초기 보급", "전투 시작 시 보유 광물이 증가합니다.", LabEffectType.StartingMinerals, 2, LabCardRarity.Rare);
        Add(config, "reroll_discount", "재활용 공정", "유닛 리롤에 필요한 광물이 감소합니다.", LabEffectType.RerollCostReductionPercent, 10, LabCardRarity.Rare);
        Add(config, "max_units", "지휘 체계", "전투 시작 시 최대 유닛 수가 증가합니다.", LabEffectType.MaxUnitCount, 1, LabCardRarity.Legendary);
        Add(config, "idle_reward", "자동 연구", "방치형 보상 획득량이 증가합니다.", LabEffectType.IdleRewardPercent, 10, LabCardRarity.Rare);
        Add(config, "attack_speed", "신경 가속", "아군 유닛의 공격 속도가 증가합니다.", LabEffectType.AttackSpeedPercent, 5, LabCardRarity.Common);
        Add(config, "detection_range", "광역 탐지", "아군 유닛의 탐지 범위가 증가합니다.", LabEffectType.DetectionRangePercent, 10, LabCardRarity.Common);
        Add(config, "energy_recovery", "동력 순환", "아군 유닛의 에너지 회복량이 증가합니다.", LabEffectType.EnergyRecoveryPercent, 8, LabCardRarity.Common);
        Add(config, "wave_reward", "현장 채굴", "웨이브 완료 시 획득하는 광물이 증가합니다.", LabEffectType.WaveRewardPercent, 10, LabCardRarity.Rare);
        Add(config, "sell_price", "자원 회수", "유닛 판매로 돌려받는 광물이 증가합니다.", LabEffectType.SellPricePercent, 20, LabCardRarity.Rare);
        Add(config, "population_discount", "지휘 자동화", "최대 유닛 수 확장에 필요한 광물이 감소합니다.", LabEffectType.PopulationUpgradeCostReductionPercent, 15, LabCardRarity.Rare);
        EditorUtility.SetDirty(config);
        return config;
    }

    private static void Add(LabConfigSO config, string id, string title, string description, LabEffectType type, float value, LabCardRarity rarity)
    {
        string path = $"{CardFolder}/{id}.asset";
        LabCardDataSO card = AssetDatabase.LoadAssetAtPath<LabCardDataSO>(path);
        if (card == null) { card = ScriptableObject.CreateInstance<LabCardDataSO>(); AssetDatabase.CreateAsset(card, path); }
        card.CardId = id; card.DisplayName = title; card.Description = description; card.Rarity = rarity;
        if (card.Icon == null)
            card.Icon = ResolveDefaultIcon(id);
        card.EffectType = type; card.Value = value; card.DrawWeight = rarity switch
        {
            LabCardRarity.Common => 70f,
            LabCardRarity.Rare => 25f,
            LabCardRarity.Legendary => 5f,
            _ => 1f,
        };
        EditorUtility.SetDirty(card); config.Cards.Add(card);
    }

    private static Sprite ResolveDefaultIcon(string cardId)
    {
        string path = cardId switch
        {
            "attack" => "Assets/Art/UI/Generated/Relics/relic_ally_attack.png",
            "attack_speed" => "Assets/Art/UI/Generated/Relics/relic_ally_attack_speed.png",
            "critical_chance" => "Assets/Art/UI/Generated/Relics/relic_ally_critical.png",
            "starting_minerals" => "Assets/Art/UI/Generated/Relics/relic_random_minerals.png",
            "wave_reward" => "Assets/Art/UI/Generated/Relics/relic_random_minerals.png",
            "reroll_discount" => "Assets/Art/UI/Generated/Relics/relic_free_rerolls.png",
            "max_units" => "Assets/Art/UI/Generated/icon_population.png",
            "population_discount" => "Assets/Art/UI/Generated/icon_population.png",
            _ => null,
        };
        return string.IsNullOrEmpty(path) ? null : AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    private static LabCardView CreateCardPrefab(string prefabName, float width, float height)
    {
        string path = $"{PrefabFolder}/{prefabName}.prefab";
        GameObject root = new(prefabName, typeof(RectTransform), typeof(Image), typeof(LabCardGradient), typeof(Button), typeof(LayoutElement), typeof(LabCardView));
        root.GetComponent<RectTransform>().sizeDelta = new Vector2(width, height);
        root.GetComponent<LayoutElement>().preferredWidth = width; root.GetComponent<LayoutElement>().preferredHeight = height;
        Image bg = root.GetComponent<Image>(); bg.color = new Color(.3f, .32f, .35f); root.GetComponent<Button>().targetGraphic = bg;
        RectTransform selectionFrameRect = ChildRect(root.transform, "SelectionFrame");
        Stretch(selectionFrameRect, 0, 0, 0, 0);
        LabSelectionFrame selectionFrame = selectionFrameRect.gameObject.AddComponent<LabSelectionFrame>();
        selectionFrame.color = new Color(1f, 0.98f, 0.78f, 1f);
        selectionFrame.raycastTarget = false;
        selectionFrame.enabled = false;
        Image icon = ImageObject(root.transform, "Icon", Color.white); Anchor((RectTransform)icon.transform, new Vector2(.2f, .32f), new Vector2(.8f, .72f), Vector2.zero, Vector2.zero);
        RectTransform lockedIconRect = ChildRect(root.transform, "LockedFlask");
        Anchor(lockedIconRect, new Vector2(.3f, .3f), new Vector2(.7f, .72f), Vector2.zero, Vector2.zero);
        LabFlaskIcon lockedIcon = lockedIconRect.gameObject.AddComponent<LabFlaskIcon>();
        lockedIcon.color = new Color(.72f, .74f, .78f, 1f);
        lockedIcon.raycastTarget = false;
        TMP_Text name = Text(root.transform, "Name", "?", 25, TextAlignmentOptions.Center); Anchor(name.rectTransform, new Vector2(.04f, .73f), new Vector2(.96f, .98f), Vector2.zero, Vector2.zero);
        TMP_Text value = Text(root.transform, "Value", "LOCKED", 23, TextAlignmentOptions.Center); Anchor(value.rectTransform, new Vector2(.04f, .03f), new Vector2(.96f, .28f), Vector2.zero, Vector2.zero);
        SerializedObject so = new(root.GetComponent<LabCardView>()); Set(so, "button", root.GetComponent<Button>()); Set(so, "background", bg); Set(so, "gradient", root.GetComponent<LabCardGradient>()); Set(so, "selectionFrame", selectionFrame); Set(so, "icon", icon); Set(so, "lockedIcon", lockedIcon); Set(so, "nameText", name); Set(so, "valueText", value); so.ApplyModifiedPropertiesWithoutUndo();
        UnityEditor.Events.UnityEventTools.AddPersistentListener(root.GetComponent<Button>().onClick, root.GetComponent<LabCardView>().HandleClick);
        PrefabUtility.SaveAsPrefabAsset(root, path);
        Object.DestroyImmediate(root);
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
        return AssetDatabase.LoadAssetAtPath<GameObject>(path)?.GetComponent<LabCardView>();
    }

    private static ScrollRect BuildScroll(Transform parent, out Transform content)
    {
        GameObject root = Panel(parent, "Card_ScrollRect", new Color(0, 0, 0, .12f)); Anchor((RectTransform)root.transform, new Vector2(0, .08f), Vector2.one, Vector2.zero, Vector2.zero);
        ScrollRect scroll = GetOrAdd<ScrollRect>(root); scroll.horizontal = false;
        GameObject viewport = Panel(root.transform, "Viewport", Color.clear); Stretch((RectTransform)viewport.transform, 0, 0, 0, 0); GetOrAdd<RectMask2D>(viewport);
        RectTransform c = ChildRect(viewport.transform, "Content"); c.anchorMin = new Vector2(0, 1); c.anchorMax = new Vector2(1, 1); c.pivot = new Vector2(.5f, 1); c.sizeDelta = Vector2.zero;
        GridLayoutGroup grid = GetOrAdd<GridLayoutGroup>(c.gameObject); grid.cellSize = new Vector2(180, 240); grid.spacing = new Vector2(15, 15); grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount; grid.constraintCount = 5; grid.childAlignment = TextAnchor.UpperCenter;
        GetOrAdd<ContentSizeFitter>(c.gameObject).verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        scroll.viewport = (RectTransform)viewport.transform; scroll.content = c; content = c; return scroll;
    }

    private static T FindComponent<T>(Transform root, string name) where T : Component { Transform t = Find(root, name); return t?.GetComponent<T>(); }
    private static TMP_Text FindText(Transform root, string name) => FindComponent<TMP_Text>(root, name);
    private static Transform Find(Transform root, string name) { foreach (Transform t in root.GetComponentsInChildren<Transform>(true)) if (t.name == name) return t; return null; }
    private static GameObject FindSceneObject(Scene scene, string name)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            Transform found = Find(root.transform, name);
            if (found != null)
                return found.gameObject;
        }

        return null;
    }
    private static Transform FindAncestor(Transform child, string name)
    {
        for (Transform current = child; current != null; current = current.parent)
            if (current.name == name)
                return current;
        return null;
    }
    private static void DestroySceneObject(Scene scene, string name)
    {
        GameObject existing = FindSceneObject(scene, name);
        if (existing != null)
            Object.DestroyImmediate(existing);
    }
    private static bool IsSceneWired()
    {
        if (!System.IO.File.Exists(ScenePath))
            return false;
        string yaml = System.IO.File.ReadAllText(ScenePath);
        return !yaml.Contains("m_Name: Lab_Runtime") &&
               yaml.Contains("Assembly-CSharp::LabPanelView") &&
               yaml.Contains("m_Name: Lab_OfferOverlay") &&
               yaml.Contains("m_Name: Lab_DetailPopup") &&
               yaml.Contains("detailGradient:") &&
               !yaml.Contains("detailGradient: {fileID: 0}") &&
               !yaml.Contains("currencyText: {fileID: 0}") &&
               !yaml.Contains("cardPrefab: {fileID: 0}") &&
               !yaml.Contains("offerCardPrefab: {fileID: 0}");
    }
    private static RectTransform ChildRect(Transform p, string n) { Transform old = p.Find(n); if (old != null) Object.DestroyImmediate(old.gameObject); GameObject g = new(n, typeof(RectTransform)); g.transform.SetParent(p, false); return (RectTransform)g.transform; }
    private static GameObject Panel(Transform p, string n, Color c) { RectTransform r = ChildRect(p, n); Image i = r.gameObject.AddComponent<Image>(); i.color = c; return r.gameObject; }
    private static Image ImageObject(Transform p, string n, Color c) { GameObject g = Panel(p, n, c); return g.GetComponent<Image>(); }
    private static TMP_Text Text(Transform p, string n, string s, float size, TextAlignmentOptions align) { RectTransform r = ChildRect(p, n); TextMeshProUGUI t = r.gameObject.AddComponent<TextMeshProUGUI>(); t.text = s; t.fontSize = size; t.alignment = align; t.color = Color.white; return t; }
    private static Button ButtonObject(Transform p, string n, string label, Color c) { GameObject g = Panel(p, n, c); Button b = g.AddComponent<Button>(); b.targetGraphic = g.GetComponent<Image>(); TMP_Text t = Text(g.transform, "Label", label, 28, TextAlignmentOptions.Center); Stretch(t.rectTransform, 0, 0, 0, 0); return b; }
    private static T GetOrAdd<T>(GameObject g) where T : Component => g.GetComponent<T>() ?? g.AddComponent<T>();
    private static void Set(SerializedObject so, string n, Object value) { SerializedProperty p = so.FindProperty(n); if (p == null) throw new System.InvalidOperationException($"Missing field {n}"); p.objectReferenceValue = value; }
    private static void Anchor(RectTransform r, Vector2 min, Vector2 max, Vector2 pos, Vector2 size) { r.anchorMin = min; r.anchorMax = max; r.anchoredPosition = pos; r.sizeDelta = size; }
    private static void Stretch(RectTransform r, float l, float t, float rr, float b) { r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one; r.offsetMin = new Vector2(l, b); r.offsetMax = new Vector2(-rr, -t); }
    private static void EnsureFolder(string path) { string[] parts = path.Split('/'); string current = parts[0]; for (int i = 1; i < parts.Length; i++) { string next = current + "/" + parts[i]; if (!AssetDatabase.IsValidFolder(next)) AssetDatabase.CreateFolder(current, parts[i]); current = next; } }
}
#endif
