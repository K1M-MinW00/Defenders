using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class RechargeProductView : ShopProductView
{
    [Header("Recharge UI")]
    [SerializeField] private Image gemImage;
    [SerializeField] private GameObject bonusPanel;
    [SerializeField] private TMP_Text bonusText;

    protected override void BindProduct(ShopProductData product)
    {
        RechargeShopProductData recharge = product as RechargeShopProductData;
        if (recharge == null)
        {
            Debug.LogError($"[{nameof(RechargeProductView)}] RechargeShopProductData가 필요합니다.", this);
            return;
        }

        Sprite image = recharge.GemImage != null
            ? recharge.GemImage
            : ResolvePrimaryRewardIcon(recharge);
        ApplySprite(gemImage, image);

        if (bonusPanel != null)
            bonusPanel.SetActive(recharge.HasFirstPurchaseBonus);
        if (bonusText != null)
            bonusText.text = recharge.FirstPurchaseBonusLabel;
    }
}
