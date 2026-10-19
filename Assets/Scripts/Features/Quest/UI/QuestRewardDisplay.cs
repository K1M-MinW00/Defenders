using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class QuestRewardDisplay : MonoBehaviour
{
    [SerializeField] private RectTransform panel;
    [SerializeField] private Transform slotsRoot;
    [SerializeField] private TMP_Text title;
    [SerializeField] private Button backdropButton;
    [SerializeField] private CommonSlotUI slotPrefab;
    private readonly List<CommonSlotUI> slotPool = new();
    private bool showingAcquired;

    public void ShowPreview(IReadOnlyList<RewardData> rewards, RectTransform anchor)
    {
        Bind(rewards);
        if (panel == null || title == null) return;
        showingAcquired = false;
        RefreshTitle();
        gameObject.SetActive(true);
        transform.SetAsLastSibling();
        PositionNear(anchor);
        ClampToCanvas();
    }

    public void ShowAcquired(IReadOnlyList<RewardData> rewards)
    {
        Bind(rewards);
        if (panel == null || title == null) return;
        showingAcquired = true;
        RefreshTitle();
        gameObject.SetActive(true);
        transform.SetAsLastSibling();
        panel.anchorMin = panel.anchorMax = panel.pivot = new Vector2(.5f, .5f);
        panel.anchoredPosition = Vector2.zero;
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private void Awake()
    {
        backdropButton?.onClick.AddListener(Hide);
    }

    private void OnEnable()
    {
        LocalizationService.LocaleChanged += HandleLocaleChanged;
        RefreshTitle();
    }

    private void OnDisable()
    {
        LocalizationService.LocaleChanged -= HandleLocaleChanged;
    }

    private void OnDestroy()
    {
        LocalizationService.LocaleChanged -= HandleLocaleChanged;
        backdropButton?.onClick.RemoveListener(Hide);
    }

    private void HandleLocaleChanged(string _) => RefreshTitle();

    private void RefreshTitle()
    {
        if (title == null)
            return;

        title.text = LocalizationService.Get(
            LocalizationTables.Lobby,
            showingAcquired ? "quest.reward.acquired_title" : "quest.reward.preview_title");
    }

    private void Bind(IReadOnlyList<RewardData> rewards)
    {
        if (slotsRoot == null || slotPrefab == null) return;
        foreach (CommonSlotUI slot in slotPool)
            if (slot != null) slot.gameObject.SetActive(false);

        if (rewards == null) return;
        int visibleIndex = 0;
        foreach (RewardData reward in rewards)
        {
            if (!RewardPresentationResolver.TryResolve(reward, GameConfig.Icons, out RewardPresentation presentation))
                continue;
            CommonSlotUI slot = GetSlot(visibleIndex++);
            slot.Setup(presentation.Icon, presentation.Rarity, reward.Amount, true,
                () => ItemTooltipUI.Show(presentation.DisplayName, slot.transform as RectTransform), false);
            slot.gameObject.SetActive(true);
        }
    }

    private CommonSlotUI GetSlot(int index)
    {
        while (slotPool.Count <= index)
            slotPool.Add(Instantiate(slotPrefab, slotsRoot));
        return slotPool[index];
    }

    private void PositionNear(RectTransform anchor)
    {
        RectTransform canvasRect = transform as RectTransform;
        panel.anchorMin = panel.anchorMax = panel.pivot = new Vector2(.5f, .5f);
        if (anchor == null || canvasRect == null) { panel.anchoredPosition = Vector2.zero; return; }
        Vector2 local = canvasRect.InverseTransformPoint(anchor.position);
        panel.anchoredPosition = local + Vector2.down * 170f;
    }

    private void ClampToCanvas()
    {
        RectTransform canvasRect = transform as RectTransform;
        if (canvasRect == null) return;

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(panel);
        Bounds bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(canvasRect, panel);
        const float padding = 24f;
        float left = canvasRect.rect.xMin + padding;
        float right = canvasRect.rect.xMax - padding;
        float bottom = canvasRect.rect.yMin + padding;
        float top = canvasRect.rect.yMax - padding;
        Vector2 correction = Vector2.zero;

        if (bounds.min.x < left) correction.x = left - bounds.min.x;
        else if (bounds.max.x > right) correction.x = right - bounds.max.x;
        if (bounds.min.y < bottom) correction.y = bottom - bounds.min.y;
        else if (bounds.max.y > top) correction.y = top - bounds.max.y;

        panel.anchoredPosition += correction;
    }
}
