using Firebase.Firestore;

[System.Serializable]
[FirestoreData]
public sealed class EquipmentLoadout
{
    [FirestoreProperty] public string HelmetId { get; set; }
    [FirestoreProperty] public string ArmorId { get; set; }
    [FirestoreProperty] public string MainWeaponId { get; set; }
    [FirestoreProperty] public string SubWeaponId { get; set; }

    public string Get(EquipmentType type) => type switch
    {
        EquipmentType.Helmet => HelmetId,
        EquipmentType.Armor => ArmorId,
        EquipmentType.MainWeapon => MainWeaponId,
        EquipmentType.SubWeapon => SubWeaponId,
        _ => null,
    };

    public void Set(EquipmentType type, string uniqueId)
    {
        switch (type)
        {
            case EquipmentType.Helmet: HelmetId = uniqueId; break;
            case EquipmentType.Armor: ArmorId = uniqueId; break;
            case EquipmentType.MainWeapon: MainWeaponId = uniqueId; break;
            case EquipmentType.SubWeapon: SubWeaponId = uniqueId; break;
        }
    }

    public System.Collections.Generic.IEnumerable<string> EnumerateIds()
    {
        if (!string.IsNullOrWhiteSpace(HelmetId)) yield return HelmetId;
        if (!string.IsNullOrWhiteSpace(ArmorId)) yield return ArmorId;
        if (!string.IsNullOrWhiteSpace(MainWeaponId)) yield return MainWeaponId;
        if (!string.IsNullOrWhiteSpace(SubWeaponId)) yield return SubWeaponId;
    }
}
