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

    private void Awake()
    {
        promotionButton.onClick.AddListener(OnClickPromotion);
    }

    public void Bind(UnitDataSO unitData, UnitDetailView panel)
    {
        currentUnitData = unitData;
        currentUnit = UserDataManager.Instance.UserData.Roster.GetOwnedUnit(unitData.unitId);

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

        if (currentUnit.Promotion >= 4)
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
        bool success = UserDataManager.Instance.RosterService.TryPromotion(currentUnitData);

        if (!success)
            return;

        await UserDataManager.Instance.SaveAsync();

        Refresh();
        detailPanel.Refresh();
    }
}