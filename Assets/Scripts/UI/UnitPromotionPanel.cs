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
    [SerializeField] private Transform effectRoot;
    [SerializeField] private GameObject effectTemplate;

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
        if (promotionButton != null)
            promotionButton.onClick.AddListener(OnClickPromotion);
    }

    public void Bind(UnitDataSO unitData, UnitDetailView panel)
    {
        currentUnitData = unitData;
        detailPanel = panel;

        Refresh();
    }

    private void Refresh()
    {
        if (currentUnitData == null || UserDataManager.Instance == null)
            return;

        currentUnit = UserDataManager.Instance.RosterService.GetUnit(currentUnitData.unitId);

        if (currentUnit == null)
        {
            Debug.LogWarning($"[UnitPromotionPanel] Owned unit data not found: {currentUnitData.unitId}");
            return;
        }

        RefreshPromotionInfo();
        RefreshEffects();
        RefreshMaterials();
    }

    private void RefreshPromotionInfo()
    {
        int promotion = currentUnit.Promotion;

        if (promotionText != null)
            promotionText.text = $"{promotion}진급";

        if (promotionImage != null && promotion_sprites != null &&
            promotion >= 0 && promotion < promotion_sprites.Length)
        {
            promotionImage.sprite = promotion_sprites[promotion];
        }

    }

    private void RefreshEffects()
    {
        if (effectRoot == null || effectTemplate == null)
            return;

        PromotionProgressionSO progression = PromotionProgressionDatabase.Get();

        if (progression == null)
        {
            Debug.LogWarning("[UnitPromotionPanel] Promotion progression data is missing.");
            return;
        }

        EnsureEffectEntryCount(progression.Stages.Count);

        for (int i = 0; i < effectRoot.childCount; i++)
        {
            GameObject entry = effectRoot.GetChild(i).gameObject;

            if (i >= progression.Stages.Count || progression.Stages[i] == null)
            {
                entry.SetActive(false);
                continue;
            }

            PromotionStageData stage = progression.Stages[i];
            entry.SetActive(true);

            TMP_Text descriptionText = entry.GetComponentInChildren<TMP_Text>(true);
            if (descriptionText != null)
                descriptionText.text = stage.description;

            Transform lockObject = entry.transform.Find("locked_Img");
            if (lockObject != null)
                lockObject.gameObject.SetActive(currentUnit.Promotion < stage.promotionLevel);
        }
    }

    private void EnsureEffectEntryCount(int requiredCount)
    {
        while (effectRoot.childCount < requiredCount)
            Instantiate(effectTemplate, effectRoot);
    }

    private void RefreshMaterials()
    {
        if (materialRoot != null)
        {
            foreach (Transform child in materialRoot)
                Destroy(child.gameObject);
        }

        if (currentUnitData.promotionCost == null ||
            currentUnit.Promotion >= currentUnitData.promotionCost.Length)
        {
            promotionButton?.gameObject.SetActive(false);
            return;
        }

        if (promotionButton == null || materialRoot == null || materialSlotPrefab == null)
            return;

        promotionButton.gameObject.SetActive(true);

        PromotionCost cost = currentUnitData.promotionCost[currentUnit.Promotion];

        if (cost == null || string.IsNullOrWhiteSpace(cost.MaterialId) || cost.Count <= 0)
        {
            Debug.LogWarning($"[UnitPromotionPanel] Invalid promotion cost at level {currentUnit.Promotion}.");
            promotionButton.interactable = false;
            return;
        }

        MaterialDataSO material = ItemDatabase.Get(cost.MaterialId) as MaterialDataSO;

        if (material == null)
        {
            Debug.LogWarning($"[UnitPromotionPanel] Promotion material not found: {cost.MaterialId}");
            promotionButton.interactable = false;
            return;
        }

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
        if (promotionButton != null)
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

            Refresh();
            detailPanel?.Refresh();
            UserDataManager.Instance.RaiseRosterUpdated();
        }
        finally
        {
            isPromoting = false;

            if (promotionButton != null && promotionButton.gameObject.activeSelf)
                RefreshMaterials();
        }
    }

    private void OnDestroy()
    {
        if (promotionButton != null)
            promotionButton.onClick.RemoveListener(OnClickPromotion);
    }
}
