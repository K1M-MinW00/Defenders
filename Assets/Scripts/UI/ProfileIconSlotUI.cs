using System;
using UnityEngine;
using UnityEngine.UI;

public class ProfileIconSlotUI : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private Image iconImage;
    public string IconId { get; private set; }

    private Action<string> onClick;

    public void Initialize(string iconId, Sprite iconSprite, Action<string> onClick)
    {
        IconId = iconId;
        this.onClick = onClick;

        iconImage.sprite = iconSprite;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => this.onClick?.Invoke(IconId));
    }
}