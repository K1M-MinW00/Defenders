using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UnitPromotionPanel : MonoBehaviour
{
    [Header("Promotion")]
    [SerializeField] private Image promotionImage;
    [SerializeField] private Sprite[] promotion_sprites;
    [SerializeField] private TMP_Text promotionText;

    [Header("Upgrades")]
    [SerializeField] private GameObject[] upgrade_Entries;

    [Header("Materials")]
    [SerializeField] private Transform materialRoot;
    [SerializeField] private CommonSlotUI materialSlotPrefab;

    [Header("Button")]
    [SerializeField] private Button promotionButton;

    private UnitDataSO currentUnitData;
    private UserUnitData currentUnit;
    private UnitDetailView detailPanel;
    private bool isPromoting;

    private void Awake()
    {
        promotionButton.onClick.AddListener(OnClickPromotion);
    }

    public void Bind(UnitDataSO unitData, UnitDetailView panel)
    {
        currentUnitData = unitData;
        currentUnit = UserDataManager.Instance.RosterService.GetUnit(unitData.unitId);

        detailPanel = panel;

        Refresh();
    }

    private void Refresh()
    {
        RefreshPromotionInfo();
        RefreshEffects();
        RefreshMaterials();
    }

    private void RefreshPromotionInfo()
    {
        int promotion = currentUnit.Promotion;

        promotionText.text = $"{promotion}진급";

        if(promotion_sprites != null && promotion < promotion_sprites.Length)
            promotionImage.sprite = promotion_sprites[promotion];

    }

    private void RefreshEffects()
    {
        for (int i = 0; i < upgrade_Entries.Length; i++)
        {
            bool locked = i >= currentUnit.Promotion;

            upgrade_Entries[i].SetActive(locked);
        }
    }

    private void RefreshMaterials()
    {
        foreach (Transform child in materialRoot)
            Destroy(child.gameObject);

        if (currentUnitData.promotionCost == null ||
            currentUnit.Promotion >= currentUnitData.promotionCost.Length)
        {
            promotionButton.gameObject.SetActive(false);
            return;
        }

        promotionButton.gameObject.SetActive(true);

        PromotionCost cost = currentUnitData.promotionCost[currentUnit.Promotion];

        MaterialDataSO material = ItemDatabase.Get(cost.MaterialId) as MaterialDataSO;

        int owned = UserDataManager.Instance.InventoryService.GetItemCount(material.ItemId);

        CommonSlotUI slot = Instantiate(materialSlotPrefab, materialRoot);
        slot.Setup(material.Icon, owned, true, currentUnitData.rarity, null);

        promotionButton.interactable = owned >= cost.Count;
    }

    private async void OnClickPromotion()
    {
        if (isPromoting || currentUnitData == null)
            return;

        isPromoting = true;
        promotionButton.interactable = false;

        try
        {
            PromoteUnitResult result = await UserDataManager.Instance.UnitPromotionUseCase.ExecuteAsync(
                new PromoteUnitCommand(currentUnitData.unitId));

            if (!result.Succeeded)
            {
                Debug.LogWarning($"[UnitPromotionPanel] Promotion failed: {result.Failure}");
                return;
            }

            currentUnit = UserDataManager.Instance.RosterService.GetUnit(currentUnitData.unitId);
            Refresh();
            detailPanel.Refresh();
        }
        finally
        {
            isPromoting = false;

            if (promotionButton.gameObject.activeSelf)
                RefreshMaterials();
        }
    }
}
