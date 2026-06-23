using TMPro;
using UnityEngine;

public class UnitDragActionUI : MonoBehaviour
{
    [SerializeField] private GameObject defaultButtonsGroup;
    [SerializeField] private GameObject unitActionButtonsGroup;

    [SerializeField] private GameObject rerollZone;
    [SerializeField] private GameObject sellZone;

    [SerializeField] private TextMeshProUGUI sellCostText;

    private EconomyManager economy;

    public void Initialize(EconomyManager economy)
    {
        this.economy = economy;
        SetDragMode(false);
    }

    public void SetDragMode(bool isDraggingUnit, bool canReroll = true, int star = 1)
    {
        defaultButtonsGroup?.SetActive(!isDraggingUnit);
        unitActionButtonsGroup?.SetActive(isDraggingUnit);

        bool showRerollZone = isDraggingUnit && canReroll;

        rerollZone?.SetActive(showRerollZone);
        sellZone?.SetActive(isDraggingUnit);

        if (sellCostText != null && economy != null)
            sellCostText.SetText("{0}", economy.GetSellCost(star));
    }
}