using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class StageRelicUI : MonoBehaviour
{
    [Header("Owned relics")]
    [SerializeField] private RectTransform ownedList;
    [SerializeField] private StageRelicIconView ownedIconPrefab;
    [SerializeField] private GameObject tooltip;
    [SerializeField] private TMP_Text tooltipName;
    [SerializeField] private TMP_Text tooltipDescription;

    [Header("Choice overlay")]
    [SerializeField] private GameObject choiceOverlay;
    [SerializeField] private GameObject firstChoiceRoot;
    [SerializeField] private TMP_Text firstChoiceName;
    [SerializeField] private Image firstChoiceSymbol;
    [SerializeField] private TMP_Text firstChoiceDescription;
    [SerializeField] private Button firstChoiceButton;
    [SerializeField] private GameObject secondChoiceRoot;
    [SerializeField] private TMP_Text secondChoiceName;
    [SerializeField] private Image secondChoiceSymbol;
    [SerializeField] private TMP_Text secondChoiceDescription;
    [SerializeField] private Button secondChoiceButton;

    private bool initialized;
    private StageRelicDefinition firstChoice;
    private StageRelicDefinition secondChoice;
    private TaskCompletionSource<StageRelicDefinition> choiceCompletion;
    private Transform tooltipHomeParent;
    private readonly List<StageRelicIconView> ownedIconPool = new();
    private StageRelicDefinition tooltipRelic;
    public event Action<bool> ChoiceVisibilityChanged;
    public bool IsConfigured => ownedList != null && ownedIconPrefab != null &&
                                choiceOverlay != null && firstChoiceButton != null && secondChoiceButton != null;

    public void Initialize()
    {
        if (initialized)
            return;

        initialized = true;
        if (tooltip != null)
            tooltipHomeParent = tooltip.transform.parent;
        firstChoiceButton?.onClick.AddListener(SelectFirstChoice);
        secondChoiceButton?.onClick.AddListener(SelectSecondChoice);
        HideTooltip();
        if (choiceOverlay != null)
            choiceOverlay.SetActive(false);
    }

    private void OnEnable()
    {
        LocalizationService.LocaleChanged += HandleLocaleChanged;
    }

    public void SetOwnedHudVisible(bool visible)
    {
        Initialize();
        if (ownedList != null)
            ownedList.gameObject.SetActive(visible);

        if (!visible)
            HideTooltip();
    }

    public void SetOwnedRelics(IReadOnlyList<StageRelicDefinition> relics)
    {
        Initialize();
        if (ownedList == null)
            return;

        HideTooltip();
        HideOwnedIcons();

        if (relics == null)
            return;

        for (int index = 0; index < relics.Count; index++)
        {
            StageRelicDefinition relic = relics[index];
            if (relic == null)
                continue;

            StageRelicIconView icon = GetOrCreateOwnedIcon(index);
            icon.Bind(
                relic,
                () => ShowTooltip(relic, icon.transform as RectTransform),
                HideTooltip);
        }
    }

    public Task<StageRelicDefinition> ShowChoiceAsync(
        StageRelicDefinition first,
        StageRelicDefinition second)
    {
        Initialize();
        firstChoice = first;
        secondChoice = second;
        ApplyChoice(firstChoiceRoot, firstChoiceName, firstChoiceSymbol, firstChoiceDescription, first);
        ApplyChoice(secondChoiceRoot, secondChoiceName, secondChoiceSymbol, secondChoiceDescription, second);

        HideTooltip();
        choiceOverlay?.SetActive(true);
        ChoiceVisibilityChanged?.Invoke(true);
        choiceCompletion = new TaskCompletionSource<StageRelicDefinition>();
        return choiceCompletion.Task;
    }

    private StageRelicIconView GetOrCreateOwnedIcon(int index)
    {
        if (index < ownedIconPool.Count)
            return ownedIconPool[index];

        StageRelicIconView icon = Instantiate(ownedIconPrefab, ownedList);
        icon.name = $"OwnedRelic_{ownedIconPool.Count + 1}";
        ownedIconPool.Add(icon);
        return icon;
    }

    private void ShowTooltip(StageRelicDefinition relic, RectTransform icon)
    {
        if (tooltip == null || relic == null)
            return;

        tooltipName.text = relic.Name;
        tooltipDescription.text = relic.Description;
        tooltipRelic = relic;
        RectTransform tooltipRect = tooltip.transform as RectTransform;
        tooltipRect.SetParent(icon, false);
        tooltipRect.anchorMin = new Vector2(0.5f, 0.5f);
        tooltipRect.anchorMax = new Vector2(0.5f, 0.5f);
        tooltipRect.pivot = new Vector2(0.5f, 0.5f);
        const float gap = 12f;
        tooltipRect.anchoredPosition = new Vector2(
            -(icon.rect.width * 0.5f + tooltipRect.rect.width * 0.5f + gap),
            0f);
        tooltip.SetActive(true);
        tooltip.transform.SetAsLastSibling();
        KeepTooltipInsideCanvas(tooltipRect);
    }

    private void KeepTooltipInsideCanvas(RectTransform tooltipRect)
    {
        RectTransform canvasRect = transform as RectTransform;
        if (canvasRect == null || tooltipRect == null)
            return;

        Canvas.ForceUpdateCanvases();
        Bounds bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(canvasRect, tooltipRect);
        const float margin = 12f;
        Vector2 correction = Vector2.zero;

        if (bounds.min.x < canvasRect.rect.xMin + margin)
            correction.x = canvasRect.rect.xMin + margin - bounds.min.x;
        else if (bounds.max.x > canvasRect.rect.xMax - margin)
            correction.x = canvasRect.rect.xMax - margin - bounds.max.x;

        if (bounds.min.y < canvasRect.rect.yMin + margin)
            correction.y = canvasRect.rect.yMin + margin - bounds.min.y;
        else if (bounds.max.y > canvasRect.rect.yMax - margin)
            correction.y = canvasRect.rect.yMax - margin - bounds.max.y;

        tooltipRect.anchoredPosition += correction;
    }

    private void HideTooltip()
    {
        if (tooltip == null)
            return;

        tooltip.SetActive(false);
        tooltipRelic = null;
        if (tooltipHomeParent != null && tooltip.transform.parent != tooltipHomeParent)
            tooltip.transform.SetParent(tooltipHomeParent, false);
    }

    private static void ApplyChoice(
        GameObject root,
        TMP_Text nameLabel,
        Image symbolLabel,
        TMP_Text descriptionLabel,
        StageRelicDefinition relic)
    {
        if (root == null)
            return;

        root.SetActive(relic != null);
        if (relic == null)
            return;

        nameLabel.text = relic.Name;
        ApplyChoiceIcon(symbolLabel, relic);
        descriptionLabel.text = relic.Description;
    }

    private static void ApplyChoiceIcon(Image symbolLabel, StageRelicDefinition relic)
    {
        if (symbolLabel == null)
            return;

        symbolLabel.sprite = relic.Icon;
        symbolLabel.color = Color.white;
        symbolLabel.preserveAspect = true;
        symbolLabel.raycastTarget = false;
        symbolLabel.enabled = relic.Icon != null;
    }

    private void SelectFirstChoice() => SelectRelic(firstChoice);
    private void SelectSecondChoice() => SelectRelic(secondChoice);

    private void SelectRelic(StageRelicDefinition selected)
    {
        if (selected == null || choiceCompletion == null)
            return;

        choiceOverlay?.SetActive(false);
        ChoiceVisibilityChanged?.Invoke(false);
        TaskCompletionSource<StageRelicDefinition> completion = choiceCompletion;
        choiceCompletion = null;
        completion.TrySetResult(selected);
    }

    private void HideOwnedIcons()
    {
        foreach (StageRelicIconView icon in ownedIconPool)
            icon?.Clear();
    }

    private void OnDestroy()
    {
        LocalizationService.LocaleChanged -= HandleLocaleChanged;
        ChoiceVisibilityChanged?.Invoke(false);
        firstChoiceButton?.onClick.RemoveListener(SelectFirstChoice);
        secondChoiceButton?.onClick.RemoveListener(SelectSecondChoice);
        choiceCompletion?.TrySetCanceled();
    }

    private void OnDisable()
    {
        LocalizationService.LocaleChanged -= HandleLocaleChanged;
        ChoiceVisibilityChanged?.Invoke(false);
        HideTooltip();
    }

    private void HandleLocaleChanged(string _)
    {
        ApplyChoice(firstChoiceRoot, firstChoiceName, firstChoiceSymbol, firstChoiceDescription, firstChoice);
        ApplyChoice(secondChoiceRoot, secondChoiceName, secondChoiceSymbol, secondChoiceDescription, secondChoice);
        if (tooltipRelic != null && tooltip != null && tooltip.activeSelf)
        {
            tooltipName.text = tooltipRelic.Name;
            tooltipDescription.text = tooltipRelic.Description;
        }
    }
}
