using TMPro;
using UnityEngine;

public sealed class StageMonsterInfoItemUI : MonoBehaviour
{
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private UnityEngine.UI.Image portraitImage;
    [SerializeField, Min(0f)] private float bossPreferredHeight = 430f;
    [SerializeField, Min(0f)] private float resistanceLineHeight = 24f;
    private MonsterDataSO currentData;
    private UnityEngine.UI.LayoutElement layoutElement;
    private float defaultPreferredHeight;

    private void Awake()
    {
        layoutElement = GetComponent<UnityEngine.UI.LayoutElement>();
        if (layoutElement != null)
            defaultPreferredHeight = layoutElement.preferredHeight;
    }

    private void OnEnable() => LocalizationService.LocaleChanged += HandleLocaleChanged;
    private void OnDisable() => LocalizationService.LocaleChanged -= HandleLocaleChanged;

    public void Bind(MonsterDataSO data)
    {
        if (data == null)
            return;

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

        if (layoutElement != null)
            layoutElement.preferredHeight = data.Classification == MonsterClassification.Boss
                ? bossPreferredHeight + CountVisibleResistances(data.BossDefinition) * resistanceLineHeight
                : defaultPreferredHeight;

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

    private static int CountVisibleResistances(BossDefinitionSO definition)
    {
        BossStatusResistanceSettings resistance = definition?.statusResistance;
        if (resistance == null)
            return 0;

        return (resistance.slow > 0f ? 1 : 0) +
               (resistance.stun > 0f ? 1 : 0) +
               (resistance.poison > 0f ? 1 : 0) +
               (resistance.burn > 0f ? 1 : 0) +
               (resistance.defenseDown > 0f ? 1 : 0);
    }

    private void OnDestroy() => LocalizationService.LocaleChanged -= HandleLocaleChanged;
}
