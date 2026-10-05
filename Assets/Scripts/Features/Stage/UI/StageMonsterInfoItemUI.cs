using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class StageMonsterInfoItemUI : MonoBehaviour
{
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private Image portraitImage;

    public void Bind(MonsterDataSO data)
    {
        if (data == null)
            return;

        if (nameText != null)
            nameText.text = string.IsNullOrWhiteSpace(data.displayName) ? data.name : data.displayName;

        if (descriptionText != null)
        {
            descriptionText.text = string.IsNullOrWhiteSpace(data.description)
                ? "설명이 준비 중입니다."
                : data.description;
        }

        if (portraitImage != null)
        {
            portraitImage.sprite = data.portrait;
            portraitImage.color = data.portrait != null ? Color.white : Color.clear;
            portraitImage.preserveAspect = true;
        }
    }
}
