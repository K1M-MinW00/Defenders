using UnityEngine;

public class RatePopupPanelView : MonoBehaviour
{
    [SerializeField] private RateGroupView legendGroup;
    [SerializeField] private RateGroupView rareGroup;
    [SerializeField] private RateGroupView normalGroup;

    private readonly System.Collections.Generic.List<RateGroupView> groupViews = new();
    private Coroutine rebuildRoutine;

    private void Awake()
    {
        InitializeGroupViews();
        EnsureGroupCapacity(4);
    }

    public bool Open(GachaDataSO banner)
    {
        if (banner == null)
        {
            Debug.LogError("[RatePopupPanelView] Banner data is not assigned.", this);
            return false;
        }

        if (legendGroup == null || rareGroup == null || normalGroup == null)
        {
            Debug.LogError("[RatePopupPanelView] One or more rate groups are not assigned.", this);
            return false;
        }

        gameObject.SetActive(true);
        InitializeGroupViews();

        System.Collections.Generic.IReadOnlyList<RecruitRatePreviewRow> rows = RecruitRatePreviewBuilder.Build(banner);
        EnsureGroupCapacity(rows.Count);
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
        gameObject.SetActive(false);
    }

    private System.Collections.IEnumerator RebuildNextFrame()
    {
        yield return null;
        Canvas.ForceUpdateCanvases();
        foreach (RateGroupView group in groupViews)
        {
            if (group.gameObject.activeSelf)
                group.RebuildLayout();
        }

        if (transform is RectTransform popupRect)
            UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(popupRect);

        rebuildRoutine = null;
    }

    private void OnDisable()
    {
        rebuildRoutine = null;
    }

    private void InitializeGroupViews()
    {
        if (groupViews.Count > 0 || legendGroup == null || rareGroup == null || normalGroup == null)
            return;

        groupViews.Add(legendGroup);
        groupViews.Add(rareGroup);
        groupViews.Add(normalGroup);
    }

    private void EnsureGroupCapacity(int requiredCount)
    {
        if (groupViews.Count == 0)
            return;

        while (groupViews.Count < requiredCount)
        {
            RateGroupView template = groupViews[0];
            RateGroupView clone = Instantiate(template, template.transform.parent);
            clone.name = $"Rate_Group_Runtime_{groupViews.Count}";
            clone.gameObject.SetActive(false);
            groupViews.Add(clone);
        }
    }

    private int GetFirstSiblingIndex()
    {
        int firstIndex = int.MaxValue;
        foreach (RateGroupView group in groupViews)
            firstIndex = Mathf.Min(firstIndex, group.transform.GetSiblingIndex());

        return firstIndex == int.MaxValue ? 0 : firstIndex;
    }
}
