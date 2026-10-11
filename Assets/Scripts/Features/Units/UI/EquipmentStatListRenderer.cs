using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Renders the repeated framed rows used by equipment base and bonus summaries.
/// The owning panel supplies data only and no longer constructs UI objects itself.
/// </summary>
public sealed class EquipmentStatListRenderer
{
    private readonly Transform root;
    private readonly TMP_Text template;
    private readonly Sprite frameSprite;

    public EquipmentStatListRenderer(Transform root, TMP_Text template, Sprite frameSprite)
    {
        this.root = root;
        this.template = template;
        this.frameSprite = frameSprite;
    }

    public void Render(IEnumerable<string> values)
    {
        if (root == null || template == null)
            return;

        ClearRows();
        if (values != null)
        {
            foreach (string value in values)
                CreateRow(value);
        }

        template.gameObject.SetActive(false);
    }

    private void ClearRows()
    {
        for (int index = root.childCount - 1; index >= 0; index--)
        {
            Transform child = root.GetChild(index);
            if (child != template.transform)
                Object.Destroy(child.gameObject);
        }
    }

    private void CreateRow(string value)
    {
        GameObject row = new(
            "EquipmentStatRow",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image),
            typeof(LayoutElement));
        row.transform.SetParent(root, false);

        Image frame = row.GetComponent<Image>();
        frame.sprite = frameSprite;
        frame.type = Image.Type.Sliced;
        frame.color = Color.white;

        LayoutElement layout = row.GetComponent<LayoutElement>();
        layout.preferredHeight = 55f;
        layout.minHeight = 55f;

        TMP_Text label = Object.Instantiate(template, row.transform);
        RectTransform rect = (RectTransform)label.transform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(16f, 4f);
        rect.offsetMax = new Vector2(-16f, -4f);
        label.text = value;
        label.gameObject.SetActive(true);
    }
}
