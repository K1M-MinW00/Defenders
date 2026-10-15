using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class StageRelicIconView : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text fallbackSymbolText;
    [SerializeField] private RelicTooltipPressHandler pressHandler;

    public void Bind(StageRelicDefinition relic, Action pressed, Action released)
    {
        if (relic == null)
        {
            gameObject.SetActive(false);
            return;
        }

        Sprite icon = relic.Icon;
        iconImage.sprite = icon;
        iconImage.color = icon != null
            ? Color.white
            : new Color(0.04f, 0.24f, 0.26f, 0.96f);
        iconImage.preserveAspect = true;

        fallbackSymbolText.gameObject.SetActive(icon == null);
        fallbackSymbolText.text = icon == null ? relic.Symbol : string.Empty;
        pressHandler.Initialize(pressed, released);
        gameObject.SetActive(true);
    }

    public void Clear()
    {
        pressHandler.Initialize(null, null);
        gameObject.SetActive(false);
    }
}
