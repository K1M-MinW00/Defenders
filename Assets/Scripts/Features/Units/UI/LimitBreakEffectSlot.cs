using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LimitBreakEffectSlot: MonoBehaviour
{
    // [SerializeField] private Image icon;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private GameObject lockObject;
    [SerializeField] private CanvasGroup canvasGroup;

    public void Setup(LimitBreakData data, bool unlocked)
    {
        descriptionText.text = data.description;

        lockObject.SetActive(!unlocked);

        canvasGroup.alpha = unlocked ? 1f : 0.4f;
    }
}