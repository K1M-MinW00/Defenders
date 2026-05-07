using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UnitDetailTabButton : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private TMP_Text labelText;
    [SerializeField] private RectTransform targetRect;

    [Header("Visual")]
    [SerializeField] private Color normalColor = new Color(0.15f, 0.25f, 0.38f, 1f);
    [SerializeField] private Color selectedColor = new Color(0.15f, 0.75f, 1f, 1f);

    public Button Button => button;

    private void Reset()
    {
        button = GetComponent<Button>();
        targetRect = GetComponent<RectTransform>();
    }

    public void SetSelected(bool selected)
    {
        if (backgroundImage != null)
            backgroundImage.color = selected ? selectedColor : normalColor;

        if (labelText != null)
            labelText.fontStyle = selected ? FontStyles.Bold : FontStyles.Normal;
    }
}