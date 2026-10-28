using System.Collections.Generic;
using TMPro;
using UnityEngine;

public sealed class BossStatusSummaryView : MonoBehaviour
{
    private const float IconSize = 26f;
    private const float ItemSpacing = 4f;
    private const float CategorySpacing = 16f;
    private static readonly StatusEffectType[] StatusTypes =
    {
        StatusEffectType.Slow,
        StatusEffectType.Stun,
        StatusEffectType.Poison,
        StatusEffectType.Burn,
        StatusEffectType.DefenseDown,
    };

    private RectTransform summaryRoot;
    private SummaryCategory skillCategory;
    private SummaryCategory weaknessCategory;
    private SummaryCategory resistanceCategory;
    private SummaryCategory immunityCategory;

    public int SkillCount => skillCategory?.VisibleIconCount ?? 0;
    public int WeaknessCount => weaknessCategory?.VisibleIconCount ?? 0;
    public int ResistanceCount => resistanceCategory?.VisibleIconCount ?? 0;
    public int ImmunityCount => immunityCategory?.VisibleIconCount ?? 0;
    public float PreferredContentWidth
    {
        get
        {
            SummaryCategory[] categories =
            {
                skillCategory,
                weaknessCategory,
                resistanceCategory,
                immunityCategory,
            };
            float width = 0f;
            int visibleCount = 0;
            foreach (SummaryCategory category in categories)
            {
                if (category == null || category.VisibleIconCount == 0)
                    continue;
                width += category.PreferredWidth;
                visibleCount++;
            }
            return width + Mathf.Max(0, visibleCount - 1) * CategorySpacing;
        }
    }

    public void Bind(BossDefinitionSO definition, TMP_Text styleSource)
    {
        EnsureBuilt(styleSource);
        if (definition == null)
        {
            summaryRoot.gameObject.SetActive(false);
            return;
        }

        List<StatusEffectType> skills = GetSkillEffects(definition.areaAttackStatusEffects);
        List<StatusEffectType> weaknesses = new();
        List<StatusEffectType> resistances = new();
        List<StatusEffectType> immunities = new();
        BossStatusResistanceSettings settings = definition.statusResistance;

        foreach (StatusEffectType type in StatusTypes)
        {
            float value = settings?.Get(type) ?? 0f;
            if (value >= 0.999f)
                immunities.Add(type);
            else if (value >= 0.5f)
                resistances.Add(type);
            else if (value <= 0f)
                weaknesses.Add(type);
        }

        StatusEffectIconSetSO icons = StatusEffectIconSetSO.Load();
        skillCategory.Set(Localize("monster.info.summary.skill"), skills, icons);
        weaknessCategory.Set(Localize("monster.info.summary.weakness"), weaknesses, icons);
        resistanceCategory.Set(Localize("monster.info.summary.resistance"), resistances, icons);
        immunityCategory.Set(Localize("monster.info.summary.immunity"), immunities, icons);
        summaryRoot.gameObject.SetActive(
            SkillCount + WeaknessCount + ResistanceCount + ImmunityCount > 0);
    }

    public void Hide()
    {
        if (summaryRoot != null)
            summaryRoot.gameObject.SetActive(false);
    }

    private void EnsureBuilt(TMP_Text styleSource)
    {
        if (summaryRoot != null)
            return;

        Transform existing = transform.Find("BossStatusSummary");
        if (existing != null)
            Destroy(existing.gameObject);

        GameObject rootObject = new("BossStatusSummary", typeof(RectTransform));
        summaryRoot = rootObject.GetComponent<RectTransform>();
        summaryRoot.SetParent(transform, false);
        summaryRoot.anchorMin = new Vector2(0f, 0f);
        summaryRoot.anchorMax = new Vector2(0.72f, 0f);
        summaryRoot.pivot = new Vector2(0.5f, 0f);
        summaryRoot.anchoredPosition = new Vector2(10f, 14f);
        summaryRoot.sizeDelta = new Vector2(-36f, 32f);

        UnityEngine.UI.HorizontalLayoutGroup layout = rootObject.AddComponent<UnityEngine.UI.HorizontalLayoutGroup>();
        layout.spacing = CategorySpacing;
        layout.childAlignment = TextAnchor.MiddleLeft;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;

        skillCategory = CreateCategory("Skill", styleSource);
        weaknessCategory = CreateCategory("Weakness", styleSource);
        resistanceCategory = CreateCategory("Resistance", styleSource);
        immunityCategory = CreateCategory("Immunity", styleSource);
        summaryRoot.gameObject.SetActive(false);
    }

    private SummaryCategory CreateCategory(string name, TMP_Text styleSource)
    {
        GameObject categoryObject = new(name, typeof(RectTransform));
        categoryObject.transform.SetParent(summaryRoot, false);
        UnityEngine.UI.LayoutElement categoryLayout = categoryObject.AddComponent<UnityEngine.UI.LayoutElement>();
        categoryLayout.preferredHeight = 32f;

        UnityEngine.UI.HorizontalLayoutGroup layout = categoryObject.AddComponent<UnityEngine.UI.HorizontalLayoutGroup>();
        layout.spacing = ItemSpacing;
        layout.childAlignment = TextAnchor.MiddleLeft;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;

        GameObject labelObject = new("Label", typeof(RectTransform));
        labelObject.transform.SetParent(categoryObject.transform, false);
        TextMeshProUGUI label = labelObject.AddComponent<TextMeshProUGUI>();
        label.font = styleSource != null ? styleSource.font : TMP_Settings.defaultFontAsset;
        label.fontSize = styleSource != null ? Mathf.Min(styleSource.fontSize, 20f) : 20f;
        label.color = styleSource != null ? styleSource.color : new Color(0.07f, 0.09f, 0.12f, 1f);
        label.alignment = TextAlignmentOptions.MidlineLeft;
        label.textWrappingMode = TextWrappingModes.NoWrap;
        label.raycastTarget = false;
        UnityEngine.UI.LayoutElement labelLayout = labelObject.AddComponent<UnityEngine.UI.LayoutElement>();
        labelLayout.preferredHeight = 32f;

        return new SummaryCategory(categoryObject, categoryLayout, label, labelLayout);
    }

    private static List<StatusEffectType> GetSkillEffects(BossAreaAttackStatusSettings effects)
    {
        List<StatusEffectType> result = new();
        if (effects == null)
            return result;
        if (effects.enableSlow) result.Add(StatusEffectType.Slow);
        if (effects.enableStun) result.Add(StatusEffectType.Stun);
        if (effects.enablePoison) result.Add(StatusEffectType.Poison);
        if (effects.enableBurn) result.Add(StatusEffectType.Burn);
        if (effects.enableDefenseDown) result.Add(StatusEffectType.DefenseDown);
        return result;
    }

    private static string Localize(string key) => LocalizationService.Get(LocalizationTables.Stage, key);

    private sealed class SummaryCategory
    {
        private readonly GameObject root;
        private readonly UnityEngine.UI.LayoutElement rootLayout;
        private readonly TMP_Text label;
        private readonly UnityEngine.UI.LayoutElement labelLayout;
        private readonly List<UnityEngine.UI.Image> iconPool = new();

        public int VisibleIconCount { get; private set; }
        public float PreferredWidth => rootLayout.preferredWidth;

        public SummaryCategory(
            GameObject root,
            UnityEngine.UI.LayoutElement rootLayout,
            TMP_Text label,
            UnityEngine.UI.LayoutElement labelLayout)
        {
            this.root = root;
            this.rootLayout = rootLayout;
            this.label = label;
            this.labelLayout = labelLayout;
        }

        public void Set(string title, IReadOnlyList<StatusEffectType> types, StatusEffectIconSetSO iconSet)
        {
            label.text = title;
            label.ForceMeshUpdate();
            labelLayout.preferredWidth = Mathf.Ceil(label.preferredWidth);
            EnsureIconCount(types.Count);

            VisibleIconCount = 0;
            for (int i = 0; i < iconPool.Count; i++)
            {
                bool visible = i < types.Count;
                UnityEngine.UI.Image image = iconPool[i];
                image.gameObject.SetActive(visible);
                if (!visible)
                    continue;

                image.sprite = iconSet != null ? iconSet.Get(types[i]) : null;
                image.color = image.sprite != null ? Color.white : Color.clear;
                VisibleIconCount++;
            }

            root.SetActive(VisibleIconCount > 0);
            rootLayout.preferredWidth = labelLayout.preferredWidth
                                        + VisibleIconCount * IconSize
                                        + VisibleIconCount * ItemSpacing;
        }

        private void EnsureIconCount(int count)
        {
            while (iconPool.Count < count)
            {
                GameObject iconObject = new($"Icon{iconPool.Count + 1}", typeof(RectTransform));
                iconObject.transform.SetParent(root.transform, false);
                UnityEngine.UI.Image image = iconObject.AddComponent<UnityEngine.UI.Image>();
                image.preserveAspect = true;
                image.raycastTarget = false;
                UnityEngine.UI.LayoutElement layout = iconObject.AddComponent<UnityEngine.UI.LayoutElement>();
                layout.preferredWidth = IconSize;
                layout.preferredHeight = IconSize;
                iconPool.Add(image);
            }
        }
    }
}
