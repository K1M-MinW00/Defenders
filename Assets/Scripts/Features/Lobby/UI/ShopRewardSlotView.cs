using UnityEngine;

public sealed class ShopRewardSlotView : MonoBehaviour
{
    [SerializeField] private CommonSlotUI commonSlot;

    public void Bind(RewardData reward)
    {
        bool resolved = RewardPresentationResolver.TryResolve(reward,GameConfig.Icons,out RewardPresentation presentation);

        if (commonSlot != null)
        {
            if (resolved)
                commonSlot.SetupPresentation(
                    presentation,
                    reward.Amount,
                    () => ItemTooltipUI.Show(presentation.DisplayName, transform as RectTransform));
            else
                commonSlot.Clear();
        }
    }

    public void Clear()
    {
        commonSlot?.Clear();
    }
}

