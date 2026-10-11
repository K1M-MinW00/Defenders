using Firebase.Firestore;

[System.Serializable]
[FirestoreData]
public sealed class EquipmentBonusOptionData
{
    [FirestoreProperty] public EquipmentStatType StatType { get; set; }
    [FirestoreProperty] public EquipmentModifierType ModifierType { get; set; }
    [FirestoreProperty] public float Value { get; set; }
}
