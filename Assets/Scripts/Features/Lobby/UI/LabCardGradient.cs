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

    public void SetRarity(Rarity rarity)
    {
        switch (rarity)
        {
            case Rarity.Rare:
                SetFromBaseColor(RarityPalette.Rare);
                break;
            case Rarity.Epic:
                SetFromBaseColor(RarityPalette.Epic);
                break;
            case Rarity.Legend:
                SetFromBaseColor(RarityPalette.Legend);
                break;
            default:
                SetFromBaseColor(RarityPalette.Normal);
                break;
        }
    }

    private void SetFromBaseColor(Color color)
    {
        SetColors(
            Color.Lerp(color, Color.white, 0.18f),
            Color.Lerp(color, Color.black, 0.32f));
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
