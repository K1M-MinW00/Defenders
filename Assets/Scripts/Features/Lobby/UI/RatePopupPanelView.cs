using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class RatePopupPanelView : MonoBehaviour, IModalDismissHandler
{
    [Header("Controls")]
    [SerializeField] private Button closeButton;

    [Header("Rate Rows")]
    [Tooltip("Pre-placed rate rows in display order. Four rows cover Rare, Epic, Legend, plus one pickup split row.")]
    [SerializeField] private List<RateGroupView> groupViews = new();

    private Coroutine rebuildRoutine;
    private ModalOverlayLifecycle lifecycle;
    private ModalOverlayLifecycle Lifecycle => lifecycle ??= new ModalOverlayLifecycle(gameObject);

    private void Awake()
    {
        if (!ValidateReferences())
        {
            enabled = false;
            return;
        }

        closeButton.onClick.AddListener(Close);
    }

    public bool Open(GachaDataSO banner)
    {
        if (banner == null)
        {
            Debug.LogError("[RatePopupPanelView] Banner data is not assigned.", this);
            return false;
        }

        if (!ValidateReferences())
            return false;

        IReadOnlyList<RecruitRatePreviewRow> rows = RecruitRatePreviewBuilder.Build(banner);
        if (rows.Count > groupViews.Count)
        {
            Debug.LogError($"[RatePopupPanelView] {rows.Count} rows are required, but only {groupViews.Count} are assigned.", this);
            return false;
        }

        Lifecycle.Show();
        int firstSiblingIndex = GetFirstSiblingIndex();

        for (int i = 0; i < groupViews.Count; i++)
        {
            bool isUsed = i < rows.Count;
            RateGroupView group = groupViews[i];
            group.gameObject.SetActive(isUsed);

            if (!isUsed)
                continue;

            group.transform.SetSiblingIndex(firstSiblingIndex + i);
            RecruitRatePreviewRow row = rows[i];
            group.Setup(row.Label, row.TotalRate, row.Units);
        }

        if (rebuildRoutine != null)
            StopCoroutine(rebuildRoutine);

        rebuildRoutine = StartCoroutine(RebuildNextFrame());
        return true;
    }

    public void Close()
    {
        Lifecycle.Close();
    }

    public void Dismiss() => Close();

    private IEnumerator RebuildNextFrame()
    {
        yield return null;
        Canvas.ForceUpdateCanvases();
        foreach (RateGroupView group in groupViews)
        {
            if (group.gameObject.activeSelf)
                group.RebuildLayout();
        }

        if (transform is RectTransform popupRect)
            LayoutRebuilder.ForceRebuildLayoutImmediate(popupRect);

        rebuildRoutine = null;
    }

    private void OnDisable()
    {
        rebuildRoutine = null;
    }

    private bool ValidateReferences()
    {
        if (closeButton == null)
        {
            Debug.LogError("[RatePopupPanelView] Close button is not assigned.", this);
            return false;
        }

        if (groupViews == null || groupViews.Count == 0)
        {
            Debug.LogError("[RatePopupPanelView] Pre-placed rate groups are not assigned.", this);
            return false;
        }

        for (int i = 0; i < groupViews.Count; i++)
        {
            if (groupViews[i] != null)
                continue;

            Debug.LogError($"[RatePopupPanelView] Rate group {i} is not assigned.", this);
            return false;
        }

        return true;
    }

    private void OnDestroy()
    {
        if (closeButton != null)
            closeButton.onClick.RemoveListener(Close);
    }

    private int GetFirstSiblingIndex()
    {
        int firstIndex = int.MaxValue;
        foreach (RateGroupView group in groupViews)
            firstIndex = Mathf.Min(firstIndex, group.transform.GetSiblingIndex());

        return firstIndex == int.MaxValue ? 0 : firstIndex;
    }
}
