using TMPro;
using UnityEngine;

public sealed class StageCombatStatisticsRowUI : MonoBehaviour
{
    [SerializeField] private TMP_Text rankText;
    [SerializeField] private UnityEngine.UI.Image portraitImage;
    [SerializeField] private TMP_Text damageText;
    [SerializeField] private TMP_Text healingText;
    [SerializeField] private TMP_Text takenText;
    [SerializeField] private UnityEngine.UI.Slider damageSlider;
    [SerializeField] private UnityEngine.UI.Slider healingSlider;
    [SerializeField] private UnityEngine.UI.Slider takenSlider;

    public void SetEmpty(int rank)
    {
        if (rankText != null) rankText.text = rank.ToString();
        if (portraitImage != null) { portraitImage.sprite = null; portraitImage.color = new Color(1f, 1f, 1f, 0.12f); }
        SetValues(0f, 0f, 0f, 0f, 0f, 0f);
    }

    public void SetData(
        int rank,
        UnitCombatStatistics entry,
        float maximumDamage,
        float maximumHealing,
        float maximumTaken)
    {
        UnitController unit = entry?.Unit;
        if (unit == null) { SetEmpty(rank); return; }
        if (rankText != null) rankText.text = rank.ToString();
        if (portraitImage != null)
        {
            portraitImage.sprite = unit.UnitData?.icon;
            portraitImage.color = portraitImage.sprite != null ? Color.white : new Color(1f, 1f, 1f, 0.12f);
            portraitImage.preserveAspect = true;
        }
        SetValues(
            entry.DamageDealt,
            entry.HealingDone,
            entry.DamageTaken,
            maximumDamage,
            maximumHealing,
            maximumTaken);
    }

    private void SetValues(
        float damage,
        float healing,
        float taken,
        float maximumDamage,
        float maximumHealing,
        float maximumTaken)
    {
        if (damageText != null) damageText.text = FormatValue(damage);
        if (healingText != null) healingText.text = FormatValue(healing);
        if (takenText != null) takenText.text = FormatValue(taken);
        SetSlider(damageSlider, damage, maximumDamage);
        SetSlider(healingSlider, healing, maximumHealing);
        SetSlider(takenSlider, taken, maximumTaken);
    }

    private static void SetSlider(UnityEngine.UI.Slider slider, float value, float maximum)
    {
        if (slider != null)
            slider.value = maximum > 0f ? Mathf.Clamp01(value / maximum) : 0f;
    }

    public static string FormatValue(float value)
    {
        value = Mathf.Max(0f, value);
        if (value < 1000f)
            return Mathf.RoundToInt(value).ToString("N0");

        float thousands = Mathf.Floor(value / 100f + 0.5f) / 10f;
        return thousands.ToString("0.#", System.Globalization.CultureInfo.InvariantCulture) + "K";
    }
}
