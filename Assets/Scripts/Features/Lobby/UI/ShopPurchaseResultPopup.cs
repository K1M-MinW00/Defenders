using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class ShopPurchaseResultPopup : MonoBehaviour, IModalDismissHandler
{
    [SerializeField] private TMP_Text resultTitleText;
    [SerializeField] private TMP_Text productNameText;
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private Transform rewardContainer;
    [SerializeField] private ShopRewardSlotView rewardSlotPrefab;
    [SerializeField] private Button closeButton;
    private readonly List<ShopRewardSlotView> rewardSlots = new();

    private ModalOverlayLifecycle lifecycle;
    private ModalOverlayLifecycle Lifecycle => lifecycle ??= new ModalOverlayLifecycle(gameObject);

    private void Awake()
    {
        if (closeButton != null)
            closeButton.onClick.AddListener(Dismiss);
    }

    public void Show(ShopProductData product, ShopPurchaseResult result)
    {
        bool succeeded = result != null && result.Succeeded;

        if (resultTitleText != null)
        {
            resultTitleText.text = succeeded ? "구매 완료!" : "구매 실패";
            resultTitleText.color = succeeded
                ? new Color32(255, 229, 76, 255)
                : new Color32(255, 120, 120, 255);
        }

        if (productNameText != null)
            productNameText.text = product != null ? product.DisplayName : string.Empty;
        if (messageText != null)
            messageText.text = ShopFeedbackMessages.Get(result);

        RebuildRewards(succeeded ? product : null);

        if (closeButton != null)
            closeButton.interactable = true;

        Lifecycle.Show();
    }

    public void Dismiss()
    {
        Lifecycle.Close();
    }

    public void Hide() => Dismiss();

    private void RebuildRewards(ShopProductData product)
    {
        if (rewardContainer == null || rewardSlotPrefab == null)
            return;

        if (product == null || product.Rewards == null)
        {
            HideRewardSlotsFrom(0);
            rewardContainer.gameObject.SetActive(false);
            return;
        }

        rewardContainer.gameObject.SetActive(true);
        int visibleCount = 0;
        foreach (RewardData reward in product.Rewards)
        {
            if (reward == null)
                continue;

            ShopRewardSlotView slot = GetOrCreateRewardSlot(visibleCount++);
            slot.gameObject.SetActive(true);
            slot.Bind(reward);
        }
        HideRewardSlotsFrom(visibleCount);
    }

    private ShopRewardSlotView GetOrCreateRewardSlot(int index)
    {
        if (index < rewardSlots.Count)
            return rewardSlots[index];

        ShopRewardSlotView slot = Instantiate(rewardSlotPrefab, rewardContainer);
        rewardSlots.Add(slot);
        return slot;
    }

    private void HideRewardSlotsFrom(int firstHiddenIndex)
    {
        for (int i = firstHiddenIndex; i < rewardSlots.Count; i++)
        {
            rewardSlots[i].Clear();
            rewardSlots[i].gameObject.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        if (closeButton != null)
            closeButton.onClick.RemoveListener(Dismiss);
    }
}
