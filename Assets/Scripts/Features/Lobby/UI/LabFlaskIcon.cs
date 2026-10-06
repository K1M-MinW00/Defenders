using UnityEngine;
using UnityEngine.UI;

public sealed class LabFlaskIcon : MaskableGraphic
{
    protected override void OnPopulateMesh(VertexHelper vertexHelper)
    {
        vertexHelper.Clear();
        Rect rect = GetPixelAdjustedRect();
        Vector2[] points =
        {
            new(-0.18f, 0.5f), new(0.18f, 0.5f), new(0.18f, 0.12f),
            new(0.48f, -0.42f), new(0.38f, -0.5f), new(-0.38f, -0.5f),
            new(-0.48f, -0.42f), new(-0.18f, 0.12f),
        };

        vertexHelper.AddVert(rect.center, color, Vector2.zero);
        foreach (Vector2 point in points)
            vertexHelper.AddVert(rect.center + Vector2.Scale(point, rect.size), color, Vector2.zero);

        for (int i = 0; i < points.Length; i++)
            vertexHelper.AddTriangle(0, i + 1, ((i + 1) % points.Length) + 1);
    }
}
