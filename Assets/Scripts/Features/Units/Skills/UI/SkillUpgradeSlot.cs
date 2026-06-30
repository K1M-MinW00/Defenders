using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillUpgradeSlot : MonoBehaviour
{
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private Image lockImage;
    [SerializeField] private CanvasGroup canvasGroup;

    public void Setup(int promotion, SkillUpgradeData data)
    {
        titleText.text = promotion <= 1 ? "기본 효과" : $"{promotion} 진급 효과";
        descriptionText.text = data.description;

        bool unlocked = promotion >= data.promotionLevel;

        lockImage.gameObject.SetActive(!unlocked);

        canvasGroup.alpha = unlocked ? 1f : 0.45f;
    }
}