using UnityEngine;

public class RatePopupPanelView : MonoBehaviour
{
    [SerializeField] private RateGroupView legendGroup;
    [SerializeField] private RateGroupView rareGroup;
    [SerializeField] private RateGroupView normalGroup;

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

        legendGroup.Setup("전설", banner.legendRate, banner.legendPool);
        rareGroup.Setup("희귀", banner.rareRate, banner.rarePool);
        normalGroup.Setup("일반", banner.normalRate, banner.normalPool);

        gameObject.SetActive(true);
        return true;
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }
}
