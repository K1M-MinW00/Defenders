using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class IdleRewardSlotView : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text amountText;

    public void Bind(IdleRewardEntry entry)
    {
        RewardData reward = new() { Type = entry.Type, Id = entry.ItemId, Amount = entry.TotalAmount };
        if (RewardPresentationResolver.TryResolve(reward, GameConfig.Icons, out RewardPresentation presentation))
        {
            icon.sprite = presentation.Icon;
            icon.enabled = presentation.Icon != null;
        }
        amountText.text = entry.TotalAmount.ToString("N0");
    }
}
