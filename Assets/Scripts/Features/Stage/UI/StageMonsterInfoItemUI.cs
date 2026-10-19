using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class StageMonsterInfoItemUI : MonoBehaviour
{
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private Image portraitImage;
    private MonsterDataSO currentData;

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
            descriptionText.text = string.IsNullOrWhiteSpace(data.monsterId)
                ? Localize("monster.description.unavailable")
                : Localize($"monster.description.{data.monsterId}");
        }

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
