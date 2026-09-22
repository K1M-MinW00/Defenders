using UnityEngine;

public class RatePopupPanelView : MonoBehaviour
{
    [SerializeField] private RateGroupView legendGroup;
    [SerializeField] private RateGroupView rareGroup;
    [SerializeField] private RateGroupView normalGroup;

    private Coroutine rebuildRoutine;

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
        legendGroup.Setup("전설", banner.LegendRate, banner.LegendPool);
        rareGroup.Setup("희귀", banner.RareRate, banner.RarePool);
        normalGroup.Setup("일반", banner.NormalRate, banner.NormalPool);

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
        legendGroup.RebuildLayout();
        rareGroup.RebuildLayout();
        normalGroup.RebuildLayout();

        if (transform is RectTransform popupRect)
            UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(popupRect);

        rebuildRoutine = null;
    }

    private void OnDisable()
    {
        rebuildRoutine = null;
    }
}
