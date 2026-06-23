using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CommonSlotUI : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private Image frameImage;
    [SerializeField] private TMP_Text countText;
    [SerializeField] private Button button;

    public void Setup(Sprite icon, int count,bool showCount,Rarity rarity, Action onClick)
    {
        iconImage.sprite = icon;
        frameImage.sprite = GameIconDatabase.GetRarityFrame(rarity);

        countText.gameObject.SetActive(showCount);

        if (showCount)
            countText.text = count.ToString();

        button.onClick.RemoveAllListeners();

        if (onClick != null)
        {
            button.onClick.AddListener(() => { onClick(); });
        }
    }
}

