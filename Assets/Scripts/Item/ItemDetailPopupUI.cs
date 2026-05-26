using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemDetailPopupUI : MonoBehaviour
{
    [SerializeField] private Image iconImage;

    [SerializeField] private TMP_Text nameText;

    [SerializeField] private TMP_Text countText;

    [SerializeField] private TMP_Text descriptionText;

    public void Show(ItemDataSO itemData, int count)
    {
        gameObject.SetActive(true);
        iconImage.sprite = itemData.Icon;
        nameText.text = itemData.ItemName;
        countText.text = itemData.Stackable ? $"보유 : {count}": "";
        descriptionText.text = itemData.Description;
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}