using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlotUI : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text countText;
    [SerializeField] private Button button;

    public void Setup(ItemDataSO itemData,int count,Action<ItemDataSO, int> onClick)
    {
        iconImage.sprite = itemData.Icon;

        if (itemData.Stackable)
        {
            countText.gameObject.SetActive(true);
            countText.text = count.ToString();
        }
        else
        {
            countText.gameObject.SetActive(false);
        }

        button.onClick.RemoveAllListeners();

        button.onClick.AddListener(() =>
        {
            onClick?.Invoke(itemData, count);
        });
    }
}