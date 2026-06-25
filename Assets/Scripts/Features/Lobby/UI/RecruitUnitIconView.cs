using UnityEngine;
using UnityEngine.UI;

public class RecruitUnitIconView : MonoBehaviour
{
    [SerializeField] private Image bgImage;
    [SerializeField] private Image iconImage;
    [SerializeField] private GameObject duplicateMark;

    public void Setup(GachaResult result)
    {
        iconImage.sprite = result.Unit.icon;

        bgImage.color = result.Unit.rarity switch
        {
            Rarity.Legend => Color.yellow,
            Rarity.Rare => Color.blue,
            Rarity.Normal => Color.wheat,
            _ => Color.white
        };

        duplicateMark.SetActive(result.IsDuplicateReward);
    }

    public void Setup(UnitDataSO unit)
    {
        iconImage.sprite = unit.icon;

        bgImage.color = unit.rarity switch
        {
            Rarity.Legend => Color.yellow,
            Rarity.Rare => Color.blue,
            Rarity.Normal => Color.wheat,
            _ => Color.white
        };

        duplicateMark.SetActive(false);
    }
}