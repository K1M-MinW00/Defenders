using TMPro;
using UnityEngine;

public sealed class StageMonsterInfoItemUI : MonoBehaviour
{
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private UnityEngine.UI.Image portraitImage;
    private const float BossPreferredHeight = 300f;
    private const float BossDescriptionBottomInset = 58f;
    private MonsterDataSO currentData;
    private UnityEngine.UI.LayoutElement layoutElement;
    private float defaultPreferredHeight;
    private BossStatusSummaryView bossStatusSummary;
    private RectTransform descriptionRect;
    private float defaultDescriptionBottomInset;

    private void Awake()
    {
        EnsureReferences();
    }

    private void EnsureReferences()
    {
        layoutElement = GetComponent<UnityEngine.UI.LayoutElement>();
        if (layoutElement != null)
            defaultPreferredHeight = layoutElement.preferredHeight;

        descriptionRect = descriptionText != null ? descriptionText.rectTransform : null;
        if (descriptionRect != null)
            defaultDescriptionBottomInset = descriptionRect.offsetMin.y;

        bossStatusSummary = GetComponent<BossStatusSummaryView>();
        if (bossStatusSummary == null)
            bossStatusSummary = gameObject.AddComponent<BossStatusSummaryView>();
    }

    private void OnEnable() => LocalizationService.LocaleChanged += HandleLocaleChanged;
    private void OnDisable() => LocalizationService.LocaleChanged -= HandleLocaleChanged;

    public void Bind(MonsterDataSO data)
    {
        if (data == null)
            return;

        if (bossStatusSummary == null)
            EnsureReferences();

        currentData = data;

        if (nameText != null)
            nameText.text = string.IsNullOrWhiteSpace(data.monsterId)
                ? (string.IsNullOrWhiteSpace(data.displayName) ? data.name : data.displayName)
                : Localize($"monster.name.{data.monsterId}");

        if (descriptionText != null)
        {
            string description = string.IsNullOrWhiteSpace(data.monsterId)
                ? Localize("monster.description.unavailable")
                : Localize($"monster.description.{data.monsterId}");

            if (data.Classification == MonsterClassification.Boss && data.BossDefinition != null)
                description += $"\n\n{BossMonsterInfoFormatter.Build(data.BossDefinition)}";

            descriptionText.text = description;
        }

        bool isBoss = data.Classification == MonsterClassification.Boss && data.BossDefinition != null;
        if (layoutElement != null)
            layoutElement.preferredHeight = isBoss ? BossPreferredHeight : defaultPreferredHeight;

        if (descriptionRect != null)
        {
            Vector2 offsetMin = descriptionRect.offsetMin;
            offsetMin.y = isBoss ? BossDescriptionBottomInset : defaultDescriptionBottomInset;
            descriptionRect.offsetMin = offsetMin;
        }

        if (isBoss)
            bossStatusSummary.Bind(data.BossDefinition, descriptionText);
        else
            bossStatusSummary.Hide();

        if (portraitImage != null)
        {
            portraitImage.sprite = data.portrait;
            portraitImage.color = data.portrait != null ? Color.white : Color.clear;
            portraitImage.preserveAspect = true;
        }
    }

    private void HandleLocaleChanged(string _)
    {
        if (currentData != null && gameObject.activeInHierarchy)
            Bind(currentData);
    }

    private static string Localize(string key) =>
        LocalizationService.Get(LocalizationTables.Stage, key);

    private void OnDestroy() => LocalizationService.LocaleChanged -= HandleLocaleChanged;
}
