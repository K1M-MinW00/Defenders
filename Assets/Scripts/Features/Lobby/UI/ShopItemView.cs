using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopItemView : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text costText;
    [SerializeField] private TMP_Text remainText;
    [SerializeField] private Button purchaseButton;

    private ShopProductData productData;

    public void Initialize(ShopProductData product)
    {
        productData = product;

        iconImage.sprite = product.Icon;
        titleText.text = product.DisplayName;

        Refresh();

        purchaseButton.onClick.RemoveAllListeners();
        purchaseButton.onClick.AddListener(OnClickPurchase);
    }

    public void Refresh()
    {
        descriptionText.text = GetRewardText();
        costText.text = GetCostText();
        remainText.text = GetRemainText();
    }

    private void OnClickPurchase()
    {
        // ShopManager.Instance.Purchase(productData);
    }

    private string GetRewardText()
    {
        if (productData.Rewards == null || productData.Rewards.Count == 0)
            return "";

        string text = "";

        foreach (ShopRewardData reward in productData.Rewards)
        {
            text += $"{reward.RewardType} +{reward.Amount}\n";
        }

        return text.TrimEnd();
    }

    private string GetCostText()
    {
        switch (productData.PurchaseType)
        {
            case ShopPurchaseType.Advertisement:
                return "광고 보기";

            case ShopPurchaseType.InAppPurchase:
                return $"{productData.Price:N0}원";

            case ShopPurchaseType.Gem:
                return $"잼 {productData.CostAmount}";

            case ShopPurchaseType.Gold:
                return $"골드 {productData.CostAmount}";

            default:
                return "";
        }
    }

    private string GetRemainText()
    {
        switch (productData.PurchaseType)
        {
            case ShopPurchaseType.Advertisement:
                // int remain = ShopManager.Instance.GetRemainingCount(productData);
                return $"남은 횟수 {0}/{productData.DailyLimit}";

            default:
                return "";
        }
    }
}