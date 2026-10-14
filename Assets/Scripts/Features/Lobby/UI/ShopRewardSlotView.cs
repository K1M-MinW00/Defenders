using UnityEngine;

[DisallowMultipleComponent]
public sealed class ShopRewardSlotView : MonoBehaviour
{
    [SerializeField] private CommonSlotUI commonSlot;

    private void Awake()
    {
        if (commonSlot == null)
        {
            Debug.LogError("[ShopRewardSlotView] Common slot is not assigned.", this);
            enabled = false;
        }
    }

    public void Bind(RewardData reward)
    {
        if (commonSlot == null)
            return;

        bool resolved = RewardPresentationResolver.TryResolve(
            reward,
            GameConfig.Icons,
            out RewardPresentation presentation);

        if (resolved)
            commonSlot.SetupPresentation(
                presentation,
                reward.Amount,
                () => ItemTooltipUI.Show(presentation.DisplayName, transform as RectTransform));
        else
            commonSlot.Clear();
    }

    public void Clear()
    {
        commonSlot?.Clear();
    }
}

