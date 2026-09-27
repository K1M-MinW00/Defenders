using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public sealed class ShopPurchaseConfirmPopup : MonoBehaviour
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

    private void Awake()
    {
        if (confirmButton != null)
            confirmButton.onClick.AddListener(Confirm);
        if (cancelButton != null)
            cancelButton.onClick.AddListener(Hide);
        if (rechargeButton != null)
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

        if (titleText != null)
            titleText.text = product.DisplayName;
        if (currentGemText != null)
            currentGemText.text = "보유  ◆ " + currentGem.ToString("N0");
        if (costText != null)
            costText.text = "가격  ◆ " + product.CostAmount.ToString("N0");

        long remaining = (long)currentGem - product.CostAmount;
        bool affordable = remaining >= 0;

        if (remainingGemText != null)
            remainingGemText.text = affordable
                ? "구매 후  ◆ " + remaining.ToString("N0")
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

        gameObject.SetActive(true);
        transform.SetAsLastSibling();
    }

    public void SetBusy(bool busy)
    {
        if (confirmButton != null)
            confirmButton.interactable = !busy;
        if (cancelButton != null)
            cancelButton.interactable = !busy;
        if (rechargeButton != null)
            rechargeButton.interactable = !busy;

        if (warningText != null && busy)
            warningText.text = "구매 처리 중...";
    }

    public void Hide()
    {
        onConfirm = null;
        onRecharge = null;
        gameObject.SetActive(false);
    }

    private void RebuildRewards(ShopProductData product)
    {
        if (rewardContainer == null || rewardSlotPrefab == null)
            return;

        for (int i = rewardContainer.childCount - 1; i >= 0; i--)
            Destroy(rewardContainer.GetChild(i).gameObject);

        if (product.Rewards == null)
            return;

        foreach (RewardData reward in product.Rewards)
        {
            if (reward != null)
                Instantiate(rewardSlotPrefab, rewardContainer).Bind(reward);
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
        Hide();
        if (action != null)
            action.Invoke();
    }

    private void OnDestroy()
    {
        if (confirmButton != null)
            confirmButton.onClick.RemoveListener(Confirm);
        if (cancelButton != null)
            cancelButton.onClick.RemoveListener(Hide);
        if (rechargeButton != null)
            rechargeButton.onClick.RemoveListener(OpenRecharge);
    }
}

