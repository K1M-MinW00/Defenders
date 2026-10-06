using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Graphic))]
public sealed class LabCardGradient : BaseMeshEffect
{
    [SerializeField] private Color topColor = Color.white;
    [SerializeField] private Color bottomColor = Color.gray;

    public void SetColors(Color top, Color bottom)
    {
        topColor = top;
        bottomColor = bottom;
        graphic?.SetVerticesDirty();
    }

    public void SetRarity(LabCardRarity rarity)
    {
        switch (rarity)
        {
            case LabCardRarity.Rare:
                SetColors(new Color(0.72f, 0.38f, 1f), new Color(0.3f, 0.08f, 0.58f));
                break;
            case LabCardRarity.Legendary:
                SetColors(new Color(1f, 0.82f, 0.2f), new Color(0.9f, 0.38f, 0.02f));
                break;
            default:
                SetColors(new Color(0.3f, 0.82f, 1f), new Color(0.04f, 0.34f, 0.78f));
                break;
        }
    }

    public override void ModifyMesh(VertexHelper vertexHelper)
    {
        if (!IsActive() || vertexHelper.currentVertCount == 0)
            return;

        UIVertex vertex = default;
        float minY = float.MaxValue;
        float maxY = float.MinValue;
        for (int i = 0; i < vertexHelper.currentVertCount; i++)
        {
            vertexHelper.PopulateUIVertex(ref vertex, i);
            minY = Mathf.Min(minY, vertex.position.y);
            maxY = Mathf.Max(maxY, vertex.position.y);
        }

        float height = Mathf.Max(0.001f, maxY - minY);
        for (int i = 0; i < vertexHelper.currentVertCount; i++)
        {
            vertexHelper.PopulateUIVertex(ref vertex, i);
            vertex.color *= Color.Lerp(bottomColor, topColor, (vertex.position.y - minY) / height);
            vertexHelper.SetUIVertex(vertex, i);
        }
    }
}
