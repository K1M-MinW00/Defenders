using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemDetailPanelView : MonoBehaviour
{
    [Header("Item")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text countText;
    [SerializeField] private TMP_Text descriptionText;

    public void Show(ItemDataSO itemData, int count)
    {
        if (itemData == null)
            return;

        gameObject.SetActive(true);
        
        iconImage.sprite = itemData.Icon;
        nameText.text = itemData.ItemName;
        descriptionText.text = itemData.Description;

        countText.text = itemData.Stackable ? $"보유 : {count}": "";
    }

    public void Hide()
    {
        iconImage.sprite = null;
        nameText.text = string.Empty;
        countText.text = string.Empty;
        descriptionText.text = string.Empty;

        gameObject.SetActive(false);
    }
}