using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WaveNodeUI : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI numberText;
    [SerializeField] private GameObject currentArrow;

    [Header("Colors")]
    [SerializeField] private Color normalTextColor = Color.white;
    [SerializeField] private Color clearedTextColor = Color.gray;
    [SerializeField] private Color currentTextColor = Color.white;

    public void Setup(Sprite iconSprite, int waveNumber)
    {
        if (iconImage != null)
            iconImage.sprite = iconSprite;

        if (numberText != null)
            numberText.text = waveNumber.ToString();

        SetAsUpcoming();
    }

    public void SetAsCleared(Color iconTint)
    {
        if (iconImage != null)
            iconImage.color = iconTint;

        if (numberText != null)
            numberText.color = clearedTextColor;

        if (currentArrow != null)
            currentArrow.SetActive(false);
    }

    public void SetAsCurrent(Color iconTint)
    {
        if (iconImage != null)
            iconImage.color = iconTint;

        if (numberText != null)
            numberText.color = currentTextColor;

        if (currentArrow != null)
            currentArrow.SetActive(true);
    }

    public void SetAsUpcoming()
    {
        if (numberText != null)
            numberText.color = normalTextColor;

        if (currentArrow != null)
            currentArrow.SetActive(false);
    }

    public void SetIconTint(Color tint)
    {
        if (iconImage != null)
            iconImage.color = tint;
    }
}