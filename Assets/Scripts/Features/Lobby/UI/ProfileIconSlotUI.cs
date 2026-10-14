using System;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class ProfileIconSlotUI : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private Image iconImage;
    public string IconId { get; private set; }

    private Action<string> onClick;

    private void Awake()
    {
        if (button != null)
            button.onClick.AddListener(HandleClicked);
    }

    public void Initialize(string iconId, Sprite iconSprite, Action<string> onClick)
    {
        IconId = iconId;
        this.onClick = onClick;

        iconImage.sprite = iconSprite;
        iconImage.enabled = iconSprite != null;
    }

    public void Clear()
    {
        IconId = string.Empty;
        onClick = null;
        if (iconImage != null)
        {
            iconImage.sprite = null;
            iconImage.enabled = false;
        }
    }

    private void HandleClicked() => onClick?.Invoke(IconId);

    private void OnDestroy()
    {
        if (button != null)
            button.onClick.RemoveListener(HandleClicked);
    }
}
