using UnityEngine;
using UnityEngine.UI;

public class RecruitUnitIconView : MonoBehaviour
{
    [SerializeField] private Image bgImage;
    [SerializeField] private Image iconImage;

    public void Setup(UnitDataSO unit)
    {
        iconImage.sprite = unit.icon;

        bgImage.color = unit.rarity switch
        {
            Rarity.Legend => Color.yellow,
            Rarity.Rare => Color.blue,
            Rarity.Normal => Color.brown,
            _ => Color.white
        };
    }
}