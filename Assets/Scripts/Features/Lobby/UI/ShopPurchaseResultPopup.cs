using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class ShopPurchaseResultPopup : MonoBehaviour
{
    [SerializeField] private TMP_Text resultTitleText;
    [SerializeField] private TMP_Text productNameText;
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private Transform rewardContainer;
    [SerializeField] private ShopRewardSlotView rewardSlotPrefab;
    [SerializeField] private Button closeButton;

    private void Awake()
    {
        if (closeButton != null)
            closeButton.onClick.AddListener(Hide);
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

        gameObject.SetActive(true);
        transform.SetAsLastSibling();
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private void RebuildRewards(ShopProductData product)
    {
        if (rewardContainer == null || rewardSlotPrefab == null)
            return;

        for (int i = rewardContainer.childCount - 1; i >= 0; i--)
            Destroy(rewardContainer.GetChild(i).gameObject);

        if (product == null || product.Rewards == null)
        {
            rewardContainer.gameObject.SetActive(false);
            return;
        }

        rewardContainer.gameObject.SetActive(true);
        foreach (RewardData reward in product.Rewards)
        {
            if (reward != null)
                Instantiate(rewardSlotPrefab, rewardContainer).Bind(reward);
        }
    }

    private void OnDestroy()
    {
        if (closeButton != null)
            closeButton.onClick.RemoveListener(Hide);
    }
}
