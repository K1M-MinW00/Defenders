using UnityEngine;

[CreateAssetMenu(menuName = "Game/Item/Equipment")]
public class EquipmentDataSO : ItemDataSO
{
    [Header("Equipment")]
    public EquipmentType EquipmentType;
    [Tooltip("0강 장비의 고정 옵션 종류입니다.")]
    public EquipmentStatType BaseStatType;
    [Min(0f)] public float BaseStatValue;

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
