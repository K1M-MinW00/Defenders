using System.Collections.Generic;
using UnityEngine;

public class RecruitResultPopupView : MonoBehaviour
{
    [SerializeField] private Transform contentRoot;
    [SerializeField] private RecruitUnitIconView slotPrefab;

    public void Open(List<GachaResult> results)
    {
        gameObject.SetActive(true);

        foreach (Transform child in contentRoot)
        {
            Destroy(child.gameObject);
        }

        foreach (GachaResult result in results)
        {
            RecruitUnitIconView slot = Instantiate(slotPrefab, contentRoot);

            slot.Setup(result.Unit);
        }
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }
}