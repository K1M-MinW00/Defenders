using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillUpgradeSlot : MonoBehaviour
{
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private Image lockImage;
    [SerializeField] private CanvasGroup canvasGroup;

    public void Setup(int promotion, SkillUpgradeData data, string localizedDescription)
    {
        titleText.text = data.promotionLevel <= 1
            ? LocalizationService.Get(LocalizationTables.Skills, "skill.effect.base")
            : LocalizationService.Get(LocalizationTables.Skills, "skill.effect.promotion", data.promotionLevel);
        descriptionText.text = localizedDescription;

        bool unlocked = promotion >= data.promotionLevel;

        lockImage.gameObject.SetActive(!unlocked);

        canvasGroup.alpha = unlocked ? 1f : 0.45f;
    }
}
