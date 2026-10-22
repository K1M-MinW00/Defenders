using System.Collections.Generic;
using TMPro;
using UnityEngine;

public sealed class MonsterStatusEffectBarView : MonoBehaviour
{
    private const int MaxVisibleEffects = 4;
    private static readonly Color SlotBackground = new(0.04f, 0.05f, 0.08f, 0.88f);
    private static readonly Color CooldownOverlay = new(0f, 0f, 0f, 0.58f);

    private readonly List<Slot> slots = new(MaxVisibleEffects);
    private readonly List<MonsterStatusEffectController.Snapshot> sortedEffects = new(MaxVisibleEffects + 1);
    private readonly List<StatusEffectType> visibleTypes = new(MaxVisibleEffects);
    private MonsterStatusEffectController source;
    private TextMeshProUGUI immuneText;
    private float immuneFeedbackRemaining;

    private const float ImmuneFeedbackDuration = 0.85f;

    public int VisibleCount { get; private set; }
    public IReadOnlyList<StatusEffectType> VisibleTypes => visibleTypes;

    public static MonsterStatusEffectBarView Create(Transform owner)
    {
        GameObject root = new("MonsterStatusEffectCanvas", typeof(RectTransform));
        root.layer = owner.gameObject.layer;
        root.transform.SetParent(owner, false);

        RectTransform rect = root.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(152f, 60f);

        Canvas canvas = root.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.overrideSorting = true;
        canvas.sortingOrder = 5100;

        UnityEngine.UI.CanvasScaler scaler = root.AddComponent<UnityEngine.UI.CanvasScaler>();
        scaler.dynamicPixelsPerUnit = 10f;
        return root.AddComponent<MonsterStatusEffectBarView>();
    }

    public void ConfigureHeight(MonsterClassification classification)
    {
        float height = classification switch
        {
            MonsterClassification.Boss => 2.05f,
            MonsterClassification.Elite => 1.7f,
            _ => 1.45f,
        };
        transform.localPosition = new Vector3(0f, height, 0f);
        transform.localScale = Vector3.one * (classification switch
        {
            MonsterClassification.Boss => 0.0095f,
            MonsterClassification.Elite => 0.0085f,
            _ => 0.0075f,
        });
    }

    public void Bind(MonsterStatusEffectController statusEffects)
    {
        if (source != null)
        {
            source.EffectsChanged -= Refresh;
            source.EffectImmune -= ShowImmune;
        }

        source = statusEffects;
        EnsureSlots();
        if (source != null)
        {
            source.EffectsChanged += Refresh;
            source.EffectImmune += ShowImmune;
        }
        Refresh();
    }

    private void OnDestroy()
    {
        if (source != null)
        {
            source.EffectsChanged -= Refresh;
            source.EffectImmune -= ShowImmune;
        }
    }

    private void Update()
    {
        if (immuneFeedbackRemaining <= 0f || immuneText == null)
            return;

        immuneFeedbackRemaining = Mathf.Max(0f, immuneFeedbackRemaining - Time.deltaTime);
        float progress = 1f - immuneFeedbackRemaining / ImmuneFeedbackDuration;
        RectTransform rect = immuneText.rectTransform;
        rect.anchoredPosition = new Vector2(0f, Mathf.Lerp(22f, 40f, progress));
        Color color = immuneText.color;
        color.a = Mathf.Clamp01(immuneFeedbackRemaining / 0.3f);
        immuneText.color = color;
        if (immuneFeedbackRemaining <= 0f)
        {
            immuneText.gameObject.SetActive(false);
            Refresh();
        }
    }

    private void LateUpdate() => Refresh();

    public void Refresh()
    {
        EnsureSlots();
        sortedEffects.Clear();
        IReadOnlyList<MonsterStatusEffectController.Snapshot> effects = source?.ActiveEffects;
        if (effects != null)
        {
            for (int index = 0; index < effects.Count; index++)
                sortedEffects.Add(effects[index]);
            sortedEffects.Sort((left, right) => GetPriority(left.Type).CompareTo(GetPriority(right.Type)));
        }

        VisibleCount = Mathf.Min(sortedEffects.Count, MaxVisibleEffects);
        visibleTypes.Clear();
        for (int index = 0; index < slots.Count; index++)
        {
            bool visible = index < VisibleCount;
            slots[index].Root.SetActive(visible);
            if (!visible)
                continue;

            MonsterStatusEffectController.Snapshot snapshot = sortedEffects[index];
            visibleTypes.Add(snapshot.Type);
            ApplySnapshot(slots[index], snapshot);
        }

        gameObject.SetActive(VisibleCount > 0 || immuneFeedbackRemaining > 0f);
    }

    private void EnsureSlots()
    {
        if (slots.Count > 0)
            return;

        UnityEngine.UI.HorizontalLayoutGroup layout = gameObject.AddComponent<UnityEngine.UI.HorizontalLayoutGroup>();
        layout.spacing = 4f;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;

        for (int index = 0; index < MaxVisibleEffects; index++)
            slots.Add(CreateSlot(index));

        GameObject feedbackObject = CreateUiObject("ImmuneFeedback", transform, new Vector2(140f, 28f));
        UnityEngine.UI.LayoutElement feedbackLayout = feedbackObject.AddComponent<UnityEngine.UI.LayoutElement>();
        feedbackLayout.ignoreLayout = true;
        RectTransform feedbackRect = feedbackObject.GetComponent<RectTransform>();
        feedbackRect.anchorMin = feedbackRect.anchorMax = new Vector2(0.5f, 0.5f);
        feedbackRect.pivot = new Vector2(0.5f, 0.5f);
        feedbackRect.anchoredPosition = new Vector2(0f, 22f);
        immuneText = feedbackObject.AddComponent<TextMeshProUGUI>();
        immuneText.alignment = TextAlignmentOptions.Center;
        immuneText.fontSize = 20f;
        immuneText.fontStyle = FontStyles.Bold;
        immuneText.color = new Color(0.72f, 0.86f, 1f, 1f);
        immuneText.outlineWidth = 0.18f;
        immuneText.outlineColor = new Color32(8, 16, 28, 230);
        immuneText.raycastTarget = false;
        feedbackObject.SetActive(false);
    }

    public void ShowImmune(StatusEffectType _)
    {
        EnsureSlots();
        immuneFeedbackRemaining = ImmuneFeedbackDuration;
        immuneText.text = LocalizationService.Get(LocalizationTables.Stage, "combat.status.immune");
        immuneText.color = new Color(0.72f, 0.86f, 1f, 1f);
        immuneText.rectTransform.anchoredPosition = new Vector2(0f, 22f);
        immuneText.gameObject.SetActive(true);
        gameObject.SetActive(true);
    }

    private Slot CreateSlot(int index)
    {
        GameObject root = CreateUiObject($"StatusEffectSlot_{index + 1}", transform, new Vector2(34f, 34f));
        UnityEngine.UI.Image background = root.AddComponent<UnityEngine.UI.Image>();
        background.color = SlotBackground;
        background.raycastTarget = false;

        GameObject iconObject = CreateUiObject("Icon", root.transform, new Vector2(26f, 26f));
        Stretch(iconObject.GetComponent<RectTransform>(), 4f);
        UnityEngine.UI.Image icon = iconObject.AddComponent<UnityEngine.UI.Image>();
        icon.raycastTarget = false;

        GameObject glyphObject = CreateUiObject("Glyph", iconObject.transform, Vector2.zero);
        Stretch(glyphObject.GetComponent<RectTransform>(), 0f);
        TextMeshProUGUI glyph = glyphObject.AddComponent<TextMeshProUGUI>();
        glyph.alignment = TextAlignmentOptions.Center;
        glyph.fontSize = 18f;
        glyph.fontStyle = FontStyles.Bold;
        glyph.color = Color.white;
        glyph.raycastTarget = false;

        GameObject overlayObject = CreateUiObject("RemainingOverlay", iconObject.transform, Vector2.zero);
        Stretch(overlayObject.GetComponent<RectTransform>(), 0f);
        UnityEngine.UI.Image overlay = overlayObject.AddComponent<UnityEngine.UI.Image>();
        overlay.color = CooldownOverlay;
        overlay.type = UnityEngine.UI.Image.Type.Filled;
        overlay.fillMethod = UnityEngine.UI.Image.FillMethod.Radial360;
        overlay.fillOrigin = (int)UnityEngine.UI.Image.Origin360.Top;
        overlay.fillClockwise = false;
        overlay.raycastTarget = false;

        GameObject timeObject = CreateUiObject("RemainingTime", root.transform, new Vector2(34f, 14f));
        RectTransform timeRect = timeObject.GetComponent<RectTransform>();
        timeRect.anchorMin = timeRect.anchorMax = new Vector2(0.5f, 0f);
        timeRect.pivot = new Vector2(0.5f, 0f);
        timeRect.anchoredPosition = new Vector2(0f, -11f);
        TextMeshProUGUI time = timeObject.AddComponent<TextMeshProUGUI>();
        time.alignment = TextAlignmentOptions.Center;
        time.fontSize = 11f;
        time.fontStyle = FontStyles.Bold;
        time.color = Color.white;
        time.raycastTarget = false;

        root.SetActive(false);
        return new Slot(root, icon, glyph, overlay, time);
    }

    private static void ApplySnapshot(Slot slot, MonsterStatusEffectController.Snapshot snapshot)
    {
        Sprite sprite = StatusEffectIconSetSO.Load()?.Get(snapshot.Type);
        slot.Icon.sprite = sprite;
        slot.Icon.preserveAspect = true;
        slot.Icon.color = sprite != null ? Color.white : GetColor(snapshot.Type);
        slot.Glyph.gameObject.SetActive(sprite == null);
        slot.Glyph.text = sprite == null ? GetGlyph(snapshot.Type) : string.Empty;
        slot.Overlay.fillAmount = 1f - Mathf.Clamp01(snapshot.RemainingTime / snapshot.Duration);
        slot.Time.text = snapshot.RemainingTime < 10f
            ? snapshot.RemainingTime.ToString("0.0")
            : Mathf.CeilToInt(snapshot.RemainingTime).ToString();
    }

    private static Color GetColor(StatusEffectType type) => type switch
    {
        StatusEffectType.Slow => new Color(0.2f, 0.65f, 1f, 1f),
        StatusEffectType.Stun => new Color(1f, 0.82f, 0.18f, 1f),
        StatusEffectType.Poison => new Color(0.45f, 0.85f, 0.2f, 1f),
        StatusEffectType.Burn => new Color(1f, 0.3f, 0.08f, 1f),
        StatusEffectType.DefenseDown => new Color(0.72f, 0.34f, 0.9f, 1f),
        _ => Color.white,
    };

    private static string GetGlyph(StatusEffectType type) => type switch
    {
        StatusEffectType.Slow => "S",
        StatusEffectType.Stun => "Z",
        StatusEffectType.Poison => "P",
        StatusEffectType.Burn => "B",
        StatusEffectType.DefenseDown => "D",
        _ => "?",
    };

    private static int GetPriority(StatusEffectType type) => type switch
    {
        StatusEffectType.Stun => 0,
        StatusEffectType.DefenseDown => 1,
        StatusEffectType.Burn => 2,
        StatusEffectType.Poison => 3,
        StatusEffectType.Slow => 4,
        _ => 100,
    };

    private static GameObject CreateUiObject(string name, Transform parent, Vector2 size)
    {
        GameObject result = new(name, typeof(RectTransform));
        result.layer = parent.gameObject.layer;
        result.transform.SetParent(parent, false);
        result.GetComponent<RectTransform>().sizeDelta = size;
        return result;
    }

    private static void Stretch(RectTransform rect, float inset)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.one * inset;
        rect.offsetMax = Vector2.one * -inset;
    }

    private sealed class Slot
    {
        public GameObject Root { get; }
        public UnityEngine.UI.Image Icon { get; }
        public TextMeshProUGUI Glyph { get; }
        public UnityEngine.UI.Image Overlay { get; }
        public TextMeshProUGUI Time { get; }

        public Slot(GameObject root, UnityEngine.UI.Image icon, TextMeshProUGUI glyph,
            UnityEngine.UI.Image overlay, TextMeshProUGUI time)
        {
            Root = root;
            Icon = icon;
            Glyph = glyph;
            Overlay = overlay;
            Time = time;
        }
    }
}
