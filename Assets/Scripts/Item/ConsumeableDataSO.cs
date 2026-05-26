using UnityEngine;

[CreateAssetMenu(menuName = "Game/Item/Consumable")]
public class ConsumableDataSO : ItemDataSO
{
    public override ItemCategory Category => ItemCategory.Consumable;

    public override bool Stackable => true;
}