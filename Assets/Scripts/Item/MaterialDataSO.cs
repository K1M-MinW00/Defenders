using UnityEngine;

[CreateAssetMenu(menuName = "Game/Item/Material")]
public class MaterialDataSO : ItemDataSO
{
    [Header("Material")]
    public int Value;

    public override ItemCategory Category => ItemCategory.Material;

    public override bool Stackable => true;
}