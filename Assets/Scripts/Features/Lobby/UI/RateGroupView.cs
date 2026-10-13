using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class RateGroupView : MonoBehaviour
{
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private Transform contentRoot;
    [SerializeField] private RecruitUnitIconView slotPrefab;

    private readonly List<RecruitUnitIconView> slotPool = new();

    public void Setup(string label, float rate, IReadOnlyList<UnitDataSO> units)
    {
        if (titleText != null)
            titleText.text = $"{label} ({rate:0.##}%)";

        if (contentRoot == null)
        {
            Debug.LogError($"[RateGroupView] Content root is not assigned for {label}.", this);
            return;
        }

        HideAllSlots();

        if (slotPrefab == null)
        {
            Debug.LogError($"[RateGroupView] Slot prefab is not assigned for {label}.", this);
            return;
        }

        if (units == null)
            return;

        int slotIndex = 0;
        foreach (UnitDataSO unit in units)
        {
            if (unit == null)
                continue;

            RecruitUnitIconView slot = GetOrCreateSlot(slotIndex++);
            slot.Setup(unit);
            slot.ShowImmediately();
        }

        RebuildLayout();
    }

    private RecruitUnitIconView GetOrCreateSlot(int index)
    {
        if (index < slotPool.Count)
            return slotPool[index];

        RecruitUnitIconView slot = Instantiate(slotPrefab, contentRoot);
        slot.name = $"RateUnitSlot_{slotPool.Count + 1}";
        slotPool.Add(slot);
        return slot;
    }

    private void HideAllSlots()
    {
        for (int i = 0; i < slotPool.Count; i++)
            slotPool[i].Hide();
    }

    public void RebuildLayout()
    {
        if (contentRoot is not RectTransform contentRect)
            return;

        LayoutRebuilder.MarkLayoutForRebuild(contentRect);
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentRect);
    }
}
