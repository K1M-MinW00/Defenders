using UnityEngine;

public sealed class IdleRewardSlotView : MonoBehaviour
{
    [SerializeField] private CommonSlotUI commonSlot;

    public void Bind(IdleRewardEntry entry)
    {
        RewardData reward = new() { Type = entry.Type, Id = entry.ItemId, Amount = entry.TotalAmount };
        if (RewardPresentationResolver.TryResolve(reward, GameConfig.Icons, out RewardPresentation presentation))
        {
            commonSlot.SetupPresentation(
                presentation,
                entry.TotalAmount,
                () => ItemTooltipUI.Show(presentation.DisplayName, transform as RectTransform));
        }
        else
        {
            commonSlot?.Clear();
        }
    }
}
