using Firebase.Firestore;

[System.Serializable]
[FirestoreData]
public class EquipmentItemData
{
    [FirestoreProperty] public string UniqueId { get; set; }
    [FirestoreProperty] public string ItemId { get; set; }
    [FirestoreProperty] public int Level { get; set; } = 1;
}
