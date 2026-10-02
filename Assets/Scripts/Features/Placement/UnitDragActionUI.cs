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

    public void Initialize(EconomyManager economy, StagePreparationService preparationService)
    {
        if (this.preparationService != null)
            this.preparationService.OnFreeRerollsChanged -= HandleFreeRerollsChanged;

        this.economy = economy;
        this.preparationService = preparationService;
        rerollCostText = FindRerollCostText();
        if (this.preparationService != null)
            this.preparationService.OnFreeRerollsChanged += HandleFreeRerollsChanged;
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

        RefreshRerollCost();
    }

    private void HandleFreeRerollsChanged(int _)
    {
        RefreshRerollCost();
    }

    private void RefreshRerollCost()
    {
        if (rerollCostText == null || economy == null)
            return;

        int freeRemaining = preparationService?.FreeRerollsRemaining ?? 0;
        rerollCostText.text = freeRemaining > 0
            ? $"무료 ({freeRemaining}회)"
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
            preparationService.OnFreeRerollsChanged -= HandleFreeRerollsChanged;
    }
}
