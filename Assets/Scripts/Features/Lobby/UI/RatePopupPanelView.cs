using UnityEngine;

public class RatePopupPanelView : MonoBehaviour
{
    [SerializeField] private RateGroupView legendGroup;
    [SerializeField] private RateGroupView rareGroup;
    [SerializeField] private RateGroupView normalGroup;

    public void Open(GachaDataSO banner)
    {
        gameObject.SetActive(true);

        legendGroup.Setup("전설", banner.legendRate, banner.legendPool);

        rareGroup.Setup("희귀", banner.rareRate, banner.rarePool);

        normalGroup.Setup("일반", banner.normalRate, banner.normalPool);
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }
}