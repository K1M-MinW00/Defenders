using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
[RequireComponent(typeof(GridLayoutGroup))]
public sealed class ResponsiveSquareGrid : MonoBehaviour
{
    [SerializeField, Min(1)] private int columnCount = 5;
    [SerializeField, Min(1f)] private float minimumCellSize = 1f;
    [SerializeField, Min(1f)] private float maximumCellSize = 10000f;
    private GridLayoutGroup grid;
    private float lastWidth = -1f;

    private void OnEnable() => Refresh();
    private void OnRectTransformDimensionsChange() => Refresh();
#if UNITY_EDITOR
    private void Update() { if(!Application.isPlaying)Refresh(); }
#endif
    public void Configure(int columns) { columnCount=Mathf.Max(1,columns);Refresh(); }
    public void Configure(int columns, float maxCellSize)
    {
        columnCount = Mathf.Max(1, columns);
        maximumCellSize = Mathf.Max(minimumCellSize, maxCellSize);
        lastWidth = -1f;
        Refresh();
    }

    public void Refresh()
    {
        grid ??= GetComponent<GridLayoutGroup>();
        RectTransform rect=transform as RectTransform;if(grid==null||rect==null)return;
        float width=rect.rect.width;if(width<=0f||Mathf.Approximately(width,lastWidth))return;lastWidth=width;
        float usable=width-grid.padding.left-grid.padding.right-grid.spacing.x*(columnCount-1);
        float size=Mathf.Clamp(usable/columnCount,minimumCellSize,maximumCellSize);
        grid.constraint=GridLayoutGroup.Constraint.FixedColumnCount;grid.constraintCount=columnCount;
        grid.childAlignment=TextAnchor.UpperLeft;grid.cellSize=new Vector2(size,size);
    }
}
