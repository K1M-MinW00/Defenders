using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class ShopPurchaseConfirmPopup : MonoBehaviour, IModalDismissHandler
{
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private Transform rewardContainer;
    [SerializeField] private ShopRewardSlotView rewardSlotPrefab;
    [SerializeField] private TMP_Text currentGemText;
    [SerializeField] private TMP_Text costText;
    [SerializeField] private TMP_Text remainingGemText;
    [SerializeField] private TMP_Text warningText;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;
    [SerializeField] private Button rechargeButton;

    private UnityAction onConfirm;
    private UnityAction onRecharge;
    private readonly List<ShopRewardSlotView> rewardSlots = new();
    private bool isBusy;
    private ShopProductData currentProduct;
    private int currentGem;
    private ModalOverlayLifecycle lifecycle;
    private ModalOverlayLifecycle Lifecycle => lifecycle ??= new ModalOverlayLifecycle(gameObject);

    private void Awake()
    {
        if (!ValidateReferences())
        {
            enabled = false;
            return;
        }

        confirmButton.onClick.AddListener(Confirm);
        cancelButton.onClick.AddListener(Dismiss);
        rechargeButton.onClick.AddListener(OpenRecharge);
    }

    public void Show(
        ShopProductData product,
        int currentGem,
        UnityAction confirm,
        UnityAction recharge)
    {
        if (product == null)
            return;

        onConfirm = confirm;
        onRecharge = recharge;
        isBusy = false;
        currentProduct = product;
        this.currentGem = currentGem;

        long remaining = (long)this.currentGem - product.CostAmount;
        bool affordable = remaining >= 0;
        RefreshLocalizedText();

        RebuildRewards(product);

        if (cancelButton != null)
        {
            cancelButton.gameObject.SetActive(true);
            cancelButton.interactable = true;
        }

        if (rechargeButton != null)
            rechargeButton.interactable = true;

        if (confirmButton != null)
        {
            confirmButton.gameObject.SetActive(affordable);
            confirmButton.interactable = affordable;
        }

        if (rechargeButton != null)
            rechargeButton.gameObject.SetActive(!affordable);

        Lifecycle.Show();
    }

    public void SetBusy(bool busy)
    {
        isBusy = busy;
        if (confirmButton != null)
            confirmButton.interactable = !busy;
        if (cancelButton != null)
            cancelButton.interactable = !busy;
        if (rechargeButton != null)
            rechargeButton.interactable = !busy;

        if (warningText != null && busy)
            warningText.text = LocalizationService.Get(LocalizationTables.Lobby, "shop.processing");
    }

    public void Dismiss()
    {
        if (isBusy)
            return;

        Lifecycle.Close(ClearActions);
    }

    public void Hide() => Lifecycle.Close(ClearActions);

    public void HideImmediately()
    {
        ClearActions();
        Lifecycle.HideImmediately();
    }

    public void Hide(UnityAction onHidden)
    {
        Lifecycle.Close(() =>
        {
            ClearActions();
            onHidden?.Invoke();
        });
    }

    private void RebuildRewards(ShopProductData product)
    {
        if (rewardContainer == null || rewardSlotPrefab == null)
            return;

        int visibleCount = 0;
        if (product.Rewards != null)
        {
            foreach (RewardData reward in product.Rewards)
            {
                if (reward == null)
                    continue;

                ShopRewardSlotView slot = GetOrCreateRewardSlot(visibleCount++);
                slot.gameObject.SetActive(true);
                slot.Bind(reward);
            }
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

    private void Confirm()
    {
        if (onConfirm != null)
            onConfirm.Invoke();
    }

    private void OpenRecharge()
    {
        UnityAction action = onRecharge;
        Lifecycle.Close(() =>
        {
            ClearActions();
            action?.Invoke();
        });
    }

    private void ClearActions()
    {
        isBusy = false;
        currentProduct = null;
        currentGem = 0;
        onConfirm = null;
        onRecharge = null;
    }

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
        if (currentProduct == null)
            return;

        titleText.text = currentProduct.DisplayName;
        currentGemText.text = LocalizationService.Get(
            LocalizationTables.Lobby,
            "shop.current_gems",
            currentGem);
        costText.text = LocalizationService.Get(
            LocalizationTables.Lobby,
            "shop.price_gems",
            currentProduct.CostAmount);

        long remaining = (long)currentGem - currentProduct.CostAmount;
        bool affordable = remaining >= 0;
        remainingGemText.text = affordable
            ? LocalizationService.Get(LocalizationTables.Lobby, "shop.remaining_gems", remaining)
            : LocalizationService.Get(LocalizationTables.Lobby, "shop.remaining_gems_insufficient");
        warningText.text = isBusy
            ? LocalizationService.Get(LocalizationTables.Lobby, "shop.processing")
            : LocalizationService.Get(
                LocalizationTables.Lobby,
                affordable ? "shop.confirm_purchase" : "shop.insufficient_gems");
    }

    private void OnDestroy()
    {
        if (confirmButton != null)
            confirmButton.onClick.RemoveListener(Confirm);
        if (cancelButton != null)
            cancelButton.onClick.RemoveListener(Dismiss);
        if (rechargeButton != null)
            rechargeButton.onClick.RemoveListener(OpenRecharge);
    }

    private bool ValidateReferences()
    {
        bool valid = titleText != null && rewardContainer != null && rewardSlotPrefab != null &&
                     currentGemText != null && costText != null && remainingGemText != null && warningText != null &&
                     confirmButton != null && cancelButton != null && rechargeButton != null;
        if (!valid)
            Debug.LogError("[ShopPurchaseConfirmPopup] Required UI references are not assigned.", this);
        return valid;
    }
}

