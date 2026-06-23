using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RateGroupView : MonoBehaviour
{
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private Transform contentRoot;
    [SerializeField] private RecruitUnitIconView slotPrefab;

    public void Setup(string rarityName, float rate, List<UnitDataSO> units)
    {
        titleText.text = $"{rarityName} ({rate}%)";

        foreach (Transform child in contentRoot)
        {
            Destroy(child.gameObject);
        }

        foreach (UnitDataSO unit in units)
        {
            RecruitUnitIconView slot = Instantiate(slotPrefab, contentRoot);

            slot.Setup(unit);
        }

        Canvas.ForceUpdateCanvases();

        LayoutRebuilder.ForceRebuildLayoutImmediate(
            contentRoot as RectTransform);
    }
}