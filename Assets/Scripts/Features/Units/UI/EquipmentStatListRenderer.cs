using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Reuses preconfigured stat-row views. The owning panel supplies display data only.
/// </summary>
public sealed class EquipmentStatListRenderer
{
    private readonly Transform root;
    private readonly EquipmentStatRowView template;
    private readonly List<EquipmentStatRowView> rowPool = new();

    public EquipmentStatListRenderer(Transform root, EquipmentStatRowView template)
    {
        this.root = root;
        this.template = template;

        if (template != null)
            template.gameObject.SetActive(false);
    }

    public void Render(IEnumerable<string> values)
    {
        HideRows();
        if (root == null || template == null || values == null)
            return;

        int rowIndex = 0;
        foreach (string value in values)
        {
            EquipmentStatRowView row = GetOrCreateRow(rowIndex++);
            row.SetText(value);
            row.gameObject.SetActive(true);
        }
    }

    private EquipmentStatRowView GetOrCreateRow(int index)
    {
        if (index < rowPool.Count)
            return rowPool[index];

        EquipmentStatRowView row = Object.Instantiate(template, root);
        row.name = $"EquipmentStatRow_{rowPool.Count + 1}";
        rowPool.Add(row);
        return row;
    }

    private void HideRows()
    {
        for (int i = 0; i < rowPool.Count; i++)
        {
            if (rowPool[i] != null)
                rowPool[i].gameObject.SetActive(false);
        }
    }
}
