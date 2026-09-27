using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class ShopRewardSlotView : MonoBehaviour
{
    [SerializeField] private Image frameImage;
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text typeText;
    [SerializeField] private TMP_Text amountText;

    public void Bind(RewardData reward)
    {
        bool resolved = RewardPresentationResolver.TryResolve(
            reward,
            GameConfig.Icons,
            out RewardPresentation presentation);

        if (frameImage != null && resolved && presentation.Frame != null)
            frameImage.sprite = presentation.Frame;

        if (iconImage != null)
        {
            Sprite icon = resolved ? presentation.Icon : null;
            iconImage.sprite = icon;
            iconImage.enabled = true;
            iconImage.color = icon != null ? Color.white : new Color32(255, 255, 255, 70);
        }

        if (amountText != null)
            amountText.text = reward == null ? string.Empty : reward.Amount.ToString("N0");

        if (typeText != null)
            typeText.text = resolved ? presentation.DisplayName : reward?.Type.ToString() ?? string.Empty;
    }
}

