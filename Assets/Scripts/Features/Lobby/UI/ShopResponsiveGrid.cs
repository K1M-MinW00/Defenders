using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
[RequireComponent(typeof(RectTransform), typeof(GridLayoutGroup))]
public sealed class ShopResponsiveGrid : MonoBehaviour
{
    [SerializeField, Min(1)] private int columns = 2;
    [SerializeField, Min(0.1f)] private float cardAspectRatio = 1.18f;
    [SerializeField, Min(0f)] private float horizontalSpacing = 20f;
    [SerializeField, Min(0f)] private float verticalSpacing = 20f;
    [SerializeField, Min(0f)] private float horizontalPadding = 4f;
    [SerializeField, Min(1f)] private float maxCellWidth = 400f;

    private RectTransform rectTransform;
    private GridLayoutGroup grid;
    private float appliedWidth = -1f;

    private void Awake()
    {
        CacheComponents();
        ApplyLayout(true);
    }

    private void OnEnable()
    {
        ApplyLayout(true);
    }

    private void OnRectTransformDimensionsChange()
    {
        ApplyLayout(false);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        ApplyLayout(true);
    }
#endif

    private void CacheComponents()
    {
        rectTransform ??= GetComponent<RectTransform>();
        grid ??= GetComponent<GridLayoutGroup>();
    }

    private void ApplyLayout(bool force)
    {
        CacheComponents();
        if (rectTransform == null || grid == null)
            return;

        float width = rectTransform.rect.width;
        if (width <= 0f || (!force && Mathf.Approximately(width, appliedWidth)))
            return;

        appliedWidth = width;
        float usableWidth = Mathf.Max(1f,
            width - horizontalPadding * 2f - horizontalSpacing * (columns - 1));
        // 기준 해상도보다 넓더라도 카드 자체를 확대하지 않는다. 좁은 화면에서만 축소한다.
        float cellWidth = Mathf.Min(usableWidth / columns, maxCellWidth);

        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = columns;
        grid.spacing = new Vector2(horizontalSpacing, verticalSpacing);
        grid.padding = new RectOffset(
            Mathf.RoundToInt(horizontalPadding),
            Mathf.RoundToInt(horizontalPadding),
            0,
            Mathf.RoundToInt(verticalSpacing));
        grid.cellSize = new Vector2(cellWidth, cellWidth / cardAspectRatio);
        grid.childAlignment = TextAnchor.UpperCenter;

        LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
    }
}
