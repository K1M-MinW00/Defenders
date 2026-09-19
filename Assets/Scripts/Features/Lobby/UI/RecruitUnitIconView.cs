using UnityEngine;
using UnityEngine.UI;

public class RecruitUnitIconView : MonoBehaviour
{
    [SerializeField] private Image bgImage;
    [SerializeField] private Image iconImage;
    [SerializeField] private GameObject duplicateMark;
    [SerializeField] private UnitVisualConfigSO visualConfig;

    public void Setup(GachaResult result)
    {
        iconImage.sprite = result.Unit.icon;

        bgImage.color = visualConfig != null ? visualConfig.GetRarityColor(result.Unit.rarity) : Color.white;

        duplicateMark.SetActive(result.IsDuplicateReward);
    }

    public void Setup(UnitDataSO unit)
    {
        iconImage.sprite = unit.icon;

        bgImage.color = visualConfig != null ? visualConfig.GetRarityColor(unit.rarity) : Color.white;

        duplicateMark.SetActive(false);
    }
}
