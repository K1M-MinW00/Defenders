using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public abstract class ShopProductView : MonoBehaviour
{
    [Header("Common UI")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text costText;
    [SerializeField] private TMP_Text remainText;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private Button purchaseButton;

    protected ShopProductData Product { get; private set; }
    public ShopProductData ProductData => Product;

    private UnityAction<ShopProductView> onPurchaseRequested;
    private readonly List<ShopRewardSlotView> rewardSlots = new();

    public void Initialize(ShopProductData product, UnityAction<ShopProductView> purchaseRequested = null)
    {
        Product = product;
        onPurchaseRequested = purchaseRequested;

        if (titleText != null)
            titleText.text = product != null ? product.DisplayName : string.Empty;
        if (costText != null)
            costText.text = BuildCostText();

        BindProduct(product);

        if (purchaseButton != null)
        {
            purchaseButton.onClick.RemoveListener(OnClickPurchase);
            purchaseButton.onClick.AddListener(OnClickPurchase);
        }

        RefreshState();
    }

    public void RefreshState()
    {
        if (Product == null)
            return;

        ShopProductState state = GetState();

        if (remainText != null)
            remainText.text = Product.PurchaseLimit <= 0
                ? string.Empty
                : $"구매 가능 {state.RemainingCount}/{Product.PurchaseLimit}";

        RefreshTimer(state);

        if (statusText != null)
            statusText.text = !Product.IsGemProduct ? "준비 중" :
                state.IsSoldOut ? "품절" : state.IsOnSale ? string.Empty : "판매 종료";

        if (purchaseButton != null)
            purchaseButton.interactable =
                Product.IsGemProduct && state.IsOnSale && !state.IsSoldOut;
    }

    public void RefreshTimer()
    {
        if (Product != null)
            RefreshTimer(GetState());
    }

    protected abstract void BindProduct(ShopProductData product);

    protected static void ApplySprite(Image target, Sprite sprite)
    {
        if (target == null)
            return;

        target.sprite = sprite;
        target.enabled = true;
        target.color = sprite != null ? Color.white : new Color32(255, 255, 255, 45);
    }

    protected static Sprite ResolvePrimaryRewardIcon(ShopProductData product)
    {
        if (product == null || product.Rewards == null || product.Rewards.Count == 0)
            return null;

        RewardData reward = product.Rewards[0];
        return RewardPresentationResolver.TryResolve(
            reward,
            GameConfig.Icons,
            out RewardPresentation presentation)
            ? presentation.Icon
            : null;
    }

    protected void RebuildRewardSlots(
        ShopProductData product,
        Transform container,
        ShopRewardSlotView prefab)
    {
        if (container == null || prefab == null)
            return;

        int visibleCount = 0;
        if (product?.Rewards != null)
        {
            foreach (RewardData reward in product.Rewards)
            {
                if (reward == null)
                    continue;

                ShopRewardSlotView slot = GetOrCreateRewardSlot(visibleCount++, container, prefab);
                slot.gameObject.SetActive(true);
                slot.Bind(reward);
            }
        }

        HideRewardSlotsFrom(visibleCount);
    }

    private ShopRewardSlotView GetOrCreateRewardSlot(
        int index,
        Transform container,
        ShopRewardSlotView prefab)
    {
        if (index < rewardSlots.Count)
            return rewardSlots[index];

        ShopRewardSlotView slot = Instantiate(prefab, container);
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

    private ShopProductState GetState()
    {
        return UserDataManager.Instance != null
            ? UserDataManager.Instance.GetShopProductState(Product)
            : new ShopProductState(false, false, 0, 0, null);
    }

    private string BuildCostText()
    {
        if (Product == null)
            return string.Empty;

        return Product.PurchaseType switch
        {
            ShopPurchaseType.Gem => $"Gem {Product.CostAmount:N0}",
            ShopPurchaseType.InAppPurchase => $"{Product.Price:N0}원",
            ShopPurchaseType.Advertisement => "광고 보기",
            ShopPurchaseType.Gold => $"골드 {Product.CostAmount:N0}",
            _ => string.Empty,
        };
    }

    private void RefreshTimer(ShopProductState state)
    {
        if (timerText == null)
            return;

        if (!state.NextResetUtc.HasValue)
        {
            timerText.text = string.Empty;
            return;
        }

        TimeSpan remain = state.NextResetUtc.Value - DateTimeOffset.UtcNow;
        timerText.text = remain <= TimeSpan.Zero
            ? "재고 갱신 중"
            : remain.TotalDays >= 1
                ? $"{(int)remain.TotalDays}일 {remain.Hours}시간"
                : $"{remain.Hours:00}:{remain.Minutes:00}:{remain.Seconds:00}";
    }

    private void OnClickPurchase()
    {
        if (Product != null && onPurchaseRequested != null)
            onPurchaseRequested.Invoke(this);
    }
    private void OnDestroy()
    {
        if (purchaseButton != null)
            purchaseButton.onClick.RemoveListener(OnClickPurchase);
    }
}


