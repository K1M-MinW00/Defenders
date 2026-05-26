using Firebase.Firestore;

[FirestoreData]
[System.Serializable]
public class InventoryStackItem
{
    [FirestoreProperty] public string ItemId { get; set; }
    [FirestoreProperty] public int Count { get; set; }
}
