using UnityEngine;
using UnityEngine.UI;

public sealed class LabSelectionFrame : MaskableGraphic
{
    [SerializeField, Min(1f)] private float thickness = 6f;

    protected override void OnPopulateMesh(VertexHelper vertexHelper)
    {
        vertexHelper.Clear();
        Rect outer = GetPixelAdjustedRect();
        Rect inner = new(
            outer.xMin + thickness,
            outer.yMin + thickness,
            Mathf.Max(0f, outer.width - thickness * 2f),
            Mathf.Max(0f, outer.height - thickness * 2f));

        AddVertex(vertexHelper, outer.xMin, outer.yMin);
        AddVertex(vertexHelper, outer.xMin, outer.yMax);
        AddVertex(vertexHelper, outer.xMax, outer.yMax);
        AddVertex(vertexHelper, outer.xMax, outer.yMin);
        AddVertex(vertexHelper, inner.xMin, inner.yMin);
        AddVertex(vertexHelper, inner.xMin, inner.yMax);
        AddVertex(vertexHelper, inner.xMax, inner.yMax);
        AddVertex(vertexHelper, inner.xMax, inner.yMin);

        AddQuad(vertexHelper, 0, 1, 5, 4);
        AddQuad(vertexHelper, 1, 2, 6, 5);
        AddQuad(vertexHelper, 2, 3, 7, 6);
        AddQuad(vertexHelper, 3, 0, 4, 7);
    }

    private void AddVertex(VertexHelper helper, float x, float y) =>
        helper.AddVert(new Vector3(x, y), color, Vector2.zero);

    private static void AddQuad(VertexHelper helper, int a, int b, int c, int d)
    {
        helper.AddTriangle(a, b, c);
        helper.AddTriangle(c, d, a);
    }
}
