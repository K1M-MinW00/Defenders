using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public sealed class StageRelicUI : MonoBehaviour
{
    [Header("Owned relics")]
    [SerializeField] private RectTransform ownedList;
    [SerializeField] private GameObject tooltip;
    [SerializeField] private TMP_Text tooltipName;
    [SerializeField] private TMP_Text tooltipDescription;

    [Header("Choice overlay")]
    [SerializeField] private GameObject choiceOverlay;
    [SerializeField] private GameObject firstChoiceRoot;
    [SerializeField] private TMP_Text firstChoiceName;
    [SerializeField] private TMP_Text firstChoiceSymbol;
    [SerializeField] private TMP_Text firstChoiceDescription;
    [SerializeField] private Button firstChoiceButton;
    [SerializeField] private GameObject secondChoiceRoot;
    [SerializeField] private TMP_Text secondChoiceName;
    [SerializeField] private TMP_Text secondChoiceSymbol;
    [SerializeField] private TMP_Text secondChoiceDescription;
    [SerializeField] private Button secondChoiceButton;

    private bool initialized;
    private StageRelicDefinition firstChoice;
    private StageRelicDefinition secondChoice;
    private TaskCompletionSource<StageRelicDefinition> choiceCompletion;
    private Transform tooltipHomeParent;

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
        for (int i = ownedList.childCount - 1; i >= 0; i--)
            Destroy(ownedList.GetChild(i).gameObject);

        if (relics == null)
            return;

        foreach (StageRelicDefinition relic in relics)
            CreateOwnedIcon(relic);
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
        choiceCompletion = new TaskCompletionSource<StageRelicDefinition>();
        return choiceCompletion.Task;
    }

    private void CreateOwnedIcon(StageRelicDefinition relic)
    {
        if (relic == null)
            return;

        var icon = new GameObject($"Relic_{relic.Id}", typeof(RectTransform), typeof(Image), typeof(LayoutElement), typeof(RelicTooltipPressHandler));
        icon.layer = gameObject.layer;
        icon.transform.SetParent(ownedList, false);

        var image = icon.GetComponent<Image>();
        image.color = new Color(0.04f, 0.24f, 0.26f, 0.96f);
        var layout = icon.GetComponent<LayoutElement>();
        layout.preferredWidth = 64f;
        layout.preferredHeight = 64f;

        var labelObject = new GameObject("Symbol", typeof(RectTransform), typeof(TextMeshProUGUI));
        labelObject.layer = gameObject.layer;
        labelObject.transform.SetParent(icon.transform, false);
        Stretch(labelObject.GetComponent<RectTransform>());
        var label = labelObject.GetComponent<TextMeshProUGUI>();
        label.text = relic.Symbol;
        label.fontSize = 30f;
        label.alignment = TextAlignmentOptions.Center;
        label.raycastTarget = false;

        icon.GetComponent<RelicTooltipPressHandler>().Initialize(
            () => ShowTooltip(relic, icon.GetComponent<RectTransform>()),
            HideTooltip);
    }

    private void ShowTooltip(StageRelicDefinition relic, RectTransform icon)
    {
        if (tooltip == null || relic == null)
            return;

        tooltipName.text = relic.Name;
        tooltipDescription.text = relic.Description;
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
        if (tooltipHomeParent != null && tooltip.transform.parent != tooltipHomeParent)
            tooltip.transform.SetParent(tooltipHomeParent, false);
    }

    private static void ApplyChoice(
        GameObject root,
        TMP_Text nameLabel,
        TMP_Text symbolLabel,
        TMP_Text descriptionLabel,
        StageRelicDefinition relic)
    {
        if (root == null)
            return;

        root.SetActive(relic != null);
        if (relic == null)
            return;

        nameLabel.text = relic.Name;
        symbolLabel.text = relic.Symbol;
        descriptionLabel.text = relic.Description;
    }

    private void SelectFirstChoice() => SelectRelic(firstChoice);
    private void SelectSecondChoice() => SelectRelic(secondChoice);

    private void SelectRelic(StageRelicDefinition selected)
    {
        if (selected == null || choiceCompletion == null)
            return;

        choiceOverlay?.SetActive(false);
        TaskCompletionSource<StageRelicDefinition> completion = choiceCompletion;
        choiceCompletion = null;
        completion.TrySetResult(selected);
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private void OnDestroy()
    {
        firstChoiceButton?.onClick.RemoveListener(SelectFirstChoice);
        secondChoiceButton?.onClick.RemoveListener(SelectSecondChoice);
        choiceCompletion?.TrySetCanceled();
    }
}

public sealed class RelicTooltipPressHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private System.Action onPressed;
    private System.Action onReleased;

    public void Initialize(System.Action pressed, System.Action released)
    {
        onPressed = pressed;
        onReleased = released;
    }

    public void OnPointerDown(PointerEventData eventData) => onPressed?.Invoke();

    public void OnPointerUp(PointerEventData eventData) => onReleased?.Invoke();

    private void OnDisable() => onReleased?.Invoke();
}
