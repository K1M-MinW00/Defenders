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
    private StagePreparationService preparationService;
    private TextMeshProUGUI rerollCostText;
    private bool isDraggingUnit;
    private bool draggedUnitCanReroll;
    private int draggedUnitStar = 1;

    public RectTransform RerollTarget => rerollZone != null
        ? rerollZone.transform as RectTransform
        : null;

    public void Initialize(EconomyManager economy, StagePreparationService preparationService)
    {
        if (this.preparationService != null)
        {
            this.preparationService.OnFreeRerollsChanged -= HandleFreeRerollsChanged;
            this.preparationService.AllowedActionsChanged -= HandleAllowedActionsChanged;
        }

        this.economy = economy;
        this.preparationService = preparationService;
        rerollCostText = FindRerollCostText();
        if (this.preparationService != null)
        {
            this.preparationService.OnFreeRerollsChanged += HandleFreeRerollsChanged;
            this.preparationService.AllowedActionsChanged += HandleAllowedActionsChanged;
        }
        SetDragMode(false);
    }

    public void SetDragMode(bool isDraggingUnit, bool canReroll = true, int star = 1)
    {
        this.isDraggingUnit = isDraggingUnit;
        draggedUnitCanReroll = canReroll;
        draggedUnitStar = star;

        defaultButtonsGroup?.SetActive(!isDraggingUnit);
        unitActionButtonsGroup?.SetActive(isDraggingUnit);

        StagePreparationAction actions = preparationService?.AllowedActions ?? StagePreparationAction.All;
        bool showRerollZone = isDraggingUnit && canReroll &&
                              (actions & StagePreparationAction.Reroll) != 0;
        bool showSellZone = isDraggingUnit && (actions & StagePreparationAction.Sell) != 0;

        rerollZone?.SetActive(showRerollZone);
        sellZone?.SetActive(showSellZone);

        if (sellCostText != null && economy != null)
            sellCostText.SetText("+{0}", economy.GetSellCost(star));

        RefreshRerollCost();
    }

    private void HandleFreeRerollsChanged(int _)
    {
        RefreshRerollCost();
    }

    private void HandleAllowedActionsChanged(StagePreparationAction _)
    {
        SetDragMode(isDraggingUnit, draggedUnitCanReroll, draggedUnitStar);
    }

    private void RefreshRerollCost()
    {
        if (rerollCostText == null || economy == null)
            return;

        int freeRemaining = preparationService?.FreeRerollsRemaining ?? 0;
        rerollCostText.text = freeRemaining > 0
            ? LocalizationService.Get(LocalizationTables.Stage, "hud.free_count", freeRemaining)
            : economy.GetRerollCost().ToString();
    }

    private TextMeshProUGUI FindRerollCostText()
    {
        if (rerollZone == null)
            return null;

        TextMeshProUGUI[] texts = rerollZone.GetComponentsInChildren<TextMeshProUGUI>(true);
        foreach (TextMeshProUGUI text in texts)
        {
            if (text != null && text.name.Contains("Price"))
                return text;
        }

        return texts.Length > 1 ? texts[^1] : null;
    }

    private void OnDestroy()
    {
        if (preparationService != null)
        {
            preparationService.OnFreeRerollsChanged -= HandleFreeRerollsChanged;
            preparationService.AllowedActionsChanged -= HandleAllowedActionsChanged;
        }
    }
}
