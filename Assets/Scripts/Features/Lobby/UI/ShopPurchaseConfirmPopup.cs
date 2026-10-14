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

        if (titleText != null)
            titleText.text = product.DisplayName;
        if (currentGemText != null)
            currentGemText.text = "보유 Gem " + currentGem.ToString("N0");
        if (costText != null)
            costText.text = "가격 Gem " + product.CostAmount.ToString("N0");

        long remaining = (long)currentGem - product.CostAmount;
        bool affordable = remaining >= 0;

        if (remainingGemText != null)
            remainingGemText.text = affordable
                ? "구매 후 Gem " + remaining.ToString("N0")
                : "구매 후  Gem 부족";
        if (warningText != null)
            warningText.text = affordable ? "이 상품을 구매하시겠습니까?" : "Gem이 부족합니다.";

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
            warningText.text = "구매 처리 중";
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
        onConfirm = null;
        onRecharge = null;
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

