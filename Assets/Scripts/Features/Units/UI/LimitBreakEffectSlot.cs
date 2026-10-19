using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class LimitBreakEffectSlot : MonoBehaviour
{
    // [SerializeField] private Image icon;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private GameObject lockObject;
    [SerializeField] private CanvasGroup canvasGroup;

    public void Setup(string description, bool unlocked)
    {
        descriptionText.text = description;

        lockObject.SetActive(!unlocked);

        canvasGroup.alpha = unlocked ? 1f : 0.4f;
    }
}
