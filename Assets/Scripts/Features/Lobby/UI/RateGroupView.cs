using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RateGroupView : MonoBehaviour
{
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private Transform contentRoot;
    [SerializeField] private RecruitUnitIconView slotPrefab;

    public void Setup(string rarityName, float rate, IReadOnlyList<UnitDataSO> units)
    {
        if (titleText != null)
            titleText.text = $"{rarityName} ({rate:0.##}%)";

        if (contentRoot == null)
        {
            Debug.LogError($"[RateGroupView] Content root is not assigned for {rarityName}.", this);
            return;
        }

        foreach (Transform child in contentRoot)
        {
            Destroy(child.gameObject);
        }

        if (slotPrefab == null)
        {
            Debug.LogError($"[RateGroupView] Slot prefab is not assigned for {rarityName}.", this);
            return;
        }

        if (units == null)
            return;

        foreach (UnitDataSO unit in units)
        {
            if (unit == null)
                continue;

            RecruitUnitIconView slot = Instantiate(slotPrefab, contentRoot);

            slot.Setup(unit);
        }

        RebuildLayout();
    }

    public void RebuildLayout()
    {
        if (contentRoot is not RectTransform contentRect)
            return;

        LayoutRebuilder.MarkLayoutForRebuild(contentRect);
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentRect);
    }
}
