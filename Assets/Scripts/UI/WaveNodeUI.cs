using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WaveNodeUI : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI numberText;
    [SerializeField] private GameObject currentArrow;


    public void Setup(Sprite iconSprite, int waveNumber)
    {
        if (iconImage != null)
            iconImage.sprite = iconSprite;

        if (numberText != null)
            numberText.text = waveNumber.ToString();

        SetAsUpcoming();
    }

    public void SetAsCurrent()
    {
        if (currentArrow != null)
            currentArrow.SetActive(true);
    }

    public void SetAsUpcoming()
    {
        if (currentArrow != null)
            currentArrow.SetActive(false);
    }

    public void SetIconTint(Color tint)
    {
        if (iconImage != null)
            iconImage.color = tint;
    }
}