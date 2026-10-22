using System.Collections.Generic;
using TMPro;
using UnityEngine;

public sealed class UnitStatusEffectBarView : MonoBehaviour
{
    private const int MaxVisibleEffects = 4;
    private static readonly Color SlotBackground = new(0.04f, 0.05f, 0.08f, 0.92f);
    private static readonly Color CooldownOverlay = new(0f, 0f, 0f, 0.62f);

    private readonly List<Slot> slots = new(MaxVisibleEffects);
    private readonly List<UnitStatusEffectController.Snapshot> sortedEffects = new(MaxVisibleEffects + 1);
    private readonly List<StatusEffectType> visibleTypes = new(MaxVisibleEffects);
    private UnitStatusEffectController source;

    public int VisibleCount { get; private set; }
    public IReadOnlyList<StatusEffectType> VisibleTypes => visibleTypes;

    public void Bind(UnitStatusEffectController statusEffects)
    {
        if (source == statusEffects)
        {
            EnsureSlots();
            Refresh();
            return;
        }

        if (source != null)
            source.EffectsChanged -= Refresh;

        source = statusEffects;
        EnsureSlots();

        if (source != null)
            source.EffectsChanged += Refresh;

        Refresh();
    }

    private void OnDestroy()
    {
        if (source != null)
            source.EffectsChanged -= Refresh;
    }

    private void LateUpdate()
    {
        Refresh();
    }

    public void Refresh()
    {
        EnsureSlots();
        IReadOnlyList<UnitStatusEffectController.Snapshot> effects = source?.ActiveEffects;
        sortedEffects.Clear();
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
            if (visible)
            {
                UnitStatusEffectController.Snapshot snapshot = sortedEffects[index];
                visibleTypes.Add(snapshot.Type);
                ApplySnapshot(slots[index], snapshot);
            }
        }

        gameObject.SetActive(VisibleCount > 0);
    }

    private void EnsureSlots()
    {
        if (slots.Count > 0)
            return;

        RectTransform rect = transform as RectTransform;
        if (rect != null)
        {
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(0f, 5.2f);
            rect.sizeDelta = new Vector2(152f, 34f);
            rect.localScale = Vector3.one * 0.1f;
        }

        UnityEngine.UI.HorizontalLayoutGroup layout = gameObject.GetComponent<UnityEngine.UI.HorizontalLayoutGroup>();
        if (layout == null)
            layout = gameObject.AddComponent<UnityEngine.UI.HorizontalLayoutGroup>();
        layout.spacing = 4f;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;

        for (int index = 0; index < MaxVisibleEffects; index++)
            slots.Add(CreateSlot(index));
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

    private static void ApplySnapshot(Slot slot, UnitStatusEffectController.Snapshot snapshot)
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
        _ => Color.white
    };

    private static string GetGlyph(StatusEffectType type) => type switch
    {
        StatusEffectType.Slow => "S",
        StatusEffectType.Stun => "Z",
        StatusEffectType.Poison => "P",
        StatusEffectType.Burn => "B",
        StatusEffectType.DefenseDown => "D",
        _ => "?"
    };

    private static int GetPriority(StatusEffectType type) => type switch
    {
        StatusEffectType.Stun => 0,
        StatusEffectType.DefenseDown => 1,
        StatusEffectType.Burn => 2,
        StatusEffectType.Poison => 3,
        StatusEffectType.Slow => 4,
        _ => 100
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

        public Slot(
            GameObject root,
            UnityEngine.UI.Image icon,
            TextMeshProUGUI glyph,
            UnityEngine.UI.Image overlay,
            TextMeshProUGUI time)
        {
            Root = root;
            Icon = icon;
            Glyph = glyph;
            Overlay = overlay;
            Time = time;
        }
    }
}
