using UnityEngine;

[CreateAssetMenu(menuName = "Game/Item/Equipment")]
public class EquipmentDataSO : ItemDataSO
{
    [Header("Equipment")]
    public EquipmentType EquipmentType;

    public int Attack;
    public int Defense;

    public override ItemCategory Category => ItemCategory.Equipment;

    public override bool Stackable => false;
}

public enum EquipmentType
{
    None,
    Helmet,
    Armor,
    MainWeapon,
    SubWeapon
}
