using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class ShopPurchaseResultPopup : MonoBehaviour, IModalDismissHandler
{
    [SerializeField] private TMP_Text resultTitleText;
    [SerializeField] private TMP_Text productNameText;
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private Transform rewardContainer;
    [SerializeField] private ShopRewardSlotView rewardSlotPrefab;
    [SerializeField] private Button closeButton;
    private readonly List<ShopRewardSlotView> rewardSlots = new();
    private ShopProductData currentProduct;
    private ShopPurchaseResult currentResult;

    private ModalOverlayLifecycle lifecycle;
    private ModalOverlayLifecycle Lifecycle => lifecycle ??= new ModalOverlayLifecycle(gameObject);

    private void Awake()
    {
        if (!ValidateReferences())
        {
            enabled = false;
            return;
        }

        closeButton.onClick.AddListener(Dismiss);
    }

    public void Show(ShopProductData product, ShopPurchaseResult result)
    {
        currentProduct = product;
        currentResult = result;
        bool succeeded = result != null && result.Succeeded;
        RefreshLocalizedText();

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

    public void HideImmediately() => Lifecycle.HideImmediately();

    private void OnEnable()
    {
        LocalizationService.LocaleChanged += HandleLocaleChanged;
    }

    private void OnDisable()
    {
        LocalizationService.LocaleChanged -= HandleLocaleChanged;
    }

    private void HandleLocaleChanged(string _) => RefreshLocalizedText();

    private void RefreshLocalizedText()
    {
        if (currentResult == null)
            return;

        bool succeeded = currentResult.Succeeded;
        resultTitleText.text = LocalizationService.Get(
            LocalizationTables.Lobby,
            succeeded ? "shop.result_success_title" : "shop.result_failure_title");
        resultTitleText.color = succeeded
            ? new Color32(255, 229, 76, 255)
            : new Color32(255, 120, 120, 255);
        productNameText.text = currentProduct != null ? currentProduct.DisplayName : string.Empty;
        messageText.text = ShopFeedbackMessages.Get(currentResult);
    }

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

    private bool ValidateReferences()
    {
        bool valid = resultTitleText != null && productNameText != null && messageText != null &&
                     rewardContainer != null && rewardSlotPrefab != null && closeButton != null;
        if (!valid)
            Debug.LogError("[ShopPurchaseResultPopup] Required UI references are not assigned.", this);
        return valid;
    }
}
